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

        public override ColumnDetails GetTableDetails(string selectedTableName)
        {
            var columnDetails = new ColumnDetails();
            var conn = new OracleConnection(connectionStr);
            conn.Open();
            using (conn)
            {
                using (var tableCommand = conn.CreateCommand())
                {
                    tableCommand.CommandText = "select column_name, data_type from user_tab_cols where table_name = '" + selectedTableName + "'";
                    using (var oracleDataReader = tableCommand.ExecuteReader(CommandBehavior.Default))
                    {
                        while (oracleDataReader.Read())
                        {
                            var columnName = oracleDataReader.GetString(0);
                            var dataType = oracleDataReader.GetString(1);
                            columnDetails.Add(new ColumnDetail(columnName, dataType));
                        }
                    }
                }
                using(var constraintCommand = conn.CreateCommand())
                {
                    constraintCommand.CommandText = "select constraint_name from user_constraints where table_name = '" + selectedTableName + "' and constraint_type = 'P'";
                    var value = constraintCommand.ExecuteOracleScalar();
                    if (value != null)
                    {
                        var constraintName = (OracleString) value;
                        using(var pkColumnCommand = conn.CreateCommand())
                        {
                            pkColumnCommand.CommandText = "select column_name from user_cons_columns where table_name = '" + selectedTableName+ "' and constraint_name = '"+ constraintName.Value +"'";
                            var colName = pkColumnCommand.ExecuteOracleScalar();
                            if(colName != null)
                            {
                                var pkColumnName = (OracleString)colName;
                                var columnDetail = columnDetails.Find(detail => detail.ColumnName.Equals(pkColumnName.Value));
                                columnDetail.IsPrimaryKey = true;
                            }
                        }
                    }
                }
            }
            columnDetails.Sort((x, y) => x.ColumnName.CompareTo(y.ColumnName));
            return columnDetails;
        }

        public override List<string> GetTables()
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
            tables.Sort((x,y) => x.CompareTo(y));
            return tables;
        }

        public override List<string> GetSequences()
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