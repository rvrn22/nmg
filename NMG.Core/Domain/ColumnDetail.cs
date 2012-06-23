namespace NMG.Core.Domain
{
    public class ColumnDetail
    {
        public ColumnDetail(string columnName, string dataType, int? dataLength, int? dataPrecision,
                            int dataPrecisionRadix, int? dataScale, bool isNullable)
        {
            DataLength = dataLength;
            DataPrecision = dataPrecision;
            DataPrecisionRadix = dataPrecisionRadix;
            DataScale = dataScale;
            ColumnName = columnName;
            DataType = dataType;
            IsNullable = isNullable;
            MappedType = new DataTypeMapper().MapFromDBType(ServerType.SqlServer, DataType, DataLength, DataPrecision, DataScale).Name;
        }

        public bool IsNullable { get; private set; }

        public int? DataLength { get; private set; }
        public int? DataPrecision { get; private set; }
        public int DataPrecisionRadix { get; private set; }
        public int? DataScale { get; private set; }

        public string ColumnName { get; set; }
        public string DataType { get; set; }
        public string MappedType { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsForeignKey { get; set; }
        public string PropertyName { get; set; }
        public string ForeignKeyEntity { get; set; }
    }
}