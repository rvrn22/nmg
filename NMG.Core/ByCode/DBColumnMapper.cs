using System;
using System.Collections.Generic;
using System.Linq;
using NMG.Core.Domain;
using NMG.Core.TextFormatter;
using System.Text;

namespace NMG.Core.ByCode
{
    public class DBColumnMapper
    {
        private readonly Language _language;

        public DBColumnMapper(Language language)
        {
            _language = language;
        }

//Id(x => x.Id, map =>
//{
//    map.Column("ID");
//    map.Generator(Generators.Sequence, g => g.Params(new { sequence = "TABLE_SEQ" }));
//});
        public string IdSequenceMap(Column column, string sequenceName, ITextFormatter formatter)
        {
            var builder = new StringBuilder();
            switch (_language)
            {
                case Language.CSharp:
                    builder.AppendFormat("Id(x => x.{0}, map => ", formatter.FormatText(column.Name));
                    builder.AppendLine();
                    builder.AppendLine("\t\t\t\t{");
                    builder.AppendLine("\t\t\t\t\tmap.Column(\"" + column.Name + "\");");
                    builder.AppendLine("\t\t\t\t\tmap.Generator(Generators.Sequence, g => g.Params(new { sequence = \"" + sequenceName + "\" }));");
                    builder.Append("\t\t\t\t});");
                    break;
                case Language.VB:
                    builder.AppendFormat("Id(Function(x) x.{0}, Sub(map)", formatter.FormatText(column.Name));
                    builder.AppendLine();
                    builder.AppendLine("\t\t\t\t\tmap.Column(\"" + column.Name + "\")");
                    builder.AppendLine("\t\t\t\t\tmap.Generator(Generators.Sequence, Function(g) g.Params(New { sequence = \"" + sequenceName + "\" }))");
                    builder.Append("\t\t\t\tEnd Sub)");
                    break;
            }

            return builder.ToString();
        }

        public string IdMap (Column column, ITextFormatter formatter)
        {
            var mapList = new List<string>();
            var propertyName = formatter.FormatText(column.Name);

            if (column.Name.ToLower() != propertyName.ToLower())
            {
                mapList.Add("map.Column(\"" + column.Name + "\")");
            }
            mapList.Add(column.IsIdentity ? "map.Generator(Generators.Identity)" : "map.Generator(Generators.Assigned)");

            // Outer property definition
            return FormatCode("Id", propertyName, mapList);
        }

        public string CompositeIdMap(IList<Column> columns, ITextFormatter formatter)
        {
            var builder = new StringBuilder();


            switch (_language)
            {
                case Language.CSharp:
                    builder.AppendLine("ComposedId(compId =>");
                    builder.AppendLine("\t\t\t\t{");
                    foreach (var column in columns)
                    {
                        builder.AppendLine("\t\t\t\t\tcompId.Property(x => x." + formatter.FormatText(column.Name) + ", m => m.Column(\"" + column.Name + "\"));");
                    }
                    builder.Append("\t\t\t\t});");
                    break;
                case Language.VB:
                    builder.AppendLine("ComposedId(Sub(compId)");
                    foreach (var column in columns)
                    {
                        builder.AppendLine("\t\t\t\t\tcompId.Property(Function(x) x." + formatter.FormatText(column.Name) + ", Sub(m) m.Column(\"" + column.Name + "\"))");
                    }
                    builder.AppendLine("\t\t\t\tEnd Sub)");
                    break;
            }

            return builder.ToString();
        }
        
            
//Property(x => x.Name, map =>
//                {
//                    map.Column("NAME");
//                    map.NotNullable(true);
//                    map.Length(200);
//                    map.Unique(true);
//                });
        public string Map(Column column, ITextFormatter formatter, bool includeLengthAndScale = true)
        {
            var propertyName = formatter.FormatText(column.Name);
            var mapList = new List<string>();

            // Column
            if (column.Name.ToLower() != propertyName.ToLower())
            {
                mapList.Add("map.Column(\"" + column.Name + "\")");
            }
            // Not Null
            if (!column.IsNullable)
            {
                mapList.Add("map.NotNullable(true)");
            }
            // Unique
            if (column.IsUnique)
            {
                mapList.Add("map.Unique(true)");
            }
            // Length
            if (column.DataLength.GetValueOrDefault() > 0 & includeLengthAndScale)
            {
                mapList.Add("map.Length(" + column.DataLength + ")");
            }
            else
            {
                // Precision
                if (column.DataPrecision.GetValueOrDefault(0) > 0 & includeLengthAndScale)
                {
                    mapList.Add("map.Precision(" + column.DataPrecision + ")");
                }
                // Scale
                if (column.DataScale.GetValueOrDefault(0) > 0 & includeLengthAndScale)
                {
                    mapList.Add("map.Scale(" + column.DataScale + ")");
                }
            }
            
            // Outer property definition
            return FormatCode("Property", propertyName, mapList);
        }

        private string FormatCode(string byCodeProperty, string propertyName, List<string> mapList)
        {
            // Outer property definition
            var outerStrBuilder = new StringBuilder();

            switch (_language)
            {
                case Language.CSharp:
                    switch (mapList.Count)
                    {
                        case 0:
                            outerStrBuilder.AppendFormat("{0}(x => x.{1})", byCodeProperty, propertyName);
                            break;
                        case 1:
                            outerStrBuilder.AppendFormat("{0}(x => x.{1}, map => {2});", byCodeProperty, propertyName, mapList.First());
                            break;
                        case 2:
                            outerStrBuilder.AppendFormat("{0}(x => x.{1}, map => {{ {2}; }});", byCodeProperty, propertyName, mapList.Aggregate((c, s) => string.Format("{0}; {1}", c, s)));
                            break;
                        default:
                            outerStrBuilder.AppendFormat("{0}(x => x.{1}, map => \r\n\t\t\t{{\r\n\t\t\t\t{2};\r\n\t\t\t}});", byCodeProperty,propertyName, mapList.Aggregate((c, s) => string.Format("{0};\r\n\t\t\t\t{1}", c, s)));
                            break;
                    }
                    break;
                case Language.VB:
                    if (byCodeProperty.ToLower() == "property") byCodeProperty = "[Property]";

                    switch (mapList.Count)
                    {
                        case 0:
                            outerStrBuilder.AppendFormat("{0}(Function(x) x.{1})", byCodeProperty, propertyName);
                            break;
                        case 1:
                            outerStrBuilder.AppendFormat("{0}(Function(x) x.{1}, Sub(map) {2})", byCodeProperty, propertyName, mapList.First());
                            break;
                        default:
                            outerStrBuilder.AppendFormat("{0}(Function(x) x.{1}, Sub(map)\r\n\t\t\t\t\t\t{2}\r\n\t\t\t\t\tEnd Sub)", byCodeProperty, propertyName, mapList.Aggregate((c, s) => string.Format("{0}\r\n\t\t\t\t\t\t{1}", c, s)));
                            break;
                    }
                    break;
            }

            return outerStrBuilder.ToString();
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
            if (fk.Columns.Count() == 1)
            {
                builder.Append("\t\t\tManyToOne<" + formatter.FormatSingular(fk.References) + ">(x => x." +
                               formatter.FormatSingular(fk.UniquePropertyName) + ", map => { map.Column(\"" +
                               fk.Columns.First().Name + "\"); });");
            }
            else
            {
                // Composite Foreign Key
                // eg ManyToOne<TesteHeader>(x => x.TesteHeader, map => map.Columns(new Action<IColumnMapper>[] { x => x.Name("HeadIdOne"), x => x.Name("HeadIdTwo") }));

                builder.AppendFormat("\t\t\tManyToOne<{0}>(x => x.{1}, map => map.Columns(new Action<IColumnMapper>[] {{ ",
                                     formatter.FormatSingular(fk.References),
                                     formatter.FormatSingular(fk.UniquePropertyName));

                var lastColumn = fk.Columns.Last();
                foreach (var column in fk.Columns)
                {
                    builder.AppendFormat("x.Name(\"{0}\")",column.Name);

                    var isLastColumn = lastColumn == column;
                    if (!isLastColumn)
                    {
                        builder.Append(", ");
                    }
                }

                builder.Append(" }))");
            }
            return builder.ToString();
        }
    }
}

