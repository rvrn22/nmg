using System.Collections.Generic;

namespace NHibernateMappingGenerator
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
    }
}