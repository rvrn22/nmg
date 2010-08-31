using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using NMG.Core.Domain;
using System.Linq;

namespace NMG.Core.Reader
{
    // Type safe enumerator
    public sealed class SqlServerConstraintType
    {
        private readonly String name;
        private readonly int value;

        public static readonly SqlServerConstraintType PrimaryKey = new SqlServerConstraintType(1, "PRIMARY KEY");
        public static readonly SqlServerConstraintType ForeignKey = new SqlServerConstraintType(2, "FOREIGN KEY");
        public static readonly SqlServerConstraintType Check = new SqlServerConstraintType(3, "CHECK");

        private SqlServerConstraintType(int value, String name)
        {
            this.name = name;
            this.value = value;
        }

        public override String ToString()
        {
            return name;
        }
    }

    // http://www.sqlteam.com/forums/topic.asp?TOPIC_ID=72957

    public class SqlServerMetadataReader : IMetadataReader
    {
        private readonly string connectionStr;

        public SqlServerMetadataReader(string connectionStr)
        {
            this.connectionStr = connectionStr;
        }

        public IList<Column> GetTableDetails(Table selectedTableName)
        {
            var columns = new List<Column>();
            var conn = new SqlConnection(connectionStr);
            conn.Open();
            using (conn)
            {
                using (var tableDetailsCommand = conn.CreateCommand())
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
		                        c.table_schema = ccu.table_schema and ccu.table_schema = user
		                        and c.table_name = ccu.table_name
		                        and c.column_name = ccu.column_name
	                        )
                        where c.table_name = '{0}'
                        order by c.table_name, c.ordinal_position", selectedTableName.Name);
                    using (var sqlDataReader = tableDetailsCommand.ExecuteReader(CommandBehavior.Default))
                    {
                        if (sqlDataReader != null)
                        {
                            while (sqlDataReader.Read())
                            {
                                string columnName = sqlDataReader.GetString(0);
                                string dataType = sqlDataReader.GetString(1);
                                bool isNullable = sqlDataReader.GetString(2).Equals("YES", StringComparison.CurrentCultureIgnoreCase);
                                bool isPrimaryKey = 
                                    (!sqlDataReader.IsDBNull(3) ?
                                    sqlDataReader.GetString(3).Equals(SqlServerConstraintType.PrimaryKey.ToString(), StringComparison.CurrentCultureIgnoreCase) :
                                    false);
                                bool isForeignKey =                                     
                                    (!sqlDataReader.IsDBNull(3) ? 
                                    sqlDataReader.GetString(3).Equals(SqlServerConstraintType.ForeignKey.ToString(), StringComparison.CurrentCultureIgnoreCase) :
                                    false);

                                var m = new DataTypeMapper();

                                columns.Add(new Column()
                                {
                                    Name = columnName,
                                    DataType = dataType,
                                    IsNullable = isNullable,
                                    IsPrimaryKey = isPrimaryKey, //IsPrimaryKey(selectedTableName.Name, columnName)
                                    IsForeignKey = isForeignKey, // IsFK()
                                    MappedDataType = m.MapFromDBType(dataType, null, null, null).ToString()
                                    //DataLength = dataLength
                                });

                                selectedTableName.Columns = columns;
                            }
                            selectedTableName.PrimaryKey = DeterminePrimaryKeys(selectedTableName);
                            selectedTableName.ForeignKeys = DetermineForeignKeyReferences(selectedTableName);
                            selectedTableName.HasManyRelationships = DetermineHasManyRelationships(selectedTableName);
                        }
                    }
                }
            }
            return columns;
        }

        private PrimaryKey DeterminePrimaryKeys(Table table)
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
            string foreignKeyReferenceTableName = string.Empty;
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
                        where fk.table_name = '{0}' and cu.column_name = '{1}'", selectedTableName, columnName);
                var referencedTableName = tableCommand.ExecuteScalar();

                return (string)referencedTableName;
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
                    command.CommandText = String.Format(@"
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
	                        1,2", table.Name);
                    var reader = command.ExecuteReader();

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

        //private void MarkPrimaryKeyColumn(string selectedTableName, ColumnDetails columnDetails)
        //{
        //    var conn = new SqlConnection(connectionStr);
        //    conn.Open();
        //    using (conn)
        //    {
        //        using (var constraintCommand = conn.CreateCommand())
        //        {
        //            constraintCommand.CommandText = string.Format("select constraint_name from information_schema.TABLE_CONSTRAINTS where table_name = '{0}' and constraint_type = 'PRIMARY KEY'", selectedTableName);
        //            var value = constraintCommand.ExecuteScalar();
                    
        //            if (value == null) return;

        //            var constraintName = (string)value;
        //            using (var pkColumnCommand = conn.CreateCommand())
        //            {
        //                pkColumnCommand.CommandText = string.Format("select column_name from information_schema.CONSTRAINT_COLUMN_USAGE where table_name = '{0}' and constraint_name = '{1}'", selectedTableName, constraintName);
        //                var colName = pkColumnCommand.ExecuteScalar();
        //                if (colName != null)
        //                {
        //                    var pkColumnName = (string)colName;
        //                    var columnDetail = columnDetails.Find(detail => detail.ColumnName.Equals(pkColumnName));
        //                    columnDetail.IsPrimaryKey = true;
        //                }
        //            }
        //        }
        //    }
        //}

        //private bool IsForeignKey(string selectedTableName, string columnName)
        //{
        //    var conn = new SqlConnection(connectionStr);
        //    conn.Open();
        //    using (conn)
        //    {
        //        using (var constraintCommand = conn.CreateCommand())
        //        {
        //            constraintCommand.CommandText = string.Format("SELECT constraint_name FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE table_name = '{0}' and constraint_type = 'FOREIGN KEY'", selectedTableName);
        //            var value = constraintCommand.ExecuteScalar();
        //            if (value != null)
        //            {
        //                var constraintName = (string)value;
        //                using (var fkColumnCommand = conn.CreateCommand())
        //                {
        //                    fkColumnCommand.CommandText = string.Format("SELECT column_name FROM INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE WHERE table_name = '{0}' and constraint_name = '{1}'", selectedTableName, constraintName);
        //                    var colName = fkColumnCommand.ExecuteScalar();
        //                    if (colName != null)
        //                    {
        //                        var fkColumnName = (string)colName;
        //                        return columnName.Equals(fkColumnName, StringComparison.CurrentCultureIgnoreCase);
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    return false;
        //}

        public List<Table> GetTables()
        {
            var tables = new List<Table>();
            var conn = new SqlConnection(connectionStr);
            conn.Open();
            using (conn)
            {
                var tableCommand = conn.CreateCommand();
                tableCommand.CommandText = "select table_name from information_schema.tables where table_type like 'BASE TABLE'";
                var sqlDataReader = tableCommand.ExecuteReader(CommandBehavior.CloseConnection);
                if (sqlDataReader != null)
                    while (sqlDataReader.Read())
                    {
                        string tableName = sqlDataReader.GetString(0);
                        tables.Add(new Table{Name = tableName});
                    }
            }

            return tables;
        }

//        public string GetForeignKeyReferenceTableName(string tableName)
//        {
//            string foreignKeyReferenceTableName = string.Empty;
//            var conn = new SqlConnection(connectionStr);
//            conn.Open();
//            using (conn)
//            {
//                var tableCommand = conn.CreateCommand();
//                tableCommand.CommandText = @"SELECT PK_Table  = PK.TABLE_NAME, PK_Column = PT.COLUMN_NAME, Constraint_Name = C.CONSTRAINT_NAME 
//                    FROM 
//                            INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS C 
//                            INNER JOIN 
//                            INFORMATION_SCHEMA.TABLE_CONSTRAINTS PK 
//                                ON C.UNIQUE_CONSTRAINT_NAME = PK.CONSTRAINT_NAME 
//                            INNER JOIN 
//                            ( 
//                                SELECT 
//                                    i2.COLUMN_NAME, i2.CONSTRAINT_NAME 
//                                FROM 
//                                    INFORMATION_SCHEMA.TABLE_CONSTRAINTS i1 
//                                    INNER JOIN 
//                                    INFORMATION_SCHEMA.KEY_COLUMN_USAGE i2 
//                                    ON i1.CONSTRAINT_NAME = i2.CONSTRAINT_NAME 
//                                    WHERE i1.CONSTRAINT_TYPE = 'PRIMARY KEY' 
//                            ) PT 
//                            ON PK.CONSTRAINT_NAME = PT.CONSTRAINT_NAME
//                            WHERE PK.TABLE_NAME = '" + tableName + "'";

//                var sqlDataReader = tableCommand.ExecuteReader(CommandBehavior.CloseConnection);
//                if (sqlDataReader != null)
//                    while (sqlDataReader.Read())
//                    {
//                        foreignKeyReferenceTableName = sqlDataReader.GetString(0);
//                    }
//            }
//            return foreignKeyReferenceTableName;
//        }

        public List<string> GetSequences()
        {
            return new List<string>();
        }

        public List<string> GetForeignKeyTables(string primaryKeyColumnName)
        {
            var foreignKeyTables = new List<string>();
            var conn = new SqlConnection(connectionStr);
            conn.Open();
            using (conn)
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = string.Format(@"SELECT KCU.TABLE_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS  TC JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE KCU
                                        ON TC.CONSTRAINT_NAME = KCU.CONSTRAINT_NAME AND TC.CONSTRAINT_SCHEMA = KCU.CONSTRAINT_SCHEMA
                                        WHERE TC.CONSTRAINT_TYPE = 'FOREIGN KEY' AND KCU.COLUMN_NAME = '{0}'", primaryKeyColumnName);
                    var dataReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
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

        public bool UsesCompositeKey(string tableName)
        {
            return false;
        }
    }
}