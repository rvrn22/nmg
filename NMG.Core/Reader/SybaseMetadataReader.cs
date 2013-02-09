using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SQLite;
using System.Linq;
using NMG.Core.Domain;

namespace NMG.Core.Reader
{
    public class SybaseMetadataReader : IMetadataReader
    {
        private readonly string _connectionStr;

        public SybaseMetadataReader(string connectionStr)
        {
            _connectionStr = connectionStr;
        }

        public IList<Column> GetTableDetails(Table table, string owner)
        {
            var columns = new List<Column>();

            using (var sqlCon = new OleDbConnection(_connectionStr))
            {
                try
                {
                    using (var tableDetailsCommand = sqlCon.CreateCommand())
                    {
                        var sqlText = @"SELECT 
    [default] default_value, c.column_name columnName, 
    d.domain_name DataTypeName, width ColumnSize, scale columnPrecision,
    convert(bit, CASE WHEN pkey = 'Y' THEN 1 ELSE 0 END) IsKey, 
    convert(bit, CASE WHEN nulls = 'Y' THEN 1 ELSE 0 END) AllowDBNull,
    convert(bit, CASE WHEN [default] = 'AUTOINCREMENT' THEN 1 ELSE 0 END) IsIdentity,
    convert(bit, CASE WHEN [default] = 'NEWID()' THEN 1 ELSE 0 END) IsGenerated
FROM sys.syscolumn c 
    inner join sys.systable t on t.table_id = c.table_id
    inner join sys.sysdomain d on c.domain_id = d.domain_id
inner join sysobjects o on t.object_id = o.id
inner join sys.sysuser u on u.user_id = o.uid
WHERE t.table_name = '{0}' 
and u.user_name = '{1}'";
                        tableDetailsCommand.CommandText =
                            string.Format(sqlText,
                                table.Name.Replace("'", string.Empty), owner);

                        sqlCon.Open();

                        var dr = tableDetailsCommand.ExecuteReader(CommandBehavior.Default);

                        var m = new DataTypeMapper();

                        while (dr.Read())
                        {
                            var name = dr["columnName"].ToString();
                            var isNullable = (bool) dr["AllowDBNull"];
                            var isPrimaryKey = dr["IsKey"] as bool?;

                            var dataType = dr["DataTypeName"].ToString();
                            var dataLength = Convert.ToInt32(dr["columnSize"]);
                            var dataPrecision = Convert.ToInt32(dr["columnSize"]);
                            var dataScale = Convert.ToInt32(dr["columnPrecision"]);
                            var isIdentity = dr["IsIdentity"] as bool?;
                            var isUnique = false; //(bool) dr["IsKey"];
                            var mappedType = m.MapFromDBType(ServerType.Sybase, dataType, dataLength, dataPrecision,
                                                             dataScale);

                            if (DataTypeMapper.IsNumericType(mappedType))
                            {
                                dataLength = 0;
                            }
                            else
                            {
                                dataScale = 0;
                                dataPrecision = 0;
                            }

                            columns.Add(
                                new Column
                                    {
                                        Name = name,
                                        IsNullable = isNullable,
                                        IsPrimaryKey = isPrimaryKey.GetValueOrDefault(),
                                        MappedDataType = mappedType.ToString(),
                                        DataLength = dataLength,
                                        DataScale = dataScale,
                                        DataPrecision = dataPrecision,
                                        DataType = dataType,
                                        IsUnique = isUnique,
                                        IsIdentity = isIdentity.GetValueOrDefault()
                                    });

                        }

                        dr.Close();
                    }

                    table.Owner = owner;
                    table.Columns = columns;
                    table.PrimaryKey = DeterminePrimaryKeys(table);
                    table.ForeignKeys = new List<ForeignKey>(); // DetermineForeignKeyReferences(table);
                    table.HasManyRelationships = new List<HasMany>(); // DetermineHasManyRelationships(table);
                }
                catch (Exception ex)
                {
                    Console.Write(ex.Message);
                }
                finally
                {
                    sqlCon.Close();
                }
            }

            return columns;
        }

        public List<Table> GetTables(string owner)
        {
            var tables = new List<Table>();

            using (var sqlCon = new OleDbConnection(_connectionStr))
            {
                sqlCon.Open();
                try
                {
                    using (var tableDetailsCommand = sqlCon.CreateCommand())
                    {
                        tableDetailsCommand.CommandText =
                            "SELECT table_name FROM sys.systable WHERE table_type in ('base', 'view') and not (table_name like 'SYS%' or table_name like 'ISYS%') order by 1";
                        using (var sqlDataReader = tableDetailsCommand.ExecuteReader(CommandBehavior.Default))
                        {
                            while (sqlDataReader.Read())
                            {
                                tables.Add(new Table {Name = sqlDataReader.GetString(0)});
                            }
                        }
                    }
                }
                finally
                {
                    sqlCon.Close();
                }
            }

            return tables;
        }

        public IList<string> GetOwners()
        {
            var owners = new List<string>();

            using (var sqlCon = new OleDbConnection(_connectionStr))
            {
                sqlCon.Open();
                try
                {
                    using (var tableDetailsCommand = sqlCon.CreateCommand())
                    {
                        tableDetailsCommand.CommandText =
                            "SELECT distinct b.name FROM sysobjects a, sysusers b WHERE a.type IN ('U', 'S') AND a.uid = b.uid ORDER BY b.name";
                        using (var sqlDataReader = tableDetailsCommand.ExecuteReader(CommandBehavior.Default))
                        {
                            while (sqlDataReader.Read())
                            {
                                owners.Add(sqlDataReader.GetString(0));
                            }
                        }
                    }
                }
                finally
                {
                    sqlCon.Close();
                }
            }

            return owners;
        }

        public List<string> GetSequences(string owner)
        {
            return new List<string>();
        }

        public PrimaryKey DeterminePrimaryKeys(Table table)
        {
            IList<Column> primaryKeys = table.Columns.Where(x => x.IsPrimaryKey.Equals(true)).ToList();

            if (primaryKeys.Count() == 1)
            {
                var c = primaryKeys.First();
                var key = new PrimaryKey
                              {
                                  Type = PrimaryKeyType.PrimaryKey,
                                  Columns =
                                      {
                                          new Column
                                              {
                                                  DataType = c.DataType,
                                                  Name = c.Name,
                                                  IsIdentity = c.IsIdentity
                                              }
                                      }
                              };
                return key;
            }

            if (primaryKeys.Count() > 1)
            {
                var key = new PrimaryKey
                              {
                                  Type = PrimaryKeyType.CompositeKey
                              };
                foreach (var primaryKey in primaryKeys)
                {
                    key.Columns.Add(new Column
                                        {
                                            DataType = primaryKey.DataType,
                                            Name = primaryKey.Name
                                        });
                }
                return key;
            }


            return null;
        }
    }

    public class SybaseDataType
    {
        public SybaseDataType(string sybaseType)
        {
            if (sybaseType.Contains("("))
            {
                var typeSplit = sybaseType.Replace(")", string.Empty).Split('(');
                DataType = typeSplit[0];
                DataLength = int.Parse(typeSplit[1]);
            }
        }
        public string DataType { get; set; }
        public int? DataLength { get; set; }
    }
}