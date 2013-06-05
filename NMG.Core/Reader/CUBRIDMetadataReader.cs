using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CUBRID.Data.CUBRIDClient;
using NMG.Core.Domain;

namespace NMG.Core.Reader
{
    /// <summary>
    /// CUBRID implementation of the IMetadataReader interface
    /// http://www.cubrid.org
    /// NMG support - v1.0
    /// </summary>
    public class CUBRIDMetadataReader : IMetadataReader
    {
        private readonly string connectionStr;

        public CUBRIDMetadataReader(string connectionStr)
        {
            this.connectionStr = connectionStr;
        }

        public IList<string> GetOwners()
        {
            var owners = new List<string>();
            var conn = new CUBRIDConnection(connectionStr);
            conn.Open();

            try
            {
                using (conn)
                {
                    var schema = new CUBRIDSchemaProvider(conn);
                    DataTable dt = schema.GetUsers(new[] { "%" });
                    for (var i = 0; i < dt.Rows.Count; i++)
                    {
                        owners.Add(dt.Rows[i][0].ToString().ToLower());
                    }
                }
            }
            finally
            {
                conn.Close();
            }

            return owners;
        }

        public List<Table> GetTables(string owner)
        {
            var tables = new List<Table>();
            var conn = new CUBRIDConnection(connectionStr);
            conn.Open();

            try
            {
                using (conn)
                {
                    var schema = new CUBRIDSchemaProvider(conn);
                    DataTable dt = schema.GetTables(new[] { "%" });
                    for (var i = 0; i < dt.Rows.Count; i++)
                    {
                        tables.Add(new Table { Name = dt.Rows[i][2].ToString() });
                    }
                }
            }
            finally
            {
                conn.Close();
            }

            tables.Sort((x, y) => String.Compare(x.Name, y.Name, StringComparison.Ordinal));

            return tables;
        }

        public List<string> GetSequences(string owner)
        {
            return null;
        }

        public List<string> GetSequences(string tablename, string column)
        {
            var sequences = new List<string>();
            var conn = new CUBRIDConnection(connectionStr);
            conn.Open();

            try
            {
                using (conn)
                {
                    CUBRIDCommand seqCommand = conn.CreateCommand();
                    string sqlCmd = String.Format(@"select [name] from db_serial where class_name='{0}' and att_name='{1}'",
                                                tablename,
                                                column);
                    seqCommand.CommandText = sqlCmd;
                    var seqReader = (CUBRIDDataReader)seqCommand.ExecuteReader(CommandBehavior.CloseConnection);
                    while (seqReader.Read())
                    {
                        sequences.Add(seqReader.GetString(0));
                    }
                }
            }
            finally
            {
                conn.Close();
            }

            return sequences;
        }

        public List<string> GetSequences(List<Table> tables)
        {
            var sequences = new List<string>();
            var conn = new CUBRIDConnection(connectionStr);
            conn.Open();

            try
            {
                using (conn)
                {
                    CUBRIDCommand seqCommand = conn.CreateCommand();
                    seqCommand.CommandText = "select [name], class_name from db_serial";
                    var seqReader = (CUBRIDDataReader)seqCommand.ExecuteReader(CommandBehavior.CloseConnection);
                    while (seqReader.Read())
                    {
                        sequences.AddRange(from table in tables where table.Name.ToUpper() == seqReader.GetString(1).ToUpper() select seqReader.GetString(0));
                    }
                }
            }
            finally
            {
                conn.Close();
            }

            return sequences;
        }

        private IList<HasMany> DetermineHasManyRelationships(Table table)
        {
            var hasManyRelationships = new List<HasMany>();
            var conn = new CUBRIDConnection(connectionStr);
            conn.Open();

            try
            {
                using (conn)
                {
                    var schema = new CUBRIDSchemaProvider(conn);
                    DataTable dt = schema.GetForeignKeys(new[] { "%" });
                    for (var i = 0; i < dt.Rows.Count; i++)
                    {
                      if (dt.Rows[i]["PKTABLE_NAME"].ToString() == table.Name)
                      {
                        var newHasManyItem = new HasMany
                        {
                          Reference = dt.Rows[i]["FKTABLE_NAME"].ToString(),
                          ConstraintName = dt.Rows[i]["FK_NAME"].ToString(),
                          ReferenceColumn = dt.Rows[i]["FKCOLUMN_NAME"].ToString()
                        };
                        hasManyRelationships.Add(newHasManyItem);
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

        public IList<Column> GetTableDetails(Table table, string owner)
        {
            var columns = new List<Column>();
            var conn = new CUBRIDConnection(connectionStr);
            conn.Open();
            try
            {
                using (conn)
                {
                    var schema = new CUBRIDSchemaProvider(conn);
                    DataTable dt_fk = schema.GetForeignKeys(new[] { table.Name.ToLower() });

                    string sqlInfo = String.Format("select * from [{0}] limit 1", table.Name.ToLower());
                    var adapter = new CUBRIDDataAdapter(sqlInfo, conn);
                    var tableInfo = new DataTable();
                    adapter.FillSchema(tableInfo, SchemaType.Source);

                    using (var reader = new DataTableReader(tableInfo))
                    {
                        DataTable schemaTable = reader.GetSchemaTable();
                        for (var k = 0; k < schemaTable.Rows.Count; k++)
                        {
                            string columnName = schemaTable.Rows[k]["ColumnName"].ToString().ToLower();
                            var isUnique = (bool)schemaTable.Rows[k]["IsUnique"];
                            var isNullable = (bool)schemaTable.Rows[k]["AllowDBNull"];
                            var isPrimaryKey = (bool)schemaTable.Rows[k]["IsKey"];
                            var isIdentity = (bool)schemaTable.Rows[k]["IsAutoIncrement"];
                            var dataLength = (int)schemaTable.Rows[k]["ColumnSize"];
                            int dataPrecision = 0;
                            if (schemaTable.Rows[k]["NumericPrecision"].ToString() != String.Empty)
                            {
                                dataPrecision = (int)schemaTable.Rows[k]["NumericPrecision"];
                            }
                            int dataScale = 0;
                            if (schemaTable.Rows[k]["NumericScale"].ToString() != String.Empty)
                            {
                                dataScale = (int)schemaTable.Rows[k]["NumericScale"];
                            }
                            bool isForeignKey = false;
                            string fkTableName = "";
                            string constraintName = "";
                            for (var i_fk = 0; i_fk < dt_fk.Rows.Count; i_fk++)
                            {
                                if (dt_fk.Rows[i_fk]["FKCOLUMN_NAME"].ToString().ToLower() == columnName)
                                {
                                    isForeignKey = true;
                                    fkTableName = dt_fk.Rows[i_fk]["PKTABLE_NAME"].ToString();
                                    constraintName = dt_fk.Rows[i_fk]["FK_NAME"].ToString();
                                    break;
                                }
                            }
                            string dataType;
                            using (var cmd = new CUBRIDCommand(sqlInfo, conn))
                            {
                              using (var CUBRIDReader = (CUBRIDDataReader)cmd.ExecuteReader())
                              {
                                CUBRIDReader.Read();
                                dataType = CUBRIDReader.GetColumnTypeName(k);
                              }
                            }
                            var m = new DataTypeMapper();
                            columns.Add(new Column
                                    {
                                        Name = columnName,
                                        DataType = dataType,
                                        IsNullable = isNullable,
                                        IsUnique = isUnique,
                                        IsPrimaryKey = isPrimaryKey,
                                        IsForeignKey = isForeignKey,
                                        IsIdentity = isIdentity,
                                        DataLength = dataLength,
                                        DataPrecision = dataPrecision,
                                        DataScale = dataScale,
                                        ForeignKeyTableName = fkTableName,
                                        ConstraintName = constraintName,
                                        MappedDataType =
                                            m.MapFromDBType(ServerType.CUBRID, dataType, null, null, null).ToString(),
                                    });
                        }
                    }
                }

                table.Columns = columns;

                table.Owner = owner;
                table.PrimaryKey = DeterminePrimaryKeys(table);

                table.HasManyRelationships = DetermineHasManyRelationships(table);
            }
            finally
            {
                conn.Close();
            }

            return columns;
        }

        public IList<ForeignKey> DetermineForeignKeyReferences(Table table)
        {
            List<ForeignKey> foreignKeys = (from column in table.Columns
                                            where column.ForeignKeyTableName != null
                                            select new ForeignKey
                                                       {
                                                           Name = column.ForeignKeyTableName + "_" + column.ForeignKeyColumnName,
                                                           References = column.ForeignKeyTableName,
                                                           Columns = new[] { column },
                                                           UniquePropertyName = column.Name
                                                       }).ToList();
            Table.SetUniqueNamesForForeignKeyProperties(foreignKeys);

            return foreignKeys;
        }

        public PrimaryKey DeterminePrimaryKeys(Table table)
        {
            var primaryKeys = table.Columns.Where(x => x.IsPrimaryKey.Equals(true)).ToList();

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
    }
}
