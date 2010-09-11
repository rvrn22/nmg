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
            var conn = new OracleConnection(connectionStr);
            conn.Open();
            using (conn)
            {
                using (OracleCommand tableCommand = conn.CreateCommand())
                {
                    tableCommand.CommandText =
                        String.Format(
                            @"
                            select tc.column_name as column_name, tc.data_type as data_type, tc.nullable as nullable, nvl(c.constraint_type, 'CHANGE THIS IN CODE') as constraint_type, 
                                data_length as data_length
                            from all_tab_columns tc
	                            left outer join (
			                            all_cons_columns cc
			                            join all_constraints c on (
				                            c.owner=cc.owner 
				                            and c.constraint_name = cc.constraint_name 
				                            and c.constraint_type <> 'C'
			                            )
		                            ) on (
			                            tc.owner = cc.owner and cc.owner = '{1}'
			                            and tc.table_name = cc.table_name
			                            and tc.column_name = cc.column_name
		                            )
                                where tc.table_name = '{0}'
                            order by tc.table_name, cc.position nulls last, tc.column_id",
                            table.Name, owner);
                    using (OracleDataReader oracleDataReader = tableCommand.ExecuteReader(CommandBehavior.Default))
                    {
                        var m = new DataTypeMapper();
                        while (oracleDataReader.Read())
                        {
                            columns.Add(new Column
                                            {
                                                Name = oracleDataReader["column_name"].ToString(),
                                                DataType = oracleDataReader["data_type"].ToString(),
                                                IsNullable =
                                                    oracleDataReader["nullable"].ToString().Equals("Y",
                                                                                                   StringComparison.
                                                                                                       CurrentCultureIgnoreCase),
                                                IsPrimaryKey =
                                                    ConstraintTypeResolver.IsPrimaryKey(
                                                        oracleDataReader["constraint_type"].ToString()),
                                                IsForeignKey =
                                                    ConstraintTypeResolver.IsForeignKey(
                                                        oracleDataReader["constraint_type"].ToString()),
                                                IsUnique =
                                                    ConstraintTypeResolver.IsUnique(
                                                        oracleDataReader["constraint_type"].ToString()),
                                                MappedDataType =
                                                    m.MapFromDBType(oracleDataReader["data_type"].ToString(), null, null,
                                                                    null).ToString(),
                                                DataLength = Convert.ToInt16(oracleDataReader["data_length"])
                                            });

                            table.Columns = columns;
                        }
                        table.PrimaryKey = DeterminePrimaryKeys(table);
                        table.ForeignKeys = DetermineForeignKeyReferences(table);
                        table.HasManyRelationships = DetermineHasManyRelationships(table);
                    }
                }
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
                OracleCommand tableCommand = conn.CreateCommand();
                tableCommand.CommandText = String.Format("select table_name from all_tables where owner = '{0}'", owner);
                    // where owner = 'HR'
                OracleDataReader oracleDataReader = tableCommand.ExecuteReader(CommandBehavior.CloseConnection);
                while (oracleDataReader.Read())
                {
                    string tableName = oracleDataReader.GetString(0);
                    tables.Add(new Table {Name = tableName});
                }
            }
            tables.Sort((x, y) => x.Name.CompareTo(y.Name));
            return tables;
        }

        public IList<string> GetOwners()
        {
            var owners = new List<string>();
            var conn = new OracleConnection(connectionStr);
            conn.Open();
            using (conn)
            {
                OracleCommand tableCommand = conn.CreateCommand();
                tableCommand.CommandText = "select username from all_users order by username";
                OracleDataReader oracleDataReader = tableCommand.ExecuteReader(CommandBehavior.CloseConnection);
                while (oracleDataReader.Read())
                {
                    string owner = oracleDataReader.GetString(0);
                    owners.Add(owner);
                }
            }
            return owners;
        }

        public List<string> GetSequences()
        {
            var sequences = new List<string>();
            var conn = new OracleConnection(connectionStr);
            conn.Open();
            using (conn)
            {
                OracleCommand seqCommand = conn.CreateCommand();
                seqCommand.CommandText = "select * from all_sequences";
                OracleDataReader seqReader = seqCommand.ExecuteReader(CommandBehavior.CloseConnection);
                while (seqReader.Read())
                {
                    string tableName = seqReader.GetString(0);
                    sequences.Add(tableName);
                }
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
                        String.Format(
                            @"
                        select c.table_name
                        from   all_constraints p
                        join   all_constraints c on c.r_constraint_name = p.constraint_name
                                                 and c.r_owner = p.owner
                        where p.table_name = '{0}'",
                            table.Name);
                    OracleDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        hasManyRelationships.Add(new HasMany
                                                     {
                                                         Reference = reader.GetString(0)
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
                                  Columns =
                                      {
                                          new Column
                                              {
                                                  DataType = c.DataType,
                                                  Name = c.Name
                                              }
                                      }
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
                    key.Columns.Add(new Column
                                        {
                                            DataType = primaryKey.DataType,
                                            Name = primaryKey.Name
                                        });
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

                return (string) referencedTableName;
            }
        }
    }
}