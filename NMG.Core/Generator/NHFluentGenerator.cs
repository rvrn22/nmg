using System;
using System.CodeDom;
using System.Text;
using NMG.Core.Domain;
using NMG.Core.TextFormatter;

namespace NMG.Core.Generator
{
    public class NHFluentGenerator : AbstractCodeGenerator
    {
        private readonly ApplicationPreferences appPrefs;

        public NHFluentGenerator(ApplicationPreferences applicationPreferences, Table table) : base(applicationPreferences.FolderPath, "Mapping", applicationPreferences.TableName, applicationPreferences.NameSpaceMap, applicationPreferences.AssemblyName, applicationPreferences.Sequence, table, applicationPreferences)
        {
            appPrefs = applicationPreferences;
        }

        public override void Generate(bool writeToFile = true)
        {
            var className = string.Format("{0}{1}{2}", appPrefs.ClassNamePrefix, Formatter.FormatSingular(Table.Name), "Map");
            var compileUnit = GetCompleteCompileUnit(className);
            var generateCode = GenerateCode(compileUnit, className);

            if (writeToFile)
            {
                WriteToFile(generateCode, className);
            }
        }

        public CodeCompileUnit GetCompleteCompileUnit(string className)
        {
            var codeGenerationHelper = new CodeGenerationHelper();
            var compileUnit = codeGenerationHelper.GetCodeCompileUnit(nameSpace, className);

            var newType = compileUnit.Namespaces[0].Types[0];
            newType.BaseTypes.Add(string.Format("ClassMapping<{0}{1}>", appPrefs.ClassNamePrefix, Formatter.FormatSingular(Table.Name)));

            var constructor = new CodeConstructor {Attributes = MemberAttributes.Public};
            newType.Members.Add(constructor);
            constructor.Statements.Add(new CodeSnippetStatement(TABS + "Table(\"" + tableName + "\");"));

            if(UsesSequence)
            {
                constructor.Statements.Add(new CodeSnippetStatement(String.Format(TABS + "Id(x => x.{0}).Column(x => x.{1}).GeneratedBy.Sequence(\"{2}\")",
                    Formatter.FormatText(Table.PrimaryKey.Columns[0].Name), Table.PrimaryKey.Columns[0].Name, appPrefs.Sequence)));
            }
            else if (Table.PrimaryKey != null && Table.PrimaryKey.Type == PrimaryKeyType.PrimaryKey)
            {
                constructor.Statements.Add(GetIdMapCodeSnippetStatement(appPrefs, Table.PrimaryKey.Columns[0].Name, Table.PrimaryKey.Columns[0].DataType, Formatter));
            }

            foreach (var columnDetail in Table.Columns)
            {
                if (columnDetail.IsPrimaryKey)
                {
                    continue;
                }

                if (columnDetail.IsForeignKey)
                {
                    var manyToOneMapping = TABS + "ManyToOne(x => x." + Formatter.FormatText(columnDetail.Name) + ", map => {map.Column(\"" + columnDetail.Name + "\"); map.NotNullable(" +
                                           (!columnDetail.IsNullable).ToString().ToLower() + "); map.Cascade(Cascade.None); });";
                    constructor.Statements.Add(new CodeSnippetStatement(manyToOneMapping));
                }
                else
                {
                    var columnMapping = MapNhStyle(columnDetail, appPrefs.IncludeLengthAndScale);
                    constructor.Statements.Add(new CodeSnippetStatement(TABS + columnMapping));
                }
            }
            return compileUnit;
        }

        protected override string AddStandardHeader(string entireContent)
        {
            entireContent = "using NHibernate.Mapping.ByCode;" + Environment.NewLine + entireContent;
            entireContent = "using NHibernate.Mapping.ByCode.Conformist;" + Environment.NewLine + entireContent;
            if (!string.IsNullOrWhiteSpace(nameSpace))
            {
                entireContent = string.Format("using {0};", nameSpace) + Environment.NewLine + entireContent;
            }
            return base.AddStandardHeader(entireContent);
        }

        private static CodeSnippetStatement GetIdMapCodeSnippetStatement(ApplicationPreferences appPrefs, string pkColumnName, string pkColumnType, ITextFormatter formatter)
        {
            var dataTypeMapper = new DataTypeMapper();
            var isPkTypeIntegral = (dataTypeMapper.MapFromDBType(appPrefs.ServerType, pkColumnType, null, null, null)).IsTypeIntegral();
            var idGeneratorType = isPkTypeIntegral ? "Generators.Identity" : "Generators.Assigned";
            return new CodeSnippetStatement(string.Format(TABS + "Id(x => x.{0}, map => {{ map.Column(\"{2}\"); map.Generator({1}); }});", formatter.FormatText(pkColumnName), idGeneratorType, pkColumnName));
        }

        private string MapNhStyle(Column column, bool includeLengthAndScale = true)
        {
            var mappedStrBuilder = new StringBuilder("Property(x => x." + Formatter.FormatText(column.Name) + ", map => { map.Column(\"" + column.Name + "\");");


            if (column.DataLength.GetValueOrDefault() > 0 & includeLengthAndScale)
            {
                mappedStrBuilder.Append(" map.Length(" + column.DataLength + ");");
            }
            else
            {
                if (column.DataPrecision.GetValueOrDefault(0) > 0 & includeLengthAndScale)
                {
                    mappedStrBuilder.Append(" map.Precision(" + column.DataPrecision + ");");
                }

                if (column.DataScale.GetValueOrDefault(0) > 0 & includeLengthAndScale)
                {
                    mappedStrBuilder.Append(" map.Scale(" + column.DataScale + ");");
                }
            }

            if (!column.IsNullable)
            {
                mappedStrBuilder.Append(" map.NotNullable(false);");
            }
            if (column.IsUnique)
            {
                mappedStrBuilder.Append(" map.Unique(true);");
            }
            mappedStrBuilder.Append(" });");
            return mappedStrBuilder.ToString();
        }
    }
}