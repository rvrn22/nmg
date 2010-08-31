using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;
using NMG.Core.Domain;
using System.Linq;

namespace NMG.Core.Reader
{
    public class OracleMetadataReader : IMetadataReader
    {
        private readonly string connectionStr;

        public OracleMetadataReader(string connectionStr)
        {
            this.connectionStr = connectionStr;
        }

        public IList<Column> GetTableDetails(Table selectedTableName)
        {
            var columns = new List<Column>();
            var conn = new OracleConnection(connectionStr);
            conn.Open();
            using (conn)
            {
                using (OracleCommand tableCommand = conn.CreateCommand())
                {
                    tableCommand.CommandText =
                        String.Format(@"
                            select tc.column_name, tc.data_type, tc.nullable, nvl(c.constraint_type, 'CHANGE THIS IN CODE'), data_length
                            from all_tab_columns tc
	                            left outer join (
			                            all_cons_columns cc
			                            join all_constraints c on (
				                            c.owner=cc.owner 
				                            and c.constraint_name = cc.constraint_name 
				                            and c.constraint_type <> 'C'
			                            )
		                            ) on (
			                            tc.owner = cc.owner --and cc.owner = user
			                            and tc.table_name = cc.table_name
			                            and tc.column_name = cc.column_name
		                            )
                                where tc.table_name = '{0}'
                            order by tc.table_name, cc.position nulls last, tc.column_id", selectedTableName.Name);
                    using (OracleDataReader oracleDataReader = tableCommand.ExecuteReader(CommandBehavior.Default))
                    {
                        while (oracleDataReader.Read())
                        {
                            string columnName = oracleDataReader.GetString(0);
                            string dataType = oracleDataReader.GetString(1);
                            long dataLength = oracleDataReader.GetInt64(4);
                            bool isNullable = oracleDataReader.GetString(2).Equals("Y", StringComparison.CurrentCultureIgnoreCase);
                            bool isPrimaryKey = oracleDataReader.GetString(3).Equals("P", StringComparison.CurrentCultureIgnoreCase);
                            bool isForeignKey = oracleDataReader.GetString(3).Equals("R", StringComparison.CurrentCultureIgnoreCase); // 'R' for reference

                            var m = new DataTypeMapper();

                            columns.Add(new Column
                                            {
                                                Name = columnName,
                                                DataType = dataType,
                                                IsNullable = isNullable,
                                                IsPrimaryKey = isPrimaryKey,
                                                IsForeignKey = isForeignKey,
                                                MappedDataType = m.MapFromDBType(dataType, null, null, null).ToString(),
                                                DataLength = dataLength
                                            });

                            selectedTableName.Columns = columns;
                        }
                        selectedTableName.PrimaryKey = DeterminePrimaryKeys(selectedTableName);
                        selectedTableName.ForeignKeys = DetermineForeignKeyReferences(selectedTableName);
                        selectedTableName.HasManyRelationships = DetermineHasManyRelationships(selectedTableName);
                    }
                }
            }

            return columns;
        }

        private bool IsPrimaryKey(string p, string columnName)
        {
            string query =
                String.Format(
                    @"
                select count(ac.constraint_type) from all_constraints ac
                inner join all_cons_columns acc on ac.constraint_name = acc.constraint_name
                where acc.table_name = '{0}' and acc.column_name = '{1}' 
                and ac.constraint_type = 'P'", p, columnName);
            var conn = new OracleConnection(connectionStr);
            conn.Open();
            using(conn)
            {
                using (var command = new OracleCommand())
                {
                    command.Connection = conn;
                    command.CommandText = query;
                    var pk = command.ExecuteScalar();

                    return Convert.ToInt16(pk) == 1;
                }
            }
        }

        public IList<ForeignKey> DetermineForeignKeyReferences(Table table)
        {
            var foreignKeys = table.Columns.Where(x => x.IsForeignKey.Equals(true));
            var tempForeignKeys = new List<ForeignKey>();

            foreach(var foreignKey in foreignKeys)
            {
                tempForeignKeys.Add(new ForeignKey
                                        {
                                            Name = foreignKey.Name,
                                            References = GetForeignKeyReferenceTableName(table.Name, foreignKey.Name)
                                        });
            }

            return tempForeignKeys;

//            var foreignKeys = new List<ForeignKey>();
//            var conn = new OracleConnection(connectionStr);
//            conn.Open();
//            using (var command = new OracleCommand())
//            {
//                command.Connection = conn;
//                command.CommandText = String.Format(@"
//                    SELECT    ucc2.table_name, ucc2.column_name
//                      FROM all_constraints uc, all_cons_columns ucc1, all_cons_columns ucc2
//                     WHERE uc.constraint_name = ucc1.constraint_name
//                       AND uc.r_constraint_name = ucc2.constraint_name
//                       AND ucc1.POSITION =
//                                      ucc2.POSITION
//                                                   -- Correction for multiple column primary keys.
//                       AND uc.constraint_type = 'R'
//                       AND ucc1.table_name = '{0}'", table.Name);
//                var reader = command.ExecuteReader();
//                while(reader.Read())
//                {
//                    foreignKeys.Add(new ForeignKey()
//                                              {
//                                                  Name = reader.GetString(1),
//                                                  References = reader.GetString(0)
//                                              });
//                }
//                return foreignKeys;
//            }
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
                    command.CommandText = String.Format(@"
                        select c.table_name
                        from   all_constraints p
                        join   all_constraints c on c.r_constraint_name = p.constraint_name
                                                 and c.r_owner = p.owner
                        where p.table_name = '{0}'", table.Name);
                    var reader = command.ExecuteReader();

                    while(reader.Read())
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
            var primaryKeys = table.Columns.Where(x => x.IsPrimaryKey.Equals(true));

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
        }

        // http://blog.mclaughlinsoftware.com/2009/03/05/validating-foreign-keys/

        public List<Table> GetTables()
        {
            var tables = new List<Table>();
            var conn = new OracleConnection(connectionStr);
            conn.Open();
            using (conn)
            {
                OracleCommand tableCommand = conn.CreateCommand();
                tableCommand.CommandText = "select table_name from all_tables"; // where owner = 'HR'
                OracleDataReader oracleDataReader = tableCommand.ExecuteReader(CommandBehavior.CloseConnection);
                while (oracleDataReader.Read())
                {
                    string tableName = oracleDataReader.GetString(0);
                    tables.Add(new Table {Name = tableName});
                }
            }
            //tables.Sort((x,y) => x.CompareTo(y));
            return tables;
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

//select p.table_name, 'is parent of ' rel, c.table_name
//from   all_constraints p
//join   all_constraints c on c.r_constraint_name = p.constraint_name
//                         and c.r_owner = p.owner
//where p.table_name = 'INCIDENT'    
//union all
//select c.table_name, 'is child of ' rel, p.table_name
//from   all_constraints p
//join   all_constraints c on c.r_constraint_name = p.constraint_name
//                         and c.r_owner = p.owner
//where c.table_name = 'INCIDENT' 

        // get has many relationships?
        public List<string> GetForeignKeyTables(string primaryKeyColumnName)
        {
            var foreignKeyTables = new List<string>();
            var conn = new OracleConnection(connectionStr);
            conn.Open();
            using (conn)
            {
                using (OracleCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText =
                        string.Format(
                            @"
select c.table_name
from   all_constraints p
join   all_constraints c on c.r_constraint_name = p.constraint_name
                         and c.r_owner = p.owner
where p.table_name = '{0}'",
                            primaryKeyColumnName);
                    OracleDataReader dataReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                    if (dataReader != null)
                    {
                        while (dataReader.Read())
                        {
                            foreignKeyTables.Add(dataReader.GetString(0));
                        }
                    }
                }
            }
            return foreignKeyTables;
        }

        #region crap
        //        public bool UsesCompositeKey(string tableName)
//        {
//            var conn = new OracleConnection(connectionStr);
//            conn.Open();

//            using (conn)
//            {
//                using (OracleCommand cmd = conn.CreateCommand())
//                {
//                    cmd.CommandText =
//                        string.Format(
//                            @"
//                        select
//                            count(acc.column_name)
//                        from
//                            all_constraints ac
//                            inner join all_cons_columns acc on ac.constraint_name = acc.constraint_name
//                        where 
//                            ac.constraint_type = 'P'
//                            and ac.table_name = '{0}'
//                        ",
//                            tableName);

//                    object pkCount = cmd.ExecuteScalar();
//                    if (pkCount != null)
//                    {
//                        return ((int) pkCount > 1);
//                    }

//                    return false;
//                }
//            }
//        }


//        private bool IsForeignKey(string selectedTableName, string columnName)
//        {
//            var conn = new OracleConnection(connectionStr);
//            conn.Open();
//            using (conn)
//            {
//                using (OracleCommand constraintCommand = conn.CreateCommand())
//                {
//                    constraintCommand.CommandText =
//                        string.Format(
//                            @"SELECT a.constraint_name
//                          FROM all_cons_columns a JOIN all_constraints c
//                               ON a.constraint_name = c.constraint_name
//                         WHERE c.constraint_type = 'R' and c.table_name = '{0}'
//                            and a.column_name = '{1}'",
//                            selectedTableName, columnName);
//                    object value = constraintCommand.ExecuteScalar();
//                    if (value != null)
//                    {
//                        var constraintName = (string) value;
//                        using (OracleCommand fkColumnCommand = conn.CreateCommand())
//                        {
//                            fkColumnCommand.CommandText =
//                                string.Format(
//                                    @"SELECT    ucc1.column_name
//                                  FROM all_constraints uc, all_cons_columns ucc1, all_cons_columns ucc2
//                                 WHERE uc.constraint_name = ucc1.constraint_name
//                                   AND uc.r_constraint_name = ucc2.constraint_name
//                                   AND ucc1.POSITION =
//                                                  ucc2.POSITION
//                                   AND uc.constraint_type = 'R'
//                                   AND ucc1.table_name = '{0}'
//                                   and ucc1.constraint_name = '{1}'",
//                                    selectedTableName, constraintName);
//                            object colName = fkColumnCommand.ExecuteScalar();
//                            if (colName != null)
//                            {
//                                var fkColumnName = (string) colName;
//                                return columnName.Equals(fkColumnName, StringComparison.CurrentCultureIgnoreCase);
//                            }
//                        }
//                    }
//                }
//            }

//            return false;
        //        }
        #endregion

        private string GetForeignKeyReferenceTableName(string selectedTableName, string columnName)
        {
            string foreignKeyReferenceTableName = string.Empty;
            var conn = new OracleConnection(connectionStr);
            conn.Open();
            using(conn)
            {
                OracleCommand tableCommand = conn.CreateCommand();
                tableCommand.CommandText = String.Format(
                    @"SELECT    ucc2.table_name
                      FROM all_constraints uc, all_cons_columns ucc1, all_cons_columns ucc2
                     WHERE uc.constraint_name = ucc1.constraint_name
                       AND uc.r_constraint_name = ucc2.constraint_name
                       AND uc.constraint_type = 'R'
                       AND ucc1.table_name = '{0}'
                      and ucc1.column_name = '{1}'", selectedTableName, columnName);
                var referencedTableName = tableCommand.ExecuteScalar();

                return (string) referencedTableName;
            }
//            string foreignKeyReferenceTableName = string.Empty;
//            var conn = new OracleConnection(connectionStr);
//            conn.Open();
//            using (conn)
//            {
//                OracleCommand tableCommand = conn.CreateCommand();
//                tableCommand.CommandText =
//                    @"SELECT    ucc2.table_name
//                      FROM all_constraints uc, all_cons_columns ucc1, all_cons_columns ucc2
//                     WHERE uc.constraint_name = ucc1.constraint_name
//                       AND uc.r_constraint_name = ucc2.constraint_name
//                       AND ucc1.POSITION =
//                                      ucc2.POSITION
//                                                   -- Correction for multiple column primary keys.
//                       AND uc.constraint_type = 'R'
//                       AND ucc1.table_name = '" +
//                    selectedTableName + "'" +
//                    "and ucc1.column_name = '" + columnName + "'";

//                OracleDataReader oracleDataReader = tableCommand.ExecuteReader(CommandBehavior.CloseConnection);
//                if (oracleDataReader != null)
//                    while (oracleDataReader.Read())
//                    {
//                        foreignKeyReferenceTableName = oracleDataReader.GetString(0);
//                    }
//            }

//            return foreignKeyReferenceTableName;
        }
    }
}