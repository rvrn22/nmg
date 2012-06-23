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
							@"
						SELECT distinct c.column_name, c .data_type, c.is_nullable, tc.constraint_type, c.numeric_precision, c.numeric_scale, c.character_maximum_length, c.table_name, c.ordinal_position, tc.constraint_name
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
							  and c.table_schema ='{1}'
						order by c.table_name, c.ordinal_position",
							table.Name, owner);

						using (var sqlDataReader = tableDetailsCommand.ExecuteReader(CommandBehavior.Default)) {
							while (sqlDataReader.Read()) {
								string columnName = sqlDataReader.GetString(0);
								string dataType = sqlDataReader.GetString(1);
								bool isNullable = sqlDataReader.GetString(2).Equals("YES", StringComparison.CurrentCultureIgnoreCase);
								var characterMaxLenth = sqlDataReader["character_maximum_length"] as int?;
								var numericPrecision = sqlDataReader["numeric_precision"] as int?;
								var numericScale = sqlDataReader["numeric_scale"] as int?;

								bool isPrimaryKey = (!sqlDataReader.IsDBNull(3) && sqlDataReader.GetString(3).Equals(SqlServerConstraintType.PrimaryKey.ToString(), StringComparison.CurrentCultureIgnoreCase));
								bool isForeignKey = (!sqlDataReader.IsDBNull(3) && sqlDataReader.GetString(3).Equals(SqlServerConstraintType.ForeignKey.ToString(), StringComparison.CurrentCultureIgnoreCase));

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
													MappedDataType = m.MapFromDBType(ServerType.SqlServer, dataType, characterMaxLenth, numericPrecision, numericScale).ToString(),
													DataLength = characterMaxLenth,
													ConstraintName = sqlDataReader["constraint_name"].ToString()
												});

								table.Columns = columns;
							}
							table.PrimaryKey = DeterminePrimaryKeys(table);
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
					tableCommand.CommandText = "SELECT SCHEMA_NAME from INFORMATION_SCHEMA.SCHEMATA";
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

		private static PrimaryKey DeterminePrimaryKeys(Table table)
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
			var constraints = table.Columns.Where(x => x.IsForeignKey.Equals(true)).Select(x => x.ConstraintName).Distinct().ToList();
			var foreignKeys = new List<ForeignKey>(); 
			constraints.ForEach(c =>
									{
										var fkColumns = table.Columns.Where(x => x.ConstraintName.Equals(c)).ToArray();
										var fk = new ForeignKey
													{
														Name = fkColumns[0].Name,
														References = GetForeignKeyReferenceTableName(table.Name, fkColumns[0].Name),
														AllColumnsNamesForTheSameConstraint = fkColumns.Select(f=>f.Name).ToArray()
													};
										foreignKeys.Add(fk);
									});

			Table.SetUniqueNamesForForeignKeyProperties(foreignKeys);

			return foreignKeys;
		}

		private string GetForeignKeyReferenceTableName(string selectedTableName, string columnName)
		{
			object referencedTableName;
			
			var conn = new SqlConnection(connectionStr);
			conn.Open();
			try {
				using (conn) {
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
					referencedTableName = tableCommand.ExecuteScalar();
				}
			} finally {
				conn.Close();
			}
			return (string)referencedTableName;
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