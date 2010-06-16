using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;
using NMG.Core.Domain;

namespace NMG.Core.Reader
{
    public class OracleMetadataReader : IMetadataReader
    {
        private readonly string connectionStr;

        public OracleMetadataReader(string connectionStr)
        {
            this.connectionStr = connectionStr;
        }

        public ColumnDetails GetTableDetails(string selectedTableName)
        {
            var columnDetails = new ColumnDetails();
            var conn = new OracleConnection(connectionStr);
            conn.Open();
            using (conn)
            {
                using (var tableCommand = conn.CreateCommand())
                {
                    tableCommand.CommandText = "select column_name, data_type, data_length, data_precision, data_scale, nullable from user_tab_cols where table_name = '" + selectedTableName + "'";
                    using (var oracleDataReader = tableCommand.ExecuteReader(CommandBehavior.Default))
                    {
                        while (oracleDataReader.Read())
                        {
                            var columnName = oracleDataReader.GetString(0);
                            var dataType = oracleDataReader.GetString(1);
                            int dataLength = 0;
                            int dataPrecision = 0;
                            int dataScale = 0;
                            string isNullableStr = "N";
                            try
                            {
                                dataLength = oracleDataReader.GetInt32(2);
                                dataPrecision = oracleDataReader.GetInt32(3);
                                dataScale = oracleDataReader.GetInt32(4);
                                isNullableStr = oracleDataReader.GetString(5);
                            }
                            catch (InvalidOperationException)
                            {
                                
                            }
                            bool isNullable = false;
                            if (isNullableStr.Equals("Y", StringComparison.CurrentCultureIgnoreCase))
                            {
                                isNullable = true;
                            }
                            columnDetails.Add(new ColumnDetail(columnName, dataType, dataLength, dataPrecision, 0, dataScale, isNullable));
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
            tables.Sort((x,y) => x.CompareTo(y));
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