namespace NMG.Core.Domain
{
    public class ColumnDetail
    {
        public ColumnDetail(string columnName, string dataType)
        {
            ColumnName = columnName;
            DataType = dataType;
            MappedType = new DataTypeMapper().MapFromDBType(DataType).Name;
        }

        public string ColumnName { get; set; }
        public string DataType { get; set; }
        public string MappedType { get; set; }
        public bool IsPrimaryKey { get; set; }
    }
}