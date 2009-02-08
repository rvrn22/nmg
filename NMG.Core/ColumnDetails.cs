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
            MappedType = new DataTypeMapper().MapFromOracle(DataType).Name;
        }

        public string ColumnName { get; set; }
        public string DataType { get; set; }
        public string MappedType { get; set; }
    }
}