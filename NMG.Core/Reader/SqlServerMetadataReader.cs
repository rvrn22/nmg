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
			try {
				using (conn) {
					using (var tableDetailsCommand = conn.CreateCommand()) {
						tableDetailsCommand.CommandText = string.Format(
                            @"SELECT distinct c.column_name, c.data_type, c.is_nullable, tc.constraint_type, convert(int,c.numeric_precision) numeric_precision, c.numeric_scale, c.character_maximum_length, c.table_name, c.ordinal_position, tc.constraint_name,
       columnproperty(object_id(c.table_schema + '.' + c.table_name), c.column_name,'IsIdentity') IsIdentity, 
       (SELECT CASE WHEN count(1) = 0 THEN 0 ELSE 1 END 
       FROM information_schema.table_constraints x 
       INNER JOIN information_schema.constraint_column_usage ccux ON c.table_name = ccux.table_name and c.column_name = ccux.column_name and c.table_schema = ccux.table_schema
       WHERE x.constraint_type = 'UNIQUE' and x.table_schema = ccux.table_schema and x.constraint_name = ccux.constraint_name) IsUnique
from information_schema.columns c
	left outer join (
		information_schema.constraint_column_usage ccu
		join information_schema.table_constraints tc on (
			tc.table_schema = ccu.table_schema
			and tc.constraint_name = ccu.constraint_name
			and NOT tc.constraint_type IN ('CHECK','UNIQUE')
		)
	) on (
		c.table_schema = ccu.table_schema and ccu.table_schema = 'dbo'
		and c.table_name = ccu.table_name
		and c.column_name = ccu.column_name
	)
						where c.table_name = '{0}'
							  and c.table_schema ='{1}'
						order by c.table_name, c.ordinal_position",
							table.Name, owner);

						using (var sqlDataReader = tableDetailsCommand.ExecuteReader(CommandBehavior.Default)) {
							while (sqlDataReader.Read()) {
								string columnName = sqlDataReader.GetString(0);
								string dataType = sqlDataReader.GetString(1);
								bool isNullable = sqlDataReader.GetString(2).Equals("YES", StringComparison.CurrentCultureIgnoreCase);
							    var isIdentity = Convert.ToBoolean(sqlDataReader["IsIdentity"]);
								var characterMaxLenth = sqlDataReader["character_maximum_length"] as int?;
								var numericPrecision = sqlDataReader["numeric_precision"] as int?;
								var numericScale = sqlDataReader["numeric_scale"] as int?;
							    var constraintName = sqlDataReader["constraint_name"].ToString();
							    var isUnique = Convert.ToBoolean(sqlDataReader["IsUnique"]);
								bool isPrimaryKey = (!sqlDataReader.IsDBNull(3) && sqlDataReader.GetString(3).Equals(SqlServerConstraintType.PrimaryKey.ToString(), StringComparison.CurrentCultureIgnoreCase));
								bool isForeignKey = (!sqlDataReader.IsDBNull(3) && sqlDataReader.GetString(3).Equals(SqlServerConstraintType.ForeignKey.ToString(), StringComparison.CurrentCultureIgnoreCase));

								var m = new DataTypeMapper();

								columns.Add(new Column
												{
													Name = columnName,
													DataType = dataType,
													IsNullable = isNullable,
                                                    IsIdentity = isIdentity,
													IsPrimaryKey = isPrimaryKey,
													IsForeignKey = isForeignKey,
                                                    IsUnique = isUnique,
													MappedDataType = m.MapFromDBType(ServerType.SqlServer, dataType, characterMaxLenth, numericPrecision, numericScale).ToString(),
													DataLength = characterMaxLenth,
                                                    DataScale = numericScale,
                                                    DataPrecision = numericPrecision,
													ConstraintName = constraintName
												});

								table.Columns = columns;
							}
						    table.Owner = owner;
							table.PrimaryKey = DeterminePrimaryKeys(table);

                            // Need to find the table name associated with the FK
						    foreach (var c in table.Columns)
						    {
						        if (c.IsForeignKey)
						        {
						            string referencedTableName;
						            string referencedColumnName;
						            GetForeignKeyReferenceDetails(c.ConstraintName, out referencedTableName, out referencedColumnName);

						            c.ForeignKeyTableName = referencedTableName;
						            c.ForeignKeyColumnName = referencedColumnName;
						        }
						    }
							table.ForeignKeys = DetermineForeignKeyReferences(table);
							table.HasManyRelationships = DetermineHasManyRelationships(table);
						}
					}
				}
			} finally {
				conn.Close();
			}
			
			return columns;
		}

		public IList<string> GetOwners()
		{
			var owners = new List<string>();
			var conn = new SqlConnection(connectionStr);
			conn.Open();
			try {
				using (conn) {
					var tableCommand = conn.CreateCommand();
                    tableCommand.CommandText = "SELECT SCHEMA_NAME from INFORMATION_SCHEMA.SCHEMATA ORDER BY SCHEMA_NAME";
					var sqlDataReader = tableCommand.ExecuteReader(CommandBehavior.CloseConnection);
					while (sqlDataReader.Read()) {
						var ownerName = sqlDataReader.GetString(0);
						owners.Add(ownerName);
					}
				}
			} finally {
				conn.Close();
			}
			return owners;
		}

		public List<Table> GetTables(string owner)
		{
			var tables = new List<Table>();
			var conn = new SqlConnection(connectionStr);
			conn.Open();
			try {
				using (conn) {
					var tableCommand = conn.CreateCommand();
					tableCommand.CommandText = String.Format("select table_name from information_schema.tables where table_type in ('BASE TABLE','VIEW') AND TABLE_SCHEMA = '{0}'", owner);
					var sqlDataReader = tableCommand.ExecuteReader(CommandBehavior.CloseConnection);
					while (sqlDataReader.Read()) {
						var tableName = sqlDataReader.GetString(0);
						tables.Add(new Table { Name = tableName });
					}
				}
				tables.Sort((x, y) => String.CompareOrdinal(x.Name, y.Name));
			} finally {
				conn.Close();
			}
			return tables;
		}

        public List<string> GetSequences(string owner)
        {
            return new List<string>();
        }

		#endregion

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
                                  Columns =  primaryKeys
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

		private void GetForeignKeyReferenceDetails(string constraintName, out string referencedTableName, out string referencedColumnName)
		{
			referencedTableName = string.Empty;
		    referencedColumnName = string.Empty;
			
			var conn = new SqlConnection(connectionStr);
			conn.Open();
			try {
			    using (conn)
			    {
			        using (var tableDetailsCommand = conn.CreateCommand())
			        {

			            SqlCommand tableCommand = conn.CreateCommand();
			            tableDetailsCommand.CommandText = String.Format(
			                @"
SELECT  
     KCU1.CONSTRAINT_NAME AS FK_CONSTRAINT_NAME 
    ,KCU1.TABLE_NAME AS FK_TABLE_NAME 
    ,KCU1.COLUMN_NAME AS FK_COLUMN_NAME 
    ,KCU1.ORDINAL_POSITION AS FK_ORDINAL_POSITION 
    ,KCU2.CONSTRAINT_NAME AS REFERENCED_CONSTRAINT_NAME 
    ,KCU2.TABLE_NAME AS REFERENCED_TABLE_NAME 
    ,KCU2.COLUMN_NAME AS REFERENCED_COLUMN_NAME 
    ,KCU2.ORDINAL_POSITION AS REFERENCED_ORDINAL_POSITION 
FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS AS RC 

LEFT JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS KCU1 
    ON KCU1.CONSTRAINT_CATALOG = RC.CONSTRAINT_CATALOG  
    AND KCU1.CONSTRAINT_SCHEMA = RC.CONSTRAINT_SCHEMA 
    AND KCU1.CONSTRAINT_NAME = RC.CONSTRAINT_NAME 

LEFT JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS KCU2 
    ON KCU2.CONSTRAINT_CATALOG = RC.UNIQUE_CONSTRAINT_CATALOG  
    AND KCU2.CONSTRAINT_SCHEMA = RC.UNIQUE_CONSTRAINT_SCHEMA 
    AND KCU2.CONSTRAINT_NAME = RC.UNIQUE_CONSTRAINT_NAME 
    AND KCU2.ORDINAL_POSITION = KCU1.ORDINAL_POSITION 
   WHERE KCU1.CONSTRAINT_NAME = '{0}'",
			                constraintName);

			            using (var sqlDataReader = tableDetailsCommand.ExecuteReader(CommandBehavior.Default))
			            {
			                while (sqlDataReader.Read())
			                {
			                    referencedTableName = sqlDataReader["REFERENCED_TABLE_NAME"].ToString();
			                    referencedColumnName = sqlDataReader["REFERENCED_COLUMN_NAME"].ToString();
			                }
			            }
			        }
			    }
			} finally {
				conn.Close();
			}
		}

		// http://blog.sqlauthority.com/2006/11/01/sql-server-query-to-display-foreign-key-relationships-and-name-of-the-constraint-for-each-table-in-database/
		private IList<HasMany> DetermineHasManyRelationships(Table table)
		{
			var hasManyRelationships = new List<HasMany>();
			var conn = new SqlConnection(connectionStr);
			conn.Open();
			try {
				using (conn) {
					using (var command = new SqlCommand()) {
						command.Connection = conn;
						command.CommandText =
							String.Format(
								@"
						SELECT DISTINCT 
							PK_TABLE = b.TABLE_NAME,
							FK_TABLE = c.TABLE_NAME,
							FK_COLUMN_NAME = d.COLUMN_NAME,
							CONSTRAINT_NAME = a.CONSTRAINT_NAME
						FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS a 
						  JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS b ON a.CONSTRAINT_SCHEMA = b.CONSTRAINT_SCHEMA AND a.UNIQUE_CONSTRAINT_NAME = b.CONSTRAINT_NAME 
						  JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS c ON a.CONSTRAINT_SCHEMA = c.CONSTRAINT_SCHEMA AND a.CONSTRAINT_NAME = c.CONSTRAINT_NAME
						  JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE d on a.CONSTRAINT_NAME = d.CONSTRAINT_NAME
						WHERE b.TABLE_NAME = '{0}'
						ORDER BY 1,2",
								table.Name);
						SqlDataReader reader = command.ExecuteReader();

						while (reader.Read()) {
							var constraintName = reader["CONSTRAINT_NAME"].ToString();
							var fkColumnName = reader["FK_COLUMN_NAME"].ToString();
							var existing = hasManyRelationships.FirstOrDefault(hm => hm.ConstraintName == constraintName);
							if (existing == null) {
								var newHasManyItem = new HasMany
												{
													ConstraintName = constraintName,
													Reference = reader.GetString(1)
												};
								newHasManyItem.AllReferenceColumns.Add(fkColumnName);
								hasManyRelationships.Add(newHasManyItem);

							} else {
								existing.AllReferenceColumns.Add(fkColumnName);
							}
						}
					}
				}
			} finally {
				conn.Close();
			}
			return hasManyRelationships;
		}
	}
}