using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using NMG.Core.Domain;

namespace NMG.Core.Reader
{
    public class SqliteMetadataReader : IMetadataReader
    {
        private readonly string _connectionStr;

        public SqliteMetadataReader(string connectionStr)
        {
            _connectionStr = connectionStr;
        }

        public IList<Column> GetTableDetails(Table table, string owner)
        {
            var columns = new List<Column>();

            using (var sqlCon = new SQLiteConnection(_connectionStr))
            {
                sqlCon.Open();
                try
                {
                    using (var tableDetailsCommand = sqlCon.CreateCommand())
                    {
                        var tbls = sqlCon.GetSchema("columns", new[]{ null, null, table.Name, null, null});
                        tableDetailsCommand.CommandText = string.Format("PRAGMA table_info({0})", table.Name);
                        using (var sqlDataReader = tableDetailsCommand.ExecuteReader(CommandBehavior.Default))
                        {
                            var m = new DataTypeMapper();

                            /*
                             
                                reader index    =   column
                                0                   cid
                                1                   name
                                2                   type (needs to split for data type precision)
                                3                   not null
                                4                   default value
                                5                   pk (0/1)
                             
                             */
                            while (sqlDataReader.Read())
                            {
                                var sqliteDataType = new SqliteDataType(sqlDataReader.GetString(2));
                                columns.Add(
                                    new Column
                                    {
                                        Name = sqlDataReader.GetString(1),
                                        IsNullable = sqlDataReader.GetInt32(3) == 0,
                                        IsPrimaryKey = sqlDataReader.GetBoolean(5),
                                        MappedDataType =m.MapFromDBType(ServerType.SQLite, sqliteDataType.DataType, sqliteDataType.DataLength, null, null).ToString(),
                                        DataLength = sqliteDataType.DataLength, 
                                        DataType = sqliteDataType.DataType
                                    });
                            }

                            table.Columns = columns;
                            table.PrimaryKey = DeterminePrimaryKeys(table);
                            table.ForeignKeys = new List<ForeignKey>();// DetermineForeignKeyReferences(table);
                            table.HasManyRelationships = new List<HasMany>();// DetermineHasManyRelationships(table);
                        }
                    }
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

            using (var sqlCon = new SQLiteConnection(_connectionStr))
            {
                sqlCon.Open();
                try
                {
                    using (var tableDetailsCommand = sqlCon.CreateCommand())
                    {
                        tableDetailsCommand.CommandText =
                            "SELECT name FROM sqlite_master WHERE type in ('table', 'view') AND name not like 'sqlite?_%' escape '?'";
                        using (var sqlDataReader = tableDetailsCommand.ExecuteReader(CommandBehavior.Default))
                        {
                            while (sqlDataReader.Read())
                            {
                                tables.Add(new Table { Name = sqlDataReader.GetString(0) });
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
            return new List<string>();
        }

        public List<string> GetSequences(string owner)
        {
            return new List<string>();
        }

        public PrimaryKey DeterminePrimaryKeys(Table table)
        {
            var primaryKeys = table.Columns.Where(x => x.IsPrimaryKey.Equals(true));

            if (primaryKeys.Count() == 1)
            {
                var c = primaryKeys.First();
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
                foreach (var primaryKey in primaryKeys)
                {
                    key.Columns.Add(primaryKey);
                }
                return key;
            }
        }
    }

    public class SqliteDataType
    {
        public SqliteDataType(string sqliteType)
        {
            if (sqliteType.Contains("("))
            {
                var typeSplit = sqliteType.Replace(")", string.Empty).Split('(');
                DataType = typeSplit[0];
                DataLength = int.Parse( typeSplit[1] );
            }
        }
        public string DataType { get; set; }
        public int? DataLength { get; set; }
    }
}