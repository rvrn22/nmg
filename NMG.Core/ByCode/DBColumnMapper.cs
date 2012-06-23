using System;
using NMG.Core.Domain;
using NMG.Core.TextFormatter;
using System.Text;

namespace NMG.Core.ByCode
{
    public class DBColumnMapper
    {
//Id(x => x.Id, map =>
//{
//    map.Column("ID");
//    map.Generator(Generators.Sequence, g => g.Params(new { sequence = "TABLE_SEQ" }));
//});
        public string IdSequenceMap(Column column, string sequenceName, ITextFormatter formatter)
        {
            var builder = new StringBuilder();
            builder.AppendFormat("Id(x => x.{0}, map => ", formatter.FormatText(column.Name));
            builder.AppendLine();
            builder.AppendLine("\t\t\t\t{");
            builder.AppendLine("\t\t\t\t\tmap.Column(\"" + column.Name + "\");");
            builder.AppendLine("\t\t\t\t\tmap.Generator(Generators.Sequence, g => g.Params(new { sequence = \"" + sequenceName + "\" }));");
            builder.Append("\t\t\t\t});");
            return builder.ToString();
        }

        public string IdMap (Column column, ITextFormatter formatter)
        {
            var builder = new StringBuilder();
            builder.AppendFormat("Id(x => x.{0}, map => ", formatter.FormatText(column.Name));
            builder.AppendLine();
            builder.AppendLine("\t\t\t\t{");
            builder.AppendLine("\t\t\t\t\tmap.Column(\"" + column.Name + "\");");
            builder.Append("\t\t\t\t});");
            return builder.ToString();
        }
        
            
//Property(x => x.Name, map =>
//                {
//                    map.Column("NAME");
//                    map.NotNullable(true);
//                    map.Length(200);
//                });
        public string Map(Column column, ITextFormatter formatter)
        {
            var mappedStrBuilder = new StringBuilder();
            mappedStrBuilder.AppendFormat("Property(x => x.{0}, map => ", formatter.FormatText(column.Name));
            mappedStrBuilder.AppendLine();
            mappedStrBuilder.AppendLine("\t\t\t\t{");
            mappedStrBuilder.AppendLine("\t\t\t\t\tmap.Column(\"" + column.Name + "\");");
            
            if (!column.IsNullable)
            {
                mappedStrBuilder.AppendLine("\t\t\t\t\tmap.NotNullable(true);");
            }
            if (column.DataLength > 0)
            {
                mappedStrBuilder.AppendLine("\t\t\t\t\tmap.Length(" + column.DataLength + ");");
            }
            if (column.DataPrecision.GetValueOrDefault(0) > 0)
            {
                mappedStrBuilder.AppendLine("\t\t\t\t\tmap.Precision(" + column.DataPrecision + ");");
            }
            if (column.DataScale.GetValueOrDefault(0) > 0)
            {
                mappedStrBuilder.AppendLine("\t\t\t\t\tmap.Scale(" + column.DataScale + ");");
            }
            mappedStrBuilder.Append("\t\t\t\t});");
            return mappedStrBuilder.ToString();
        }
        
        public string Bag(HasMany hasMany, ITextFormatter formatter)
        {
            var builder = new StringBuilder();
            builder.Append("\t\t\tBag<" + formatter.FormatSingular(hasMany.Reference) + ">(x => x." + formatter.FormatPlural(hasMany.Reference) + ", colmap =>  { colmap.Key(x => x.Column(\"" + hasMany.ReferenceColumn + "\"));  }, map => { map.OneToMany(x => x.Class(typeof(" + formatter.FormatSingular(hasMany.Reference) + "))); });");
            return builder.ToString();
        }
        
        public string Reference(ForeignKey fk, ITextFormatter formatter)
        {
            var builder = new StringBuilder();
            builder.Append("\t\t\tManyToOne<" + formatter.FormatSingular(fk.UniquePropertyName) + ">(x => x." + formatter.FormatSingular(fk.UniquePropertyName) + ", map => { map.Column(\"" + fk.Name + "\"); });");
            return builder.ToString();
        }
    }
}

