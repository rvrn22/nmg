using System;
using System.CodeDom;
using System.Linq;
using System.Text;
using NMG.Core.Domain;
using NMG.Core.Fluent;
using NMG.Core.TextFormatter;

namespace NMG.Core.Generator
{
    public class NHFluentGenerator : AbstractCodeGenerator
    {
        private const string TABS = "\t\t\t";
        private readonly ApplicationPreferences applicationPreferences;

        public NHFluentGenerator(ApplicationPreferences applicationPreferences, Table table)
            : base(
                applicationPreferences.FolderPath, applicationPreferences.TableName,
                applicationPreferences.NameSpace,
                applicationPreferences.AssemblyName, applicationPreferences.Sequence, table,
                applicationPreferences)
        {
            this.applicationPreferences = applicationPreferences;
            language = this.applicationPreferences.Language;
        }

        public override void Generate()
        {
            string className = Formatter.FormatSingular(Table.Name) + "Map";
            CodeCompileUnit compileUnit = GetCompleteCompileUnit(className);
            string generateCode = GenerateCode(compileUnit, className);
            WriteToFile(generateCode, className);
        }

        public CodeCompileUnit GetCompleteCompileUnit(string className)
        {
            var codeGenerationHelper = new CodeGenerationHelper();
            var compileUnit = codeGenerationHelper.GetCodeCompileUnit(nameSpace, className);

            var newType = compileUnit.Namespaces[0].Types[0];
            
            newType.IsPartial = applicationPreferences.GeneratePartialClasses;

            newType.BaseTypes.Add("ClassMap<" + Formatter.FormatSingular(Table.Name) + ">");

            var constructor = new CodeConstructor {Attributes = MemberAttributes.Public};
            constructor.Statements.Add(new CodeSnippetStatement(TABS + "Table(\"" + Table.Name + "\");"));
            constructor.Statements.Add(new CodeSnippetStatement(TABS + "LazyLoad();"));

            if(UsesSequence)
            {
                constructor.Statements.Add(new CodeSnippetStatement(String.Format("\t\t\tId(x => x.{0}).Column(x => x.{1}).GeneratedBy.Sequence(\"{2}\")",
                    Formatter.FormatText(Table.PrimaryKey.Columns[0].Name), Table.PrimaryKey.Columns[0].Name, applicationPreferences.Sequence)));
            }
            else if (Table.PrimaryKey.Type == PrimaryKeyType.PrimaryKey)
            {
                constructor.Statements.Add(GetIdMapCodeSnippetStatement(Table.PrimaryKey.Columns[0].Name, Table.PrimaryKey.Columns[0].DataType, Formatter));
            }
            else
            {
                constructor.Statements.Add(GetIdMapCodeSnippetStatement(Table.PrimaryKey, Formatter));
            }

            foreach (var fk in Table.ForeignKeys.Where(fk => !string.IsNullOrEmpty(fk.References)))
            {
                constructor.Statements.Add(new CodeSnippetStatement(string.Format(TABS + "References(x => x.{0}).Column(\"{1}\");", Formatter.FormatSingular(fk.References), fk.Name)));
            }

            foreach (var column in Table.Columns.Where(x => x.IsPrimaryKey != true && x.IsForeignKey != true))
            {
                var columnMapping = new DBColumnMapper().Map(column, Formatter);
                constructor.Statements.Add(new CodeSnippetStatement(TABS + columnMapping));
            }

            foreach (var hasMany in Table.HasManyRelationships)
            {
                constructor.Statements.Add(new OneToMany(Formatter).Create(hasMany.Reference));
            }

            newType.Members.Add(constructor);
            return compileUnit;
        }

        protected override string AddStandardHeader(string entireContent)
        {
            entireContent = "using FluentNHibernate.Mapping;" + entireContent;
            return base.AddStandardHeader(entireContent);
        }

        private static CodeSnippetStatement GetIdMapCodeSnippetStatement(string pkColumnName, string pkColumnType, ITextFormatter formatter)
        {
            var dataTypeMapper = new DataTypeMapper();
            bool isPkTypeIntegral = (dataTypeMapper.MapFromDBType(pkColumnType, null, null, null)).IsTypeIntegral();
            string idGeneratorType = (isPkTypeIntegral ? "GeneratedBy.Identity()" : "GeneratedBy.Assigned()");
            return new CodeSnippetStatement(string.Format("\t\t\tId(x => x.{0}).{1}.Column(\"{2}\");", formatter.FormatText(pkColumnName), idGeneratorType, pkColumnName));
        }

        private static CodeSnippetStatement GetIdMapCodeSnippetStatement(PrimaryKey primaryKey, ITextFormatter formatter)
        {
            var keyPropertyBuilder = new StringBuilder(primaryKey.Columns.Count);
            foreach (var pkColumn in primaryKey.Columns)
            {
                keyPropertyBuilder.Append(String.Format(".KeyProperty(x => x.{0})", formatter.FormatText(pkColumn.Name)));
            }

            return new CodeSnippetStatement(TABS + string.Format("CompositeId(){0};", keyPropertyBuilder));
        }
    }


}