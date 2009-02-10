using System;

namespace NMG.Core
{
    public class DataTypeMapper
    {
        public Type MapFromDBType(string dataType)
        {
            if (dataType == "DATE" || dataType == "datetime")
            {
                return typeof(DateTime);
            }
            if (dataType == "NUMBER" || dataType == "nchar")
            {
                return typeof(long);
            }
            return typeof(string);
        }
    }
}