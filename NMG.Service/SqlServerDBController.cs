using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using NMG.Core;

namespace NMG.Service
{
    public class SqlServerDBController : DBController
    {
        public SqlServerDBController(string connectionStr) : base(connectionStr)
        {
        }

        public override ColumnDetails GetTableDetails(string selectedTableName)
        {
            var columnDetails = new ColumnDetails();
            var conn = new SqlConnection(connectionStr);
            conn.Open();
            using (conn)
            {

                SqlCommand tableDetailsCommand = conn.CreateCommand();
                tableDetailsCommand.CommandText = "select column_name, data_type, character_maximum_length from information_schema.columns where table_name = '" + selectedTableName + "'";
                SqlDataReader sqlDataReader = tableDetailsCommand.ExecuteReader(CommandBehavior.CloseConnection);
                if (sqlDataReader != null)
                    while (sqlDataReader.Read())
                    {
                        string columnName = sqlDataReader.GetString(0);
                        string dataType = sqlDataReader.GetString(1);
                        columnDetails.Add(new ColumnDetail(columnName, dataType));
                    }
            }
            columnDetails.Sort((x, y) => x.ColumnName.CompareTo(y.ColumnName));
            return columnDetails;
        }

        public override List<string> GetTables()
        {
            var tables = new List<string>();
            var conn = new SqlConnection(connectionStr);
            conn.Open();
            using (conn)
            {
                SqlCommand tableCommand = conn.CreateCommand();
                tableCommand.CommandText = "SELECT * FROM sys.Tables";
                SqlDataReader sqlDataReader = tableCommand.ExecuteReader(CommandBehavior.CloseConnection);
                if (sqlDataReader != null)
                    while (sqlDataReader.Read())
                    {
                        string tableName = sqlDataReader.GetString(0);
                        tables.Add(tableName);
                    }
            }
            tables.Sort((x, y) => x.CompareTo(y));
            return tables;
        }

        public override List<string> GetSequences()
        {
            return new List<string>();
        }
    }
}