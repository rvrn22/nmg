using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using NMG.Core.Domain;

namespace NMG.Core.Reader
{
    public class SqlServerMetadataReader : IMetadataReader
    {
        private readonly string connectionStr;

        public SqlServerMetadataReader(string connectionStr)
        {
            this.connectionStr = connectionStr;
        }

        public ColumnDetails GetTableDetails(string selectedTableName)
        {
            var columnDetails = new ColumnDetails();
            var conn = new SqlConnection(connectionStr);
            conn.Open();
            using (conn)
            {
                using (var tableDetailsCommand = conn.CreateCommand())
                {
                    tableDetailsCommand.CommandText = "select column_name, data_type, character_maximum_length, NUMERIC_PRECISION, NUMERIC_PRECISION_RADIX, NUMERIC_SCALE, IS_NULLABLE from information_schema.columns where table_name = '" + selectedTableName + "'";
                    using (var sqlDataReader = tableDetailsCommand.ExecuteReader(CommandBehavior.Default))
                    {
                        if (sqlDataReader != null)
                        {
                            while (sqlDataReader.Read())
                            {
                                var columnName = sqlDataReader.GetString(0);
                                var dataType = sqlDataReader.GetString(1);
                                int dataLength = 0;
                                int dataPrecision = 0;
                                int dataPrecisionRadix = 0;
                                int dataScale = 0;
                                bool isNullable = false;
                                try
                                {
                                    dataLength = sqlDataReader.GetInt32(2);
                                    dataPrecision = sqlDataReader.GetInt32(3);
                                    dataPrecisionRadix = sqlDataReader.GetInt32(4);
                                    dataScale = sqlDataReader.GetInt32(5);
                                    isNullable = sqlDataReader.GetBoolean(6);
                                }
                                catch (Exception)
                                {

                                }

                                var columnDetail = new ColumnDetail(columnName, dataType, dataLength, dataPrecision, dataPrecisionRadix, dataScale, isNullable)
                                {
                                    IsForeignKey = IsForeignKey(selectedTableName, columnName),
                                    ForeignKeyEntity = GetForeignKeyReferenceTableName(selectedTableName)
                                };
                                columnDetails.Add(columnDetail);
                            }
                        }
                    }
                }

                MarkPrimaryKeyColumn(selectedTableName, columnDetails);
            }
            columnDetails.Sort((x, y) => x.ColumnName.CompareTo(y.ColumnName));
            return columnDetails;
        }

        private void MarkPrimaryKeyColumn(string selectedTableName, ColumnDetails columnDetails)
        {
            var conn = new SqlConnection(connectionStr);
            conn.Open();
            using (conn)
            {
                using (var constraintCommand = conn.CreateCommand())
                {
                    constraintCommand.CommandText = "select constraint_name from information_schema.TABLE_CONSTRAINTS where table_name = '" + selectedTableName + "' and constraint_type = 'PRIMARY KEY'";
                    var value = constraintCommand.ExecuteScalar();
                    if (value != null)
                    {
                        var constraintName = (string)value;
                        using (var pkColumnCommand = conn.CreateCommand())
                        {
                            pkColumnCommand.CommandText = "select column_name from information_schema.CONSTRAINT_COLUMN_USAGE where table_name = '" + selectedTableName + "' and constraint_name = '" + constraintName + "'";
                            var colName = pkColumnCommand.ExecuteScalar();
                            if (colName != null)
                            {
                                var pkColumnName = (string)colName;
                                var columnDetail = columnDetails.Find(detail => detail.ColumnName.Equals(pkColumnName));
                                columnDetail.IsPrimaryKey = true;
                            }
                        }
                    }
                }
            }
        }

        private bool IsForeignKey(string selectedTableName, string columnName)
        {
            var conn = new SqlConnection(connectionStr);
            conn.Open();
            using (conn)
            {
                using (var constraintCommand = conn.CreateCommand())
                {
                    constraintCommand.CommandText = "select constraint_name from information_schema.TABLE_CONSTRAINTS where table_name = '" + selectedTableName + "' and constraint_type = 'FOREIGN KEY'";
                    var value = constraintCommand.ExecuteScalar();
                    if (value != null)
                    {
                        var constraintName = (string)value;
                        using (var fkColumnCommand = conn.CreateCommand())
                        {
                            fkColumnCommand.CommandText = "select column_name from information_schema.CONSTRAINT_COLUMN_USAGE where table_name = '" + selectedTableName + "' and constraint_name = '" + constraintName + "'";
                            var colName = fkColumnCommand.ExecuteScalar();
                            if (colName != null)
                            {
                                var fkColumnName = (string)colName;
                                return columnName.Equals(fkColumnName, StringComparison.CurrentCultureIgnoreCase);
                            }
                        }
                    }
                }
            }

            return false;
        }

        public List<string> GetTables()
        {
            var tables = new List<string>();
            var conn = new SqlConnection(connectionStr);
            conn.Open();
            using (conn)
            {
                var tableCommand = conn.CreateCommand();
                tableCommand.CommandText = "SELECT * FROM sys.Tables";
                var sqlDataReader = tableCommand.ExecuteReader(CommandBehavior.CloseConnection);
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


        public string GetForeignKeyReferenceTableName(string tableName)
        {
            string foreignKeyReferenceTableName = string.Empty;
            var conn = new SqlConnection(connectionStr);
            conn.Open();
            using (conn)
            {
                var tableCommand = conn.CreateCommand();
                tableCommand.CommandText = @"SELECT PK_Table  = PK.TABLE_NAME, PK_Column = PT.COLUMN_NAME, Constraint_Name = C.CONSTRAINT_NAME 
                    FROM 
                            INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS C 
                            INNER JOIN 
                            INFORMATION_SCHEMA.TABLE_CONSTRAINTS PK 
                                ON C.UNIQUE_CONSTRAINT_NAME = PK.CONSTRAINT_NAME 
                            INNER JOIN 
                            ( 
                                SELECT 
                                    i2.COLUMN_NAME, i2.CONSTRAINT_NAME 
                                FROM 
                                    INFORMATION_SCHEMA.TABLE_CONSTRAINTS i1 
                                    INNER JOIN 
                                    INFORMATION_SCHEMA.KEY_COLUMN_USAGE i2 
                                    ON i1.CONSTRAINT_NAME = i2.CONSTRAINT_NAME 
                                    WHERE i1.CONSTRAINT_TYPE = 'PRIMARY KEY' 
                            ) PT 
                            ON PK.CONSTRAINT_NAME = PT.CONSTRAINT_NAME
                            WHERE PK.TABLE_NAME = '" + tableName + "'";

                var sqlDataReader = tableCommand.ExecuteReader(CommandBehavior.CloseConnection);
                if (sqlDataReader != null)
                    while (sqlDataReader.Read())
                    {
                        foreignKeyReferenceTableName = sqlDataReader.GetString(0);
                    }
            }
            return foreignKeyReferenceTableName;
        }

        public List<string> GetSequences()
        {
            return new List<string>();
        }
    }
}