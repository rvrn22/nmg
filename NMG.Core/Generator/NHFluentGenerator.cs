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
        private readonly ApplicationPreferences appPrefs;

        public NHFluentGenerator(ApplicationPreferences appPrefs, Table table) : base(appPrefs.FolderPath, "Mapping", appPrefs.TableName, appPrefs.NameSpace, appPrefs.AssemblyName, appPrefs.Sequence, table, appPrefs)
        {
            this.appPrefs = appPrefs;
            language = this.appPrefs.Language;
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
            
            newType.IsPartial = appPrefs.GeneratePartialClasses;

			newType.BaseTypes.Add("ClassMap<" + appPrefs.ClassNamePrefix + Formatter.FormatSingular(Table.Name) + ">");

            var constructor = new CodeConstructor {Attributes = MemberAttributes.Public};
            constructor.Statements.Add(new CodeSnippetStatement(TABS + "Table(\"" + Table.Name + "\");"));
            constructor.Statements.Add(new CodeSnippetStatement(TABS + "LazyLoad();"));

            if(UsesSequence)
            {
                constructor.Statements.Add(new CodeSnippetStatement(String.Format(TABS + "Id(x => x.{0}).Column(x => x.{1}).GeneratedBy.Sequence(\"{2}\")",
                    Formatter.FormatText(Table.PrimaryKey.Columns[0].Name), Table.PrimaryKey.Columns[0].Name, appPrefs.Sequence)));
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
            	var referencesSnippet = string.Format("References(x => x.{0})", Formatter.FormatSingular(fk.UniquePropertyName));
				var columnsSnippet = fk.AllColumnsNamesForTheSameConstraint.Length == 1 ?
					string.Format(".Column(\"{0}\");", fk.Name) :
					string.Format(".Columns({0});", fk.AllColumnsNamesForTheSameConstraint.Aggregate("new string[] { ", (a,b) => a+"\""+b+"\", ", c=>c.Substring(0, c.Length - 2) + " }" ));
				
				constructor.Statements.Add(new CodeSnippetStatement(string.Format(TABS + "{0}{1}", referencesSnippet, columnsSnippet)));
			}

            foreach (var column in Table.Columns.Where(x => x.IsPrimaryKey != true && x.IsForeignKey != true))
            {
                var columnMapping = new DBColumnMapper().Map(column, Formatter);
                constructor.Statements.Add(new CodeSnippetStatement(TABS + columnMapping));
            }

			Table.HasManyRelationships.ToList().ForEach(x =>
				constructor.Statements.Add(new OneToMany(Formatter).Create(x))
				);

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
            return new CodeSnippetStatement(string.Format(TABS + "Id(x => x.{0}).{1}.Column(\"{2}\");", formatter.FormatText(pkColumnName), idGeneratorType, pkColumnName));
        }

        private static CodeSnippetStatement GetIdMapCodeSnippetStatement(PrimaryKey primaryKey, ITextFormatter formatter)
        {
            var keyPropertyBuilder = new StringBuilder(primaryKey.Columns.Count);
            foreach (var pkColumn in primaryKey.Columns)
            {
				keyPropertyBuilder.Append(String.Format(".KeyProperty(x => x.{0}, \"{1}\")", formatter.FormatText(pkColumn.Name), pkColumn.Name));
            }

            return new CodeSnippetStatement(TABS + string.Format("CompositeId(){0};", keyPropertyBuilder));
        }
    }


}