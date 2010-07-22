using System;
using System.CodeDom;
using NMG.Core.Domain;
using NMG.Core.Fluent;
using NMG.Core.Util;

namespace NMG.Core.Generator
{
    public class FluentGenerator : AbstractCodeGenerator
    {
        private readonly ApplicationPreferences applicationPreferences;
        private const string TABS = "\t\t\t";

        public FluentGenerator(ApplicationPreferences applicationPreferences, ColumnDetails columnDetails)
            : base(
                applicationPreferences.FolderPath, applicationPreferences.TableName,
                applicationPreferences.NameSpace,
                applicationPreferences.AssemblyName, applicationPreferences.Sequence, columnDetails)
        {
            this.applicationPreferences = applicationPreferences;
            language = this.applicationPreferences.Language;
        }

        public override void Generate()
        {
            string className = tableName.GetFormattedText() + "Map";
            var compileUnit = GetCompleteCompileUnit(className);
            var generateCode = GenerateCode(compileUnit, className);
            WriteToFile(generateCode, className);
        }

        public CodeCompileUnit GetCompleteCompileUnit(string className)
        {
            var codeGenerationHelper = new CodeGenerationHelper();
            var compileUnit = codeGenerationHelper.GetCodeCompileUnit(nameSpace, className);

            var newType = compileUnit.Namespaces[0].Types[0];

            newType.BaseTypes.Add("ClassMap<" + tableName.GetFormattedText() + ">");

            var constructor = new CodeConstructor { Attributes = MemberAttributes.Public };
            newType.Members.Add(constructor);
            constructor.Statements.Add(new CodeSnippetStatement(TABS + "Table(\"" + tableName + "\");"));
            constructor.Statements.Add(new CodeSnippetStatement(TABS + "LazyLoad();"));

            foreach (var columnDetail in columnDetails)
            {
                if (columnDetail.IsPrimaryKey)
                {
                    constructor.Statements.Add(GetIdMapCodeSnippetStatement(columnDetail.ColumnName));
                    var metadataReader = MetadataFactory.GetReader(applicationPreferences.ServerType, applicationPreferences.ConnectionString);
                    var foreignKeyTables = metadataReader.GetForeignKeyTables(columnDetail.ColumnName);
                    foreach (var foreignKeyTable in foreignKeyTables)
                    {
                        constructor.Statements.Add(new CodeSnippetStatement(string.Format(TABS + "References(x => x.{0}).Column(\"{1}\");", foreignKeyTable, foreignKeyTable)));
                    } 
                    continue;
                }
                if (columnDetail.IsForeignKey)
                {
                    constructor.Statements.Add(new CodeSnippetStatement(string.Format(TABS + "References(x => x.{0}).Column(\"{1}\");", columnDetail.ColumnName, columnDetail.ColumnName)));
                }
                else
                {
                    var columnMapping = new DBColumnMapper().Map(columnDetail);
                    constructor.Statements.Add(new CodeSnippetStatement(TABS + columnMapping));
                }
            }
            return compileUnit;
        }

        protected override string AddStandardHeader(string entireContent)
        {
            entireContent = "using FluentNHibernate.Mapping;" + entireContent;
            return base.AddStandardHeader(entireContent);
        }

        private static CodeSnippetStatement GetIdMapCodeSnippetStatement(string pkColumnName)
        {
            return new CodeSnippetStatement(string.Format("\t\t\tId(x => x.{0}).GeneratedBy.Identity();", pkColumnName));
        }
    }
}