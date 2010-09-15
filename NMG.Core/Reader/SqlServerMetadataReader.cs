using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using NMG.Core.Domain;

namespace NMG.Core.Reader
{
    // http://www.sqlteam.com/forums/topic.asp?TOPIC_ID=72957

    public class SqlServerMetadataReader : IMetadataReader
    {
        private readonly string connectionStr;

        public SqlServerMetadataReader(string connectionStr)
        {
            this.connectionStr = connectionStr;
        }

        #region IMetadataReader Members

        public IList<Column> GetTableDetails(Table table, string owner)
        {
            var columns = new List<Column>();
            var conn = new SqlConnection(connectionStr);
            conn.Open();
            using (conn)
            {
                using (SqlCommand tableDetailsCommand = conn.CreateCommand())
                {
                    tableDetailsCommand.CommandText = string.Format(
                        @"
                        select c.column_name,c .data_type, c.is_nullable, tc.constraint_type
                        from information_schema.columns c
	                        left outer join (
		                        information_schema.constraint_column_usage ccu
		                        join information_schema.table_constraints tc on (
			                        tc.table_schema = ccu.table_schema
			                        and tc.constraint_name = ccu.constraint_name
			                        and tc.constraint_type <> 'CHECK'
		                        )
	                        ) on (
		                        c.table_schema = ccu.table_schema and ccu.table_schema = '{1}'
		                        and c.table_name = ccu.table_name
		                        and c.column_name = ccu.column_name
	                        )
                        where c.table_name = '{0}'
                        order by c.table_name, c.ordinal_position",
                        table.Name, owner);
                    using (SqlDataReader sqlDataReader = tableDetailsCommand.ExecuteReader(CommandBehavior.Default))
                    {
                        while (sqlDataReader.Read())
                        {
                            string columnName = sqlDataReader.GetString(0);
                            string dataType = sqlDataReader.GetString(1);
                            bool isNullable = sqlDataReader.GetString(2).Equals("YES",
                                                                                StringComparison.
                                                                                    CurrentCultureIgnoreCase);
                            bool isPrimaryKey =
                                (!sqlDataReader.IsDBNull(3)
                                     ? sqlDataReader.GetString(3).Equals(
                                         SqlServerConstraintType.PrimaryKey.ToString(),
                                         StringComparison.CurrentCultureIgnoreCase)
                                     : false);
                            bool isForeignKey =
                                (!sqlDataReader.IsDBNull(3)
                                     ? sqlDataReader.GetString(3).Equals(
                                         SqlServerConstraintType.ForeignKey.ToString(),
                                         StringComparison.CurrentCultureIgnoreCase)
                                     : false);

                            var m = new DataTypeMapper();

                            columns.Add(new Column
                                            {
                                                Name = columnName,
                                                DataType = dataType,
                                                IsNullable = isNullable,
                                                IsPrimaryKey = isPrimaryKey,
                                                //IsPrimaryKey(selectedTableName.Name, columnName)
                                                IsForeignKey = isForeignKey,
                                                // IsFK()
                                                MappedDataType =
                                                    m.MapFromDBType(dataType, null, null, null).ToString()
                                                //DataLength = dataLength
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

        public IList<string> GetOwners()
        {
            var owners = new List<string>();
            var conn = new SqlConnection(connectionStr);
            conn.Open();
            using (conn)
            {
                var tableCommand = conn.CreateCommand();
                tableCommand.CommandText = "SELECT SCHEMA_NAME from INFORMATION_SCHEMA.SCHEMATA";
                var sqlDataReader = tableCommand.ExecuteReader(CommandBehavior.CloseConnection);
                while (sqlDataReader.Read())
                {
                    var ownerName = sqlDataReader.GetString(0);
                    owners.Add(ownerName);
                }
            }

            return owners;
        }

        public List<Table> GetTables(string owner)
        {
            var tables = new List<Table>();
            var conn = new SqlConnection(connectionStr);
            conn.Open();
            using (conn)
            {
                var tableCommand = conn.CreateCommand();
                tableCommand.CommandText = String.Format("select table_name from information_schema.tables where table_type like 'BASE TABLE' and TABLE_SCHEMA = '{0}'", owner);
                var sqlDataReader = tableCommand.ExecuteReader(CommandBehavior.CloseConnection);
                while (sqlDataReader.Read())
                {
                    var tableName = sqlDataReader.GetString(0);
                    tables.Add(new Table {Name = tableName});
                }
            }
            tables.Sort((x, y) => x.Name.CompareTo(y.Name));
            return tables;
        }

        public List<string> GetSequences()
        {
            return new List<string>();
        }

        #endregion

        private static PrimaryKey DeterminePrimaryKeys(Table table)
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

        private IList<ForeignKey> DetermineForeignKeyReferences(Table table)
        {
            var foreignKeys = table.Columns.Where(x => x.IsForeignKey.Equals(true));
            var tempForeignKeys = new List<ForeignKey>();

            foreach (var foreignKey in foreignKeys)
            {
                tempForeignKeys.Add(new ForeignKey
                                        {
                                            Name = foreignKey.Name,
                                            References = GetForeignKeyReferenceTableName(table.Name, foreignKey.Name)
                                        });
            }

            return tempForeignKeys;
        }

        private string GetForeignKeyReferenceTableName(string selectedTableName, string columnName)
        {
            var conn = new SqlConnection(connectionStr);
            conn.Open();
            using (conn)
            {
                SqlCommand tableCommand = conn.CreateCommand();
                tableCommand.CommandText = String.Format(
                    @"
                        select pk_table = pk.table_name
                        from information_schema.referential_constraints c
                        inner join information_schema.table_constraints fk on c.constraint_name = fk.constraint_name
                        inner join information_schema.table_constraints pk on c.unique_constraint_name = pk.constraint_name
                        inner join information_schema.key_column_usage cu on c.constraint_name = cu.constraint_name
                        inner join (
                        select i1.table_name, i2.column_name
                        from information_schema.table_constraints i1
                        inner join information_schema.key_column_usage i2 on i1.constraint_name = i2.constraint_name
                        where i1.constraint_type = 'PRIMARY KEY'
                        ) pt on pt.table_name = pk.table_name
                        where fk.table_name = '{0}' and cu.column_name = '{1}'",
                    selectedTableName, columnName);
                object referencedTableName = tableCommand.ExecuteScalar();

                return (string) referencedTableName;
            }
        }

        // http://blog.sqlauthority.com/2006/11/01/sql-server-query-to-display-foreign-key-relationships-and-name-of-the-constraint-for-each-table-in-database/
        private IList<HasMany> DetermineHasManyRelationships(Table table)
        {
            var hasManyRelationships = new List<HasMany>();
            var conn = new SqlConnection(connectionStr);
            conn.Open();
            using (conn)
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = conn;
                    command.CommandText =
                        String.Format(
                            @"
                        select DISTINCT
	                        PK_TABLE = b.TABLE_NAME,
	                        FK_TABLE = c.TABLE_NAME
                        from
	                        INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS a
	                        join
	                        INFORMATION_SCHEMA.TABLE_CONSTRAINTS b
	                        on
	                        a.CONSTRAINT_SCHEMA = b.CONSTRAINT_SCHEMA and
	                        a.UNIQUE_CONSTRAINT_NAME = b.CONSTRAINT_NAME
	                        join
	                        INFORMATION_SCHEMA.TABLE_CONSTRAINTS c
	                        on
	                        a.CONSTRAINT_SCHEMA = c.CONSTRAINT_SCHEMA and
	                        a.CONSTRAINT_NAME = c.CONSTRAINT_NAME
                        where
	                        b.TABLE_NAME = '{0}'
                        order by
	                        1,2",
                            table.Name);
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        hasManyRelationships.Add(new HasMany
                                                     {
                                                         Reference = reader.GetString(1)
                                                     });
                    }

                    return hasManyRelationships;
                }
            }
        }
    }
}