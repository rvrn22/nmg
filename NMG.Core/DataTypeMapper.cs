using System;

namespace NMG.Core
{
    public class DataTypeMapper
    {
        public Type MapFromDBType(string dataType, int? dataLength, int? dataPrecision, int? dataScale)
        {
            if (dataType == "DATE" || dataType == "datetime" || dataType == "TIMESTAMP" || dataType == "TIMESTAMP WITH TIME ZONE" || dataType == "TIMESTAMP WITH LOCAL TIME ZONE" || dataType == "smalldatetime")
            {
                return typeof(DateTime);
            }
            if (dataType == "NUMBER" || dataType == "nchar" || dataType == "LONG" || dataType == "bigint")
            {
                return typeof(long);
            }
            if (dataType == "int" || dataType == "INTERVAL YEAR TO MONTH" || dataType == "BINARY_INTEGER")
            {
                return typeof(int);
            }
            if (dataType == "BINARY_DOUBLE" || dataType == "float")
            {
                return typeof(double);
            }
            if (dataType == "BINARY_FLOAT" || dataType == "FLOAT")
            {
                return typeof(float);
            }
            if (dataType == "BLOB" || dataType == "BFILE *" || dataType == "LONG RAW" || dataType == "binary" || dataType == "image" || dataType == "timestamp" || dataType == "varbinary")
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
            if (dataType == "decimal" || dataType == "money" || dataType == "smallmoney")
            {
                return typeof(decimal);
            }
            if (dataType == "real")
            {
                return typeof(Single);
            }
            if (dataType == "smallint")
            {
                return typeof(Int16);
            }
            if (dataType == "uniqueidentifier")
            {
                return typeof(Guid);
            }
            if (dataType == "tinyint")
            {
                return typeof(Byte);
            }

            // CHAR, CLOB, NCLOB, NCHAR, XMLType, VARCHAR2, nchar, ntext
            return typeof(string);
        }
    }
}