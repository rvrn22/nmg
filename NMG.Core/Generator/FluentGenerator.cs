using System;
using System.CodeDom;
using System.Globalization;
using System.Linq;
using System.Text;
using NMG.Core.Domain;
using NMG.Core.Fluent;
using NMG.Core.TextFormatter;

namespace NMG.Core.Generator
{
    public class FluentGenerator : AbstractCodeGenerator
    {
        private const string TABS = "\t\t\t";
        private readonly ApplicationPreferences applicationPreferences;

        public FluentGenerator(ApplicationPreferences applicationPreferences, Table table)
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
            CodeCompileUnit compileUnit = codeGenerationHelper.GetCodeCompileUnit(nameSpace, className);

            CodeTypeDeclaration newType = compileUnit.Namespaces[0].Types[0];

            newType.BaseTypes.Add("ClassMap<" + Formatter.FormatSingular(Table.Name) + ">");

            var constructor = new CodeConstructor {Attributes = MemberAttributes.Public};
            //newType.Members.Add(constructor);
            constructor.Statements.Add(
                new CodeSnippetStatement(TABS + "Table(\"" + Table.Name + "\");"));
            //constructor.Statements.Add(new CodeSnippetStatement(TABS + "ReadOnly();"));
            constructor.Statements.Add(new CodeSnippetStatement(TABS + "LazyLoad();"));

            // Determine primary key type
            if(UsesSequence)
            {
                constructor.Statements.Add(new CodeSnippetStatement(String.Format("\t\t\tId(x => x.{0}).Column(x => x.{1}).GeneratedBy.Sequence(\"{2}\")",
                    Formatter.FormatText(Table.PrimaryKey.Columns[0].Name), Table.PrimaryKey.Columns[0].Name, applicationPreferences.Sequence)));
            }
            // refactor to set primarykeytype enum and use that instead to check
            else if (Table.PrimaryKey.Type == PrimaryKeyType.PrimaryKey)
                constructor.Statements.Add(GetIdMapCodeSnippetStatement(Table.PrimaryKey.Columns[0].Name,
                                                                        Table.PrimaryKey.Columns[0].DataType,
                                                                        Table.PrimaryKey.Type,
                                                                        Formatter));
            else
            {
                constructor.Statements.Add(GetIdMapCodeSnippetStatement(Table.PrimaryKey, Formatter));
            }

            foreach (ForeignKey fk in Table.ForeignKeys)
            {
                constructor.Statements.Add(
                    new CodeSnippetStatement(string.Format(TABS + "References(x => x.{0}).Column(\"{1}\");",
                                                           Formatter.FormatSingular(fk.References), fk.Name)));
            }

            foreach (Column column in Table.Columns.Where(x => x.IsPrimaryKey != true && x.IsForeignKey != true))
            {
                string columnMapping = new DBColumnMapper().Map(column, Formatter);
                constructor.Statements.Add(new CodeSnippetStatement(TABS + columnMapping));
            }

            foreach (HasMany hasMany in Table.HasManyRelationships)
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

        private static CodeSnippetStatement GetIdMapCodeSnippetStatement(string pkColumnName, string pkColumnType,
                                                                         PrimaryKeyType keyType, ITextFormatter Formatter)
        {
            var dataTypeMapper = new DataTypeMapper();
            bool isPkTypeIntegral = (dataTypeMapper.MapFromDBType(pkColumnType, null, null, null)).IsTypeIntegral();
            string idGeneratorType = (isPkTypeIntegral ? "GeneratedBy.Identity()" : "GeneratedBy.Assigned()");
            return
                new CodeSnippetStatement(string.Format("\t\t\tId(x => x.{0}).{1}.Column(\"{2}\");",
                                                       Formatter.FormatText(pkColumnName),
                                                       idGeneratorType,
                                                       pkColumnName));
        }

        // Generate composite key 
        //IList<Column> pkColumns, PrimaryKeyType keyType
        private static CodeSnippetStatement GetIdMapCodeSnippetStatement(PrimaryKey primaryKey, ITextFormatter Formatter)
        {
            var dataTypeMapper = new DataTypeMapper();
            //bool isPkTypeIntegral = (dataTypeMapper.MapFromDBType(pkColumnType, null, null, null)).IsTypeIntegral();
            //var idGeneratorType = (isPkTypeIntegral ? "GeneratedBy.Identity()" : "GeneratedBy.Assigned()");
            var keyPropertyBuilder = new StringBuilder(primaryKey.Columns.Count);
            foreach (Column pkColumn in primaryKey.Columns)
            {
                keyPropertyBuilder.Append(String.Format(".KeyProperty(x => x.{0})", Formatter.FormatText(pkColumn.Name)));
            }

            return
                new CodeSnippetStatement(TABS + string.Format("CompositeId(){0};", keyPropertyBuilder));
        }

        // Generate id sequence
        private static CodeSnippetStatement GetIdMapCodeSnippetStatementSequenceId(PrimaryKeyType primaryKey)
        {
            return new CodeSnippetStatement(TABS);
        }
    }

    public static class DataTypeExtensions
    {
        public static bool IsTypeIntegral(this Type typeToCheck)
        {
            return
                typeToCheck == typeof (int) ||
                typeToCheck == typeof (long) ||
                typeToCheck == typeof (uint) ||
                typeToCheck == typeof (ulong);
        }
    }

    public class OneToMany
    {
        public OneToMany(ITextFormatter formatter)
        {
            Formatter = formatter;
        }

        private ITextFormatter Formatter { get; set; }

        public CodeSnippetStatement Create(string references)
        {
            return
                new CodeSnippetStatement(string.Format("\t\t\t" + "HasMany(x => x.{0});",
                                                       Formatter.FormatPlural(references)));
        }
    }

    public static class StringExtensions
    {
        public static string Capitalize(this string inputString)
        {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            return textInfo.ToTitleCase(inputString);
        }
    }
}