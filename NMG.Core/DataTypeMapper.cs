using System;

namespace NMG.Core
{
    public class DataTypeMapper
    {
        public Type MapFromOracle(string dataType)
        {
            if (dataType == "DATE")
            {
                return typeof(DateTime);
            }
            if (dataType == "NUMBER")
            {
                return typeof(long);
            }
            return typeof(string);
        }
    }
}