using System;
using NMG.Core.Domain;

namespace NMG.Core
{
    public class DataTypeMapper
    {
        public Type MapFromDBType(ServerType serverType, string dataType, int? dataLength, int? dataPrecision, int? dataScale)
        {
            switch (serverType)
            {
                case ServerType.SqlServer:
                    return MapFromSqlServerDBType(dataType, dataLength, dataPrecision, dataScale);
                case ServerType.Oracle:
                    return MapFromOracleDBType(dataType, dataLength, dataPrecision, dataScale);
                case ServerType.MySQL:
                    return MapFromMySqlDBType(dataType, dataLength, dataPrecision, dataScale);
                case ServerType.PostgreSQL:
                    return MapFromPostgreDBType(dataType, dataLength, dataPrecision, dataScale);
            }
            return MapFromDBType(dataType, dataLength, dataPrecision, dataScale);
        }
        
        //http://msdn.microsoft.com/en-us/library/cc716729.aspx
        private Type MapFromSqlServerDBType(string dataType, int? dataLength, int? dataPrecision, int? dataScale)
        {
            return MapFromDBType(dataType, dataLength, dataPrecision, dataScale); 
        }
        
        //http://docs.oracle.com/cd/B28359_01/win.111/b28376/appendixa.htm
        private Type MapFromOracleDBType(string dataType, int? dataLength, int? dataPrecision, int? dataScale)
        {
            if (string.Equals(dataType, "DATE", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(dataType, "TIMESTAMP", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(dataType, "TIMESTAMP WITH TIME ZONE", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(dataType, "TIMESTAMP WITH LOCAL TIME ZONE", StringComparison.OrdinalIgnoreCase))
            {
                return typeof (System.DateTime);
            }
            
            if (string.Equals(dataType, "NUMBER", StringComparison.OrdinalIgnoreCase))
            {
                if (dataScale.GetValueOrDefault(0) == 0) //Integer type
                {
                    if (dataPrecision.GetValueOrDefault(0) >= 1 && dataPrecision.GetValueOrDefault(0) <= 4)
                        return typeof (System.Int16);
                    if (dataPrecision.GetValueOrDefault(0) >= 5 && dataPrecision.GetValueOrDefault(0) <= 9)
                        return typeof (System.Int32);
                    if (dataPrecision.GetValueOrDefault(0) >= 10 && dataPrecision.GetValueOrDefault(0) <= 18)
                        return typeof (System.Int64);
                }
                if (dataScale.GetValueOrDefault(0) > 0) //Floating type
                {
                    if (dataPrecision.GetValueOrDefault(0) >= 1 && dataPrecision.GetValueOrDefault(0) <= 7)
                        return typeof (System.Single);
                    if (dataPrecision.GetValueOrDefault(0) >= 8 && dataPrecision.GetValueOrDefault(0) <= 15)
                        return typeof (System.Double);
                }
                return typeof (System.Decimal);
            }
            
            if (string.Equals(dataType, "BLOB", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(dataType, "BFILE *", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(dataType, "LONG RAW", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(dataType, "RAW", StringComparison.OrdinalIgnoreCase))
            {
                return typeof (byte[]);
            }
            
            if (string.Equals(dataType, "INTERVAL DAY TO SECOND", StringComparison.OrdinalIgnoreCase))
            {
                return typeof (System.TimeSpan);
            }
            
            if (string.Equals(dataType, "INTERVAL YEAR TO MONTH", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(dataType, "LONG", StringComparison.OrdinalIgnoreCase))
            {
                return typeof (System.Int64);
            }
            
            if (string.Equals(dataType, "BINARY_FLOAT", StringComparison.OrdinalIgnoreCase))
            {
                return typeof (System.Single);
            }
            
            if (string.Equals(dataType, "BINARY_DOUBLE", StringComparison.OrdinalIgnoreCase))
            {
                return typeof (System.Double);
            }
            
            if (string.Equals(dataType, "BINARY_INTEGER", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(dataType, "FLOAT", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(dataType, "REAL", StringComparison.OrdinalIgnoreCase))
            {
                return typeof (System.Decimal);
            }
            
            return typeof (System.String);
        }
        
        private Type MapFromMySqlDBType(string dataType, int? dataLength, int? dataPrecision, int? dataScale)
        {
            return MapFromDBType(dataType, dataLength, dataPrecision, dataScale);
        }
        
        private Type MapFromPostgreDBType(string dataType, int? dataLength, int? dataPrecision, int? dataScale)
        {
            return MapFromDBType(dataType, dataLength, dataPrecision, dataScale);
        }
        
        private Type MapFromDBType(string dataType, int? dataLength, int? dataPrecision, int? dataScale)
        {
            if (dataType == "DATE" ||dataType == "date" || dataType == "datetime"|| dataType == "datetime2" || dataType == "TIMESTAMP" ||
                dataType == "TIMESTAMP WITH TIME ZONE" || dataType == "TIMESTAMP WITH LOCAL TIME ZONE" ||
                dataType == "smalldatetime")
            {
                return typeof(DateTime);
            }
         if (dataType == "NUMBER" || dataType == "LONG" || dataType == "bigint" )
            {
                return typeof(long);
            }
            if (dataType == "smallint")
            {
                return typeof(Int16);
            }
            if (dataType == "tinyint")
            {
                return typeof(Byte);
            }
            if (dataType == "int" || dataType == "INTERVAL YEAR TO MONTH" || dataType == "BINARY_INTEGER" || dataType.Contains("int"))
            {
                return typeof(int);
            }
            if (dataType == "BINARY_DOUBLE" || dataType == "float" || dataType == "numeric")
            {
                return typeof(double);
            }
            if (dataType == "BINARY_FLOAT" || dataType == "FLOAT")
            {
                return typeof(float);
            }
            if (dataType == "BLOB" || dataType == "BFILE *" || dataType == "LONG RAW" || dataType == "binary" ||
                dataType == "image" || dataType == "timestamp" || dataType == "varbinary")
            {
                return typeof(byte[]);
            }
            if (dataType == "INTERVAL DAY TO SECOND")
            {
                return typeof(TimeSpan);
            }
            if (dataType == "bit")
            {
                return typeof(Boolean);
            }
            if (dataType == "decimal" || dataType == "money" || dataType == "smallmoney" || dataType == "numeric")
            {
                return typeof(decimal);
            }
            if (dataType == "real")
            {
                return typeof(Single);
            }
            if (dataType == "uniqueidentifier")
            {
                return typeof(Guid);
            }

            // CHAR, CLOB, NCLOB, NCHAR, XMLType, VARCHAR2, nchar, ntext
            return typeof(string);
        }
    }
}