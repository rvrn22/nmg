using System.Text;
using NMG.Core.Domain;

namespace NMG.Core.Fluent
{
    public class DBColumnMapper
    {
        public string Map(ColumnDetail columnDetail)
        {
            var mappedStrBuilder = new StringBuilder(string.Format("Map(x => x.{0})", columnDetail.ColumnName));
            if (columnDetail.DataLength > 0)
            {
                mappedStrBuilder.Append(Constants.Dot);
                mappedStrBuilder.Append("Length(" + columnDetail.DataLength + ")");
            }
            if(!columnDetail.IsNullable)
            {
                mappedStrBuilder.Append(Constants.Dot);
                mappedStrBuilder.Append("Not.Nullable()");
            }
            mappedStrBuilder.Append(Constants.SemiColon);
            return mappedStrBuilder.ToString();
        }
    }
}