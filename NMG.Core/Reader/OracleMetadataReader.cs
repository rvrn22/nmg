using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using NMG.Core.Domain;
using Oracle.ManagedDataAccess.Client;

namespace NMG.Core.Reader
{
    public static class ConstraintTypeResolver //: IConstraintTypeResolver
    {
        public static bool IsPrimaryKey(int constraintType)
        {
            return (constraintType & OracleConstraintType.PrimaryKey.Value) == OracleConstraintType.PrimaryKey.Value;
        }

        public static bool IsForeignKey(int constraintType)
        {
            return (constraintType & OracleConstraintType.ForeignKey.Value) == OracleConstraintType.ForeignKey.Value;
        }

        public static bool IsUnique(int constraintType)
        {
            return (constraintType & OracleConstraintType.Unique.Value) == OracleConstraintType.Unique.Value;
        }

        public static bool IsCheck(int constraintType)
        {
            return (constraintType & OracleConstraintType.Check.Value) == OracleConstraintType.Check.Value;
        }
    }

    public class OracleMetadataReader : IMetadataReader
    {
        private readonly string connectionStr;

        public OracleMetadataReader(string connectionStr)
        {
            this.connectionStr = connectionStr;
        }
        #region IMetadataReader Members
        /// <summary>
        /// Return all table details based on table and owner.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="owner"></param>
        /// <returns></returns>
        public IList<Column> GetTableDetails(Table table, string owner)
        {
            var columns = new List<Column>();
            using (var conn = new OracleConnection(connectionStr))
            {
                conn.Open();
                using (OracleCommand tableCommand = conn.CreateCommand())
                {
                    tableCommand.CommandText =
                        @"select column_name, data_type, nullable, sum(constraint_type) constraint_type, data_length, data_precision, data_scale
from (
SELECT tc.column_name AS column_name, tc.data_type AS data_type, tc.nullable AS NULLABLE, 
                                    decode(c.constraint_type, 'P', 1, 'R', 2, 'U', 4, 'C', 8, 16) AS constraint_type, 
                                    data_length, data_precision, data_scale, column_id
from all_tab_columns tc
    left outer join (
            all_cons_columns cc
            join all_constraints c on (
                c.owner=cc.owner 
                and c.constraint_name = cc.constraint_name 
            )
        ) on (
            tc.owner = cc.owner
            and tc.table_name = cc.table_name
            and tc.column_name = cc.column_name
        )
    where tc.table_name = :table_name and tc.owner = :owner    
order by tc.table_name, cc.position nulls last, tc.column_id)
group by column_name, data_type, nullable, data_length, data_precision, data_scale, column_id
order by column_id";

                    tableCommand.Parameters.Add("table_name", table.Name);
                    tableCommand.Parameters.Add("owner", owner);
                    using (OracleDataReader oracleDataReader = tableCommand.ExecuteReader(CommandBehavior.Default))
                    {
                        var m = new DataTypeMapper();
                        while (oracleDataReader.Read())
                        {
                            var constraintType = oracleDataReader.Get<int>("constraint_type");
                            int? dataLength = oracleDataReader.IsDBNull(4) ? (int?)null : oracleDataReader.Get<int>("data_length");
                            int? dataPrecision = oracleDataReader.IsDBNull(5) ? (int?)null : oracleDataReader.Get<int>("data_precision");
                            int? dataScale = oracleDataReader.IsDBNull(6) ? (int?)null : oracleDataReader.Get<int>("data_scale");

                            columns.Add(new Column {
                                Name = oracleDataReader.GetOracleString(0).Value,
                                DataType = oracleDataReader.GetOracleString(1).Value,
                                IsNullable = string.Equals(oracleDataReader.GetOracleString(2).Value, "Y", StringComparison.OrdinalIgnoreCase),
                                IsPrimaryKey = ConstraintTypeResolver.IsPrimaryKey(constraintType),
                                IsForeignKey = ConstraintTypeResolver.IsForeignKey(constraintType),
                                IsUnique = ConstraintTypeResolver.IsUnique(constraintType),
                                MappedDataType = m.MapFromDBType(ServerType.Oracle, oracleDataReader.GetOracleString(1).Value, dataLength, dataPrecision, dataScale).ToString(),
                                DataLength = dataLength,
                                DataPrecision = dataPrecision,
                                DataScale = dataScale
                            });
                        }
                        table.Owner = owner;
                        table.Columns = columns;
                        table.PrimaryKey = DeterminePrimaryKeys(table);

                        // Need to find the table name associated with the FK
                        foreach (var c in table.Columns.Where(c => c.IsForeignKey))
                        {
                            var foreignInfo = GetForeignKeyReferenceTableName(table.Name, c.Name);
                            c.ForeignKeyTableName = foreignInfo.Item1;
                            c.ForeignKeyColumnName = foreignInfo.Item2;
                        }
                        table.ForeignKeys = DetermineForeignKeyReferences(table);
                        table.HasManyRelationships = DetermineHasManyRelationships(table);
                    }
                }
                conn.Close();
            }

            return columns;
        }

        public List<Table> GetTables(string owner)
        {
            var tables = new List<Table>();
            var conn = new OracleConnection(connectionStr);
            conn.Open();
            using (conn)
            {
                using (OracleCommand tableCommand = conn.CreateCommand())
                {
                    tableCommand.CommandText = "select table_name from all_tables where owner = :table_name order by table_name";
                    tableCommand.Parameters.Add(new OracleParameter("table_name", owner));
                    using (OracleDataReader oracleDataReader = tableCommand.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (oracleDataReader.Read())
                        {
                            string tableName = oracleDataReader.GetString(0);
                            tables.Add(new Table { Name = tableName });
                        }
                    }
                }
            }
            return tables;
        }

        public IList<string> GetOwners()
        {
            var owners = new List<string>();
            using (var conn = new OracleConnection(connectionStr))
            {
                conn.Open();
                using (OracleCommand tableCommand = conn.CreateCommand())
                {
                    tableCommand.CommandText = "select username from all_users order by username";
                    using (OracleDataReader oracleDataReader = tableCommand.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (oracleDataReader.Read())
                        {
                            string owner = oracleDataReader.GetString(0);
                            owners.Add(owner);
                        }
                    }
                }
                conn.Close();
            }
            return owners;
        }

        public List<string> GetSequences(string owner)
        {
            var sequences = new List<string>();
            using (var conn = new OracleConnection(connectionStr))
            {
                conn.Open();
                using (var seqCommand = conn.CreateCommand())
                {
                    seqCommand.CommandText = "select sequence_name from all_sequences where sequence_owner = :owner order by sequence_name";
                    seqCommand.Parameters.Add(new OracleParameter("owner", owner));
                    using (OracleDataReader seqReader = seqCommand.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (seqReader.Read())
                        {
                            string tableName = seqReader.GetString(0);
                            sequences.Add(tableName);
                        }
                    }
                }
                conn.Close();
            }
            return sequences;
        }

        public PrimaryKey DeterminePrimaryKeys(Table table)
        {
            var primaryKeys = table.Columns.Where(x => x.IsPrimaryKey.Equals(true)).ToList();

            if (primaryKeys.Count() == 1)
            {
                var c = primaryKeys.First();
                var key = new PrimaryKey {
                    Type = PrimaryKeyType.PrimaryKey,
                    Columns = { c }
                };
                return key;
            }

            if (primaryKeys.Count() > 1)
            {
                // Composite key
                var key = new PrimaryKey {
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
                               select new ForeignKey {
                Name = g.Key.ConstraintName,
                References = g.Key.ForeignKeyTableName,
                Columns = g.ToList(),
                UniquePropertyName = g.Key.ForeignKeyTableName
            }).ToList();

            Table.SetUniqueNamesForForeignKeyProperties(foreignKeys);

            return foreignKeys;
        }
        #endregion
        public IList<HasMany> DetermineHasManyRelationships(Table table)
        {
            var hasManyRelationships = new List<HasMany>();
            var conn = new OracleConnection(connectionStr);
            conn.Open();
            using (conn)
            {
                using (var command = new OracleCommand())
                {
                    command.Connection = conn;
                    command.CommandText =
@"SELECT R.TABLE_NAME, COL.COLUMN_NAME
FROM ALL_CONSTRAINTS C, ALL_CONSTRAINTS R, ALL_CONS_COLUMNS COL
WHERE C.TABLE_NAME = :TABLE_NAME
  AND C.CONSTRAINT_NAME = R.R_CONSTRAINT_NAME
  AND R.CONSTRAINT_NAME = COL.CONSTRAINT_NAME
  AND R.TABLE_NAME = COL.TABLE_NAME
  AND R.OWNER = COL.OWNER";
                    command.Parameters.Add(new OracleParameter("TABLE_NAME", table.Name));
                    OracleDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        hasManyRelationships.Add(new HasMany {
                            Reference = reader.GetString(0),
                            ReferenceColumn = reader.GetString(1)
                        });
                    }

                    return hasManyRelationships;
                }
            }
        }
        // http://blog.mclaughlinsoftware.com/2009/03/05/validating-foreign-keys/
        private Tuple<string, string> GetForeignKeyReferenceTableName(string selectedTableName, string columnName)
        {
            using (var conn = new OracleConnection(connectionStr))
            {
                conn.Open();
                using (OracleCommand tableCommand = conn.CreateCommand())
                {
                    tableCommand.CommandText = 
                    @"SELECT  ucc2.table_name, ucc2.column_name
                      FROM all_constraints uc, all_cons_columns ucc1, all_cons_columns ucc2
                     WHERE uc.constraint_name = ucc1.constraint_name
                       AND uc.r_constraint_name = ucc2.constraint_name
                       AND uc.constraint_type = 'R'
                       AND ucc1.table_name = :table_name
                      and ucc1.column_name = :column_name";
                 
                    tableCommand.Parameters.Add("table_name", selectedTableName);
                    tableCommand.Parameters.Add("column_name", columnName);
                    using (var reader = tableCommand.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Tuple<string, string>(reader.GetOracleString(0).Value, reader.GetOracleString(1).Value);
                        } 
                        else
                            return null;
                    }
                }
                conn.Close();
            }
        }
    }
}