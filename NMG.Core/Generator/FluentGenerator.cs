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
        private readonly ApplicationPreferences appPrefs;

        public FluentGenerator(ApplicationPreferences appPrefs, Table table) : base(appPrefs.FolderPath, "Mapping", appPrefs.TableName, appPrefs.NameSpaceMap, appPrefs.AssemblyName, appPrefs.Sequence, table, appPrefs)
        {
            this.appPrefs = appPrefs;
            language = this.appPrefs.Language;
        }

        public override void Generate(bool writeToFile = true)
        {
            var pascalCaseTextFormatter = new PascalCaseTextFormatter { PrefixRemovalList = appPrefs.FieldPrefixRemovalList };
            var className = string.Format("{0}{1}{2}", appPrefs.ClassNamePrefix, pascalCaseTextFormatter.FormatSingular(Table.Name), "Map");
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
            
            newType.IsPartial = appPrefs.GeneratePartialClasses;
            var pascalCaseTextFormatter = new PascalCaseTextFormatter { PrefixRemovalList = appPrefs.FieldPrefixRemovalList };
            newType.BaseTypes.Add(string.Format("ClassMap<{0}{1}>", appPrefs.ClassNamePrefix, pascalCaseTextFormatter.FormatSingular(Table.Name)));

            var constructor = new CodeConstructor {Attributes = MemberAttributes.Public};
            constructor.Statements.Add(new CodeSnippetStatement(TABS + "Table(\"" + Table.Name + "\");"));
            if (appPrefs.UseLazy)
                constructor.Statements.Add(new CodeSnippetStatement(TABS + "LazyLoad();"));

            if(UsesSequence)
            {
				constructor.Statements.Add(new CodeSnippetStatement(String.Format(TABS + "Id(x => x.{0}).Column(x => x.{1}).GeneratedBy.Sequence(\"{2}\")",
                    Formatter.FormatText(Table.PrimaryKey.Columns[0].Name), Table.PrimaryKey.Columns[0].Name, appPrefs.Sequence)));
            }
            else if (Table.PrimaryKey !=null && Table.PrimaryKey.Type == PrimaryKeyType.PrimaryKey)
            {
                constructor.Statements.Add(GetIdMapCodeSnippetStatement(this.appPrefs, Table.PrimaryKey.Columns[0].Name, Table.PrimaryKey.Columns[0].DataType, Formatter));
            }
            else if (Table.PrimaryKey != null)
            {
                constructor.Statements.Add(GetIdMapCodeSnippetStatement(Table.PrimaryKey, Formatter));
            }

            foreach (var fk in Table.ForeignKeys.Where(fk => fk.Columns.First().IsForeignKey && appPrefs.IncludeForeignKeys))
            {
                var propertyName = appPrefs.NameFkAsForeignTable ? fk.UniquePropertyName : fk.Columns.First().Name;
                var fieldName = Formatter.FormatSingular(propertyName);
                constructor.Statements.Add(new CodeSnippetStatement(string.Format(TABS + "References(x => x.{0}).Column(\"{1}\");", fieldName, fk.Columns.First().Name)));
            }

            foreach (var column in Table.Columns.Where(x => !x.IsPrimaryKey && (!x.IsForeignKey || !appPrefs.IncludeForeignKeys)))
            {
                var columnMapping = new DBColumnMapper().Map(column, Formatter, appPrefs.IncludeLengthAndScale);
                constructor.Statements.Add(new CodeSnippetStatement(TABS + columnMapping));
            }

            if (appPrefs.IncludeHasMany)
                Table.HasManyRelationships.ToList().ForEach(x =>
                        constructor.Statements.Add(new OneToMany(Formatter).Create(x))
                    );

            newType.Members.Add(constructor);
            return compileUnit;
        }

        protected override string AddStandardHeader(string entireContent)
        {
            entireContent = "using " + appPrefs.NameSpace + "; " + entireContent;
            entireContent = "using FluentNHibernate.Mapping;" + Environment.NewLine + entireContent;
            return base.AddStandardHeader(entireContent);
        }

        private static CodeSnippetStatement GetIdMapCodeSnippetStatement(ApplicationPreferences appPrefs, string pkColumnName, string pkColumnType, ITextFormatter formatter)
        {
            var dataTypeMapper = new DataTypeMapper();
            bool isPkTypeIntegral = (dataTypeMapper.MapFromDBType(appPrefs.ServerType, pkColumnType, null, null, null)).IsTypeIntegral();
            string idGeneratorType = (isPkTypeIntegral ? "GeneratedBy.Identity()" : "GeneratedBy.Assigned()");
            return new CodeSnippetStatement(string.Format(TABS + "Id(x => x.{0}).{1}.Column(\"{2}\");",
                                                       formatter.FormatText(pkColumnName),
                                                       idGeneratorType,
                                                       pkColumnName));
        }

        private static CodeSnippetStatement GetIdMapCodeSnippetStatement(PrimaryKey primaryKey, ITextFormatter formatter)
        {
            var keyPropertyBuilder = new StringBuilder(primaryKey.Columns.Count);
            bool first = true;
            foreach (Column pkColumn in primaryKey.Columns)
            {
                var tmp = String.Format(".KeyProperty(x => x.{0}, \"{1}\")", formatter.FormatText(pkColumn.Name), pkColumn.Name);
                keyPropertyBuilder.Append(first ? tmp : "\n" + TABS + "             " + tmp);
                first = false;
            }

            return new CodeSnippetStatement(TABS + string.Format("CompositeId(){0};", keyPropertyBuilder));
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

        public CodeSnippetStatement Create(HasMany hasMany)
        {
        	var hasManySnippet = string.Format("HasMany(x => x.{0})", Formatter.FormatPlural(hasMany.Reference));
        	var keySnippet = hasMany.AllReferenceColumns.Count == 1 ? 
				string.Format(".KeyColumn(\"{0}\")", hasMany.ReferenceColumn) : 
				string.Format(".KeyColumns({0})", hasMany.AllReferenceColumns.Aggregate("new string[] { ", (a, b) => a + "\"" + b + "\", ", c => c.Substring(0, c.Length - 2) + " }"));

			return new CodeSnippetStatement(string.Format(AbstractGenerator.TABS + "{0}{1};", hasManySnippet, keySnippet));
		}
	}

    public static class StringExtensions
    {
        public static string Capitalize(this string inputString)
        {
            var textInfo = new CultureInfo("en-US", false).TextInfo;
            return textInfo.ToTitleCase(inputString);
        }
    }
}