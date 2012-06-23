using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;
using System.Linq;
using NMG.Core.Domain;

namespace NMG.Core.Reader
{
    public static class ConstraintTypeResolver //: IConstraintTypeResolver
    {
        public static bool IsPrimaryKey(string constraintType)
        {
            return (constraintType == OracleConstraintType.PrimaryKey.ToString());
        }

        public static bool IsForeignKey(string constraintType)
        {
            return (constraintType == OracleConstraintType.ForeignKey.ToString());
        }

        public static bool IsUnique(string constraintType)
        {
            return (constraintType == OracleConstraintType.Unique.ToString());
        }

        public static bool IsCheck(string constraintType)
        {
            return (constraintType == OracleConstraintType.Check.ToString());
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
                          @"SELECT tc.column_name AS column_name, tc.data_type AS data_type, tc.nullable AS NULLABLE, 
                                    nvl(c.constraint_type,'CHANGE THIS IN CODE') AS constraint_type, 
                                    data_length, data_precision, data_scale
                            from all_tab_columns tc
                                left outer join (
                                        all_cons_columns cc
                                        join all_constraints c on (
                                            c.owner=cc.owner 
                                            and c.constraint_name = cc.constraint_name 
                                            and c.constraint_type <> 'C'
                                        )
                                    ) on (
                                        tc.owner = cc.owner
                                        and tc.table_name = cc.table_name
                                        and tc.column_name = cc.column_name
                                    )
                                where tc.table_name = :table_name and tc.owner = :owner
                            order by tc.table_name, cc.position nulls last, tc.column_id";
                    tableCommand.Parameters.Add("table_name", table.Name);
                    tableCommand.Parameters.Add("owner", owner);
                    using (OracleDataReader oracleDataReader = tableCommand.ExecuteReader(CommandBehavior.Default))
                    {
                        var m = new DataTypeMapper();
                        while (oracleDataReader.Read())
                        {
                            var constraintType = oracleDataReader.GetOracleString(3).Value;
                            int? dataLength = oracleDataReader.IsDBNull(4) ? (int?)null : Convert.ToInt32(oracleDataReader.GetOracleNumber(4).Value);
                            int? dataPrecision = oracleDataReader.IsDBNull(5) ? (int?)null : Convert.ToInt32(oracleDataReader.GetOracleNumber(5).Value);
                            int? dataScale = oracleDataReader.IsDBNull(6) ? (int?)null : Convert.ToInt32(oracleDataReader.GetOracleNumber(6).Value);

                            columns.Add(new Column
                                            {
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
                        table.Columns = columns;
                        table.PrimaryKey = DeterminePrimaryKeys(table);
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
                            tables.Add(new Table {Name = tableName});
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

        #endregion

        private bool IsPrimaryKey(string p, string columnName)
        {
            string query =
                String.Format(
                    @"
                select count(ac.constraint_type) from all_constraints ac
                inner join all_cons_columns acc on ac.constraint_name = acc.constraint_name
                where acc.table_name = '{0}' and acc.column_name = '{1}' 
                and ac.constraint_type = 'P'",
                    p, columnName);
            var conn = new OracleConnection(connectionStr);
            conn.Open();
            using (conn)
            {
                using (var command = new OracleCommand())
                {
                    command.Connection = conn;
                    command.CommandText = query;
                    object pk = command.ExecuteScalar();

                    return Convert.ToInt16(pk) == 1;
                }
            }
        }

        public IList<ForeignKey> DetermineForeignKeyReferences(Table table)
        {
            IEnumerable<Column> foreignKeys = table.Columns.Where(x => x.IsForeignKey.Equals(true));
            var tempForeignKeys = new List<ForeignKey>();

            foreach (Column foreignKey in foreignKeys)
            {
                tempForeignKeys.Add(new ForeignKey
                                        {
                                            Name = foreignKey.Name,
                                            References = GetForeignKeyReferenceTableName(table.Name, foreignKey.Name)
                                        });
            }

            return tempForeignKeys;
        }

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
                        hasManyRelationships.Add(new HasMany
                                                     {
                                                         Reference = reader.GetString(0),
                                                         ReferenceColumn = reader.GetString(1)
                                                     });
                    }

                    return hasManyRelationships;
                }
            }
        }

        public PrimaryKey DeterminePrimaryKeys(Table table)
        {
            IEnumerable<Column> primaryKeys = table.Columns.Where(x => x.IsPrimaryKey.Equals(true));

            if (primaryKeys.Count() == 1)
            {
                Column c = primaryKeys.First();
                var key = new PrimaryKey
                              {
                                  Type = PrimaryKeyType.PrimaryKey,
                                  Columns = new List<Column> { c }
                              };
                return key;
            }
            else
            {
                var key = new PrimaryKey
                              {
                                  Type = PrimaryKeyType.CompositeKey
                              };
                foreach (Column primaryKey in primaryKeys)
                {
                    key.Columns.Add(primaryKey);
                }
                return key;
            }
        }

        // http://blog.mclaughlinsoftware.com/2009/03/05/validating-foreign-keys/

        private string GetForeignKeyReferenceTableName(string selectedTableName, string columnName)
        {
            string foreignKeyReferenceTableName = string.Empty;
            var conn = new OracleConnection(connectionStr);
            conn.Open();
            using (conn)
            {
                OracleCommand tableCommand = conn.CreateCommand();
                tableCommand.CommandText = String.Format(
                    @"SELECT    ucc2.table_name
                      FROM all_constraints uc, all_cons_columns ucc1, all_cons_columns ucc2
                     WHERE uc.constraint_name = ucc1.constraint_name
                       AND uc.r_constraint_name = ucc2.constraint_name
                       AND uc.constraint_type = 'R'
                       AND ucc1.table_name = '{0}'
                      and ucc1.column_name = '{1}'",
                    selectedTableName, columnName);
                object referencedTableName = tableCommand.ExecuteScalar();

                return (string)referencedTableName;
            }
        }
    }
}