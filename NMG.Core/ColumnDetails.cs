using System.Collections.Generic;

namespace NMG.Core
{
    public class ColumnDetails : List<ColumnDetail>
    {
    }

    public class ColumnDetail
    {
        public ColumnDetail(string columnName, string dataType)
        {
            ColumnName = columnName;
            DataType = dataType;
        }

        public string ColumnName { get; set; }
        public string DataType { get; set; }
        public string MappedType
        {
            get
            {
                return new DataTypeMapper().MapFromOracle(DataType).Name;
            }
        }
    }
}