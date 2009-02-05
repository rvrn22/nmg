using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;
using NMG.Core;

namespace NMG.Service
{
    public class OracleDBController : DBController
    {
        public OracleDBController(string connectionStr) : base(connectionStr)
        {
        }

        public ColumnDetails GetTableDetails(string selectedTableName)
        {
            var columnDetails = new ColumnDetails();
            var conn = new OracleConnection(connectionStr);
            conn.Open();
            using (conn)
            {

                OracleCommand tableCommand = conn.CreateCommand();
                tableCommand.CommandText = "select * from user_tab_cols where table_name = '" + selectedTableName + "'";
                OracleDataReader oracleDataReader = tableCommand.ExecuteReader(CommandBehavior.CloseConnection);
                while (oracleDataReader.Read())
                {
                    string columnName = oracleDataReader.GetString(1);
                    string dataType = oracleDataReader.GetString(2);
                    columnDetails.Add(new ColumnDetail(columnName, dataType));
                }
            }
            return columnDetails;
        }

        public List<string> GetTables()
        {
            var tables = new List<string>();
            var conn = new OracleConnection(connectionStr);
            conn.Open();
            using (conn)
            {
                OracleCommand tableCommand = conn.CreateCommand();
                tableCommand.CommandText = "select * from user_tables";
                OracleDataReader oracleDataReader = tableCommand.ExecuteReader(CommandBehavior.CloseConnection);
                while (oracleDataReader.Read())
                {
                    string tableName = oracleDataReader.GetString(0);
                    tables.Add(tableName);
                }
            }
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
                seqCommand.CommandText = "select * from user_sequences";
                OracleDataReader seqReader = seqCommand.ExecuteReader(CommandBehavior.CloseConnection);
                while (seqReader.Read())
                {
                    string tableName = seqReader.GetString(0);
                    sequences.Add(tableName);
                }
            }
            return sequences;
        }
    }
}