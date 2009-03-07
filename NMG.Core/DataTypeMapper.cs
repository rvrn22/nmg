using System;

namespace NMG.Core
{
    public class DataTypeMapper
    {
        public Type MapFromDBType(string dataType, int? dataLength, int? dataPrecision, int? dataScale)
        {
            if (dataType == "DATE" || dataType == "datetime" || dataType == "TIMESTAMP" || dataType == "TIMESTAMP WITH TIME ZONE" || dataType == "TIMESTAMP WITH LOCAL TIME ZONE")
            {
                return typeof(DateTime);
            }
            if (dataType == "NUMBER" || dataType == "nchar" || dataType == "LONG")
            {
                return typeof(long);
            }
            if (dataType == "int" || dataType == "INTERVAL YEAR TO MONTH" || dataType == "BINARY_INTEGER")
            {
                return typeof(int);
            }
            if (dataType == "BINARY_DOUBLE")
            {
                return typeof(double);
            }
            if (dataType == "BINARY_FLOAT" || dataType == "FLOAT")
            {
                return typeof(float);
            }
            if (dataType == "BLOB" || dataType == "BFILE *" || dataType == "LONG RAW")
            {
                return typeof(byte[]);
            }
            if (dataType == "INTERVAL DAY TO SECOND")
            {
                return typeof(TimeSpan);
            }

            // CHAR, CLOB, NCLOB, NCHAR, XMLType, VARCHAR2
            return typeof(string);
        }
    }
}