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
	                                    convert(bit, CASE WHEN c.[nulls] = 'Y' THEN 1 ELSE 0 END) AllowDBNull,
	                                    convert(bit, CASE WHEN c.[default] = 'AUTOINCREMENT' THEN 1 ELSE 0 END) IsIdentity,
	                                    convert(bit, CASE WHEN c.[default] = 'NEWID()' THEN 1 ELSE 0 END) IsGenerated,
                                        convert(bit, CASE WHEN fcol.foreign_column_id is null THEN 0 ELSE 1 END) IsForeignKey,
                                        ISNULL(fk.[role],'') ConstraintName
                                    FROM sys.syscolumn c 
	                                    inner join sys.systable t on t.table_id = c.table_id
	                                    inner join sys.sysdomain d on c.domain_id = d.domain_id
                                        inner join sysobjects o on t.object_id = o.id
                                        inner join sys.sysuser u on u.user_id = o.uid
                                        left join sysfkcol fcol ON c.table_id = fcol.foreign_table_id and c.column_id = fcol.foreign_column_id
                                        left join sysforeignkey fk ON fcol.foreign_table_id = fk.foreign_table_id AND fcol.foreign_key_id = fk.foreign_key_id
                                    WHERE t.table_name = '{0}' 
                                    and u.user_name = '{1}'
                                    ORDER BY c.Column_id";
                        tableDetailsCommand.CommandText =
                            string.Format(sqlText,table.Name, owner);

                        sqlCon.Open();

                        var dr = tableDetailsCommand.ExecuteReader(CommandBehavior.Default);

                        var m = new DataTypeMapper();

                        while (dr.Read())
                        {
                            var name = dr["columnName"].ToString();
                            var isNullable = (bool) dr["AllowDBNull"];
                            var isPrimaryKey = dr["IsKey"] as bool?;
                            var isForeignKey = (bool)dr["IsForeignKey"];
                            var dataType = dr["DataTypeName"].ToString();
                            var dataLength = Convert.ToInt32(dr["columnSize"]);
                            var dataPrecision = Convert.ToInt32(dr["columnSize"]);
                            var dataScale = Convert.ToInt32(dr["columnPrecision"]);
                            var isIdentity = dr["IsIdentity"] as bool?;
                            var constraintName = dr["ConstraintName"].ToString();
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
                                        IsForeignKey = isForeignKey,
                                        MappedDataType = mappedType.ToString(),
                                        DataLength = dataLength,
                                        DataScale = dataScale,
                                        DataPrecision = dataPrecision,
                                        DataType = dataType,
                                        IsUnique = isUnique,
                                        IsIdentity = isIdentity.GetValueOrDefault(),
                                        ConstraintName = constraintName
                                    });

                        }

                        dr.Close();
                    }

                    table.Owner = owner;
                    table.Columns = columns;
                    table.PrimaryKey = DeterminePrimaryKeys(table);

                    // Need to find the table name associated with the FK
                    foreach (var c in table.Columns)
                    {
                        c.ForeignKeyTableName = GetForeignKeyReferenceTableName(table.Name, c.Name);
                    }
                    table.ForeignKeys = DetermineForeignKeyReferences(table);
                    table.HasManyRelationships = DetermineHasManyRelationships(table);
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
                                tables.Add(new Table {Name = sqlDataReader.GetString(0).Replace("'", string.Empty)});
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
                                  Columns = { c }
                              };
                return key;
            }

            if (primaryKeys.Count() > 1)
            {
                // Composite key
                var key = new PrimaryKey
                              {
                                  Type = PrimaryKeyType.CompositeKey,
                                  Columns = primaryKeys
                              };
                return key;
            }

            return null;
        }

        public IList<ForeignKey> DetermineForeignKeyReferences(Table table)
        {
            var foreignKeys = (from c in table.Columns
                               where c.IsForeignKey
                               group c by new { c.ConstraintName, c.ForeignKeyTableName } into g
                               select new ForeignKey
                                   {
                                       Name = g.Key.ConstraintName,
                                       References = g.Key.ForeignKeyTableName,
                                       Columns = g.ToList(), 
                                       UniquePropertyName = g.Key.ForeignKeyTableName
                                   }).ToList();

            Table.SetUniqueNamesForForeignKeyProperties(foreignKeys);

            return foreignKeys;
        }

        private string GetForeignKeyReferenceTableName(string selectedTableName, string columnName)
        {
            object referencedTableName;

            var conn = new OleDbConnection(_connectionStr);
            conn.Open();
            try
            {
                using (conn)
                {
                    OleDbCommand tableCommand = conn.CreateCommand();
                    tableCommand.CommandText = String.Format(
                        @"
						SELECT pt.table_name PK_TABLE
	                         , t.Table_name FK_TABLE
	                         , fc.column_name FK_COLUMN_NAME
	                         , fk.role CONSTRAINT_NAME
                             , pc.column_name PK_COLUMN_NAME
                        FROM
	                        SYSFOREIGNKEY fk
	                        INNER JOIN systable t ON t.table_id = fk.foreign_table_id
	                        INNER JOIN sysfkcol fcol ON fcol.foreign_table_id = fk.foreign_table_id AND fcol.foreign_key_id = fk.foreign_key_id
	                        INNER JOIN syscolumn fc	ON fc.column_id = fcol.foreign_column_id AND fc.Table_id = fcol.foreign_table_id
	                        INNER JOIN systable pt ON pt.table_id = fk.primary_table_id
                            INNER JOIN syscolumn pc on pc.column_id = fcol.primary_column_id and pc.Table_id = fk.primary_table_id
                        WHERE t.Table_name = '{0}' AND fc.column_name = '{1}'",
                        selectedTableName, columnName);
                    referencedTableName = tableCommand.ExecuteScalar();
                }
            }
            finally
            {
                conn.Close();
            }
            return (string)referencedTableName;
        }


        private IList<HasMany> DetermineHasManyRelationships(Table table)
        {
            var hasManyRelationships = new List<HasMany>();
            var conn = new OleDbConnection(_connectionStr);
            conn.Open();
            try
            {
                using (conn)
                {
                    using (var command = new OleDbCommand())
                    {
                        command.Connection = conn;
                        command.CommandText =
                            String.Format(
                                @"
						SELECT pt.table_name PK_TABLE
	                         , t.Table_name FK_TABLE
	                         , fc.column_name FK_COLUMN_NAME
	                         , fk.role CONSTRAINT_NAME
                        FROM
	                        SYSFOREIGNKEY fk
	                        INNER JOIN systable t ON t.table_id = fk.foreign_table_id
	                        INNER JOIN sysfkcol fcol ON fcol.foreign_table_id = fk.foreign_table_id AND fcol.foreign_key_id = fk.foreign_key_id
	                        INNER JOIN syscolumn fc	ON fc.column_id = fcol.foreign_column_id AND fc.Table_id = fcol.foreign_table_id
	                        INNER JOIN systable pt ON pt.table_id = fk.primary_table_id
                        WHERE pt.table_name = '{0}'
                        ORDER BY 1, 2",
                                table.Name);
                        var reader = command.ExecuteReader();

                        while (reader.Read())
                        {
                            var constraintName = reader["CONSTRAINT_NAME"].ToString();
                            var fkColumnName = reader["FK_COLUMN_NAME"].ToString();
                            var existing = hasManyRelationships.FirstOrDefault(hm => hm.ConstraintName == constraintName);
                            if (existing == null)
                            {
                                var newHasManyItem = new HasMany
                                {
                                    ConstraintName = constraintName,
                                    Reference = reader.GetString(1)
                                };
                                newHasManyItem.AllReferenceColumns.Add(fkColumnName);
                                hasManyRelationships.Add(newHasManyItem);

                            }
                            else
                            {
                                existing.AllReferenceColumns.Add(fkColumnName);
                            }
                        }
                    }
                }
            }
            finally
            {
                conn.Close();
            }
            return hasManyRelationships;
        }
    }

}