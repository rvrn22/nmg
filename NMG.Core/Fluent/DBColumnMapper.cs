using NMG.Core.Domain;

namespace NMG.Core.Fluent
{
    public class DBColumnMapper
    {
        public string Map(ColumnDetail columnDetail)
        {
            var mappedStr = string.Format("Map(x => x.{0})", columnDetail.ColumnName);
            if (columnDetail.DataLength > 0)
            {
                mappedStr += Constants.Dot;
                mappedStr += "Length(" + columnDetail.DataLength + ")";
            }
            if(!columnDetail.IsNullable)
            {
                mappedStr += Constants.Dot;
                mappedStr += "Not.Nullable()";
            }
            return mappedStr + Constants.SemiColon;
        }
    }
}