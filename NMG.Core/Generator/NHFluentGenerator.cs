using System;
using System.CodeDom;
using System.Text;
using NMG.Core.Domain;

namespace NMG.Core.Generator
{
    public class NHFluentGenerator : AbstractCodeGenerator
    {
        private readonly ApplicationPreferences appPrefs;

        public NHFluentGenerator(ApplicationPreferences applicationPreferences, Table table) : base(applicationPreferences.FolderPath, "Mapping", applicationPreferences.TableName, applicationPreferences.NameSpace, applicationPreferences.AssemblyName, applicationPreferences.Sequence, table, applicationPreferences)
        {
            appPrefs = applicationPreferences;
        }

        public override void Generate()
        {
            var className = string.Format("{0}{1}{2}", appPrefs.ClassNamePrefix, Formatter.FormatSingular(Table.Name), "Map");
            var compileUnit = GetCompleteCompileUnit(className);
            var generateCode = GenerateCode(compileUnit, className);
            WriteToFile(generateCode, className);
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
            constructor.Statements.Add(GetIdMapCodeSnippetStatement());

            foreach (var columnDetail in Table.Columns)
            {
                if (columnDetail.IsPrimaryKey)
                {
                    continue;
                }

                if (columnDetail.IsForeignKey)
                {
                    var manyToOneMapping = TABS + "ManyToOne(x => x." + columnDetail.Name + ", map => {map.Column(\"" + columnDetail.Name + "\"); map.NotNullable(" +
                                           (!columnDetail.IsNullable).ToString().ToLower() + "); map.Cascade(Cascade.None); });";
                    constructor.Statements.Add(new CodeSnippetStatement(manyToOneMapping));
                }
                else
                {
                    var columnMapping = MapNhStyle(columnDetail);
                    constructor.Statements.Add(new CodeSnippetStatement(TABS + columnMapping));
                }
            }
            return compileUnit;
        }

        private CodeSnippetStatement GetIdMapCodeSnippetStatement()
        {
            var idGen = "\t\t\t" + "Id(x => x.Id, map => { map.Column(\"" + Table.PrimaryKey + "\"); map.Generator(Generators.HighLow, map => map.Params(new {table = \"T_NEXT_ID\", column = \"next_id\", max_lo = \"0\", where = \"type = 'TN'\" }));});";

            return new CodeSnippetStatement(idGen);
        }

        protected override string AddStandardHeader(string entireContent)
        {
            entireContent = "using NHibernate.Mapping.ByCode;" + Environment.NewLine + entireContent;
            entireContent = string.Format("using {0};", nameSpace) + Environment.NewLine + entireContent;
            return base.AddStandardHeader(entireContent);
        }

        private string MapNhStyle(Column column)
        {
            var mappedStrBuilder = new StringBuilder("Property(x => x." + column.Name + ", map => { map.Column(\"" + column.Name + "\");");
            if (column.DataLength > 0)
            {
                mappedStrBuilder.Append(" map.Length(" + column.DataLength + ");");
            }
            if (!column.IsNullable)
            {
                mappedStrBuilder.Append(" map.NotNullable(false);");
            }
            mappedStrBuilder.Append(" });");
            return mappedStrBuilder.ToString();
        }
    }
}