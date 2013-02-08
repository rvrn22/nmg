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
                        tableDetailsCommand.CommandText =
                            string.Format(
                                "select default_value, cname columnName, colType DataTypeName, length ColumnSize, convert(bit, CASE WHEN in_primary_key = 'Y' THEN 1 ELSE 0 END) IsKey, convert(bit, CASE WHEN nulls = 'Y' THEN 1 ELSE 0 END) AllowDBNull from sys.syscolumns where tname = '{0}'",
                                table.Name.Replace("'", string.Empty));

                        sqlCon.Open();

                        var dr = tableDetailsCommand.ExecuteReader(CommandBehavior.Default);

                        var m = new DataTypeMapper();

                        while (dr.Read())
                        {
                            var name = dr["ColumnName"].ToString();
                            var isNullable = (bool) dr["AllowDBNull"];
                            var isPrimaryKey = (bool) dr["IsKey"];

                            var dataType = dr["DataTypeName"].ToString();
                            var dataLength = Convert.ToInt32(dr["ColumnSize"]);
                            var isIdentity = dr["default_value"].ToString() == "autoincrement";
                            var isUnique = false; //(bool) dr["IsKey"];

                            columns.Add(
                                new Column
                                    {
                                        Name = name,
                                        IsNullable = isNullable,
                                        IsPrimaryKey = isPrimaryKey,
                                        MappedDataType =
                                            m.MapFromDBType(ServerType.Sybase, dataType, dataLength, null, null)
                                             .ToString(),
                                        DataLength = dataLength,
                                        DataType = dataType,
                                        IsUnique = isUnique,
                                        IsIdentity = isIdentity
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
            return new List<string> {"dba"};
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