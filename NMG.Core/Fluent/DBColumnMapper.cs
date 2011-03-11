using System.Text;
using NMG.Core.Domain;
using NMG.Core.TextFormatter;

namespace NMG.Core.Fluent
{
    public class DBColumnMapper
    {
        public string Map(Column column, ITextFormatter Formatter)
        {
            var mappedStrBuilder = new StringBuilder(string.Format("Map(x => x.{0})", Formatter.FormatText(column.Name)));
            mappedStrBuilder.Append(Constants.Dot);
            mappedStrBuilder.Append("Column(\"" + column.Name + "\")");

            if (!column.IsNullable)
            {
                mappedStrBuilder.Append(Constants.Dot);
                mappedStrBuilder.Append("Not.Nullable()");
            }

            if (column.DataLength > 0)
            {
                mappedStrBuilder.Append(Constants.Dot);
                mappedStrBuilder.Append("Length(" + column.DataLength + ")");
            }

            mappedStrBuilder.Append(Constants.SemiColon);
            return mappedStrBuilder.ToString();
        }
    }
}