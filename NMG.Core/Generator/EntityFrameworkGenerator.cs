using System;
using System.CodeDom;
using System.Linq;
using System.Text;
using NMG.Core.Domain;
using NMG.Core.Fluent;
using NMG.Core.TextFormatter;

namespace NMG.Core.Generator
{
    public class EntityFrameworkGenerator : AbstractCodeGenerator
    {
        private readonly ApplicationPreferences appPrefs;

        public EntityFrameworkGenerator(ApplicationPreferences appPrefs, Table table)
            : base(appPrefs.FolderPath, "Mapping", appPrefs.TableName, appPrefs.NameSpaceMap, appPrefs.AssemblyName, appPrefs.Sequence, table, appPrefs)
        {
            this.appPrefs = appPrefs;
            language = this.appPrefs.Language;
        }

        public override void Generate(bool writeToFile = true)
        {
            var pascalCaseTextFormatter = new PascalCaseTextFormatter {PrefixRemovalList = appPrefs.FieldPrefixRemovalList};
            var className = string.Format("{0}{1}{2}", appPrefs.ClassNamePrefix, pascalCaseTextFormatter.FormatSingular(Table.Name), "Map");
            var compileUnit = GetCompleteCompileUnit(className);
            var generateCode = GenerateCode(compileUnit, className);

            if (writeToFile)
            {
                WriteToFile(generateCode, className);
            }
            else
            {
                GeneratedCode = WriteToString(compileUnit, GetCodeDomProvider());
            }
        }

        protected override string CleanupGeneratedFile(string generatedContent)
        {
            return generatedContent;
        }

        public CodeCompileUnit GetCompleteCompileUnit(string className)
        {
            var codeGenerationHelper = new CodeGenerationHelper();
            var compileUnit = codeGenerationHelper.GetCodeCompileUnit(nameSpace, className);

            var newType = compileUnit.Namespaces[0].Types[0];

            newType.IsPartial = appPrefs.GeneratePartialClasses;
            var pascalCaseTextFormatter = new PascalCaseTextFormatter {PrefixRemovalList = appPrefs.FieldPrefixRemovalList};
            newType.BaseTypes.Add(string.Format("EntityTypeConfiguration<{0}{1}>", appPrefs.ClassNamePrefix, pascalCaseTextFormatter.FormatSingular(Table.Name)));

            var constructor = new CodeConstructor {Attributes = MemberAttributes.Public};
            constructor.Statements.Add(new CodeSnippetStatement(TABS + "ToTable(\"" + Table.Name + "\");"));
            if (appPrefs.UseLazy)
                constructor.Statements.Add(new CodeSnippetStatement(TABS + "LazyLoad();"));

            if (UsesSequence)
            {
                var fieldName = FixPropertyWithSameClassName(Table.PrimaryKey.Columns[0].Name, Table.Name);
                constructor.Statements.Add(new CodeSnippetStatement(String.Format(TABS + "Id(x => x.{0}).Column(x => x.{1}).GeneratedBy.Sequence(\"{2}\")", Formatter.FormatText(fieldName), fieldName, appPrefs.Sequence)));
            }
            else if (Table.PrimaryKey != null && Table.PrimaryKey.Type == PrimaryKeyType.PrimaryKey)
            {
                var fieldName = FixPropertyWithSameClassName(Table.PrimaryKey.Columns[0].Name, Table.Name);
                constructor.Statements.Add(GetIdMapCodeSnippetStatement(appPrefs, Table, Table.PrimaryKey.Columns[0].Name, fieldName, Table.PrimaryKey.Columns[0].DataType, Formatter));
            }
            else if (Table.PrimaryKey != null)
            {
                constructor.Statements.Add(GetIdMapCodeSnippetStatement(Table.PrimaryKey, Table, Formatter));
            }

            // Many To One Mapping
            foreach (var fk in Table.ForeignKeys.Where(fk => fk.Columns.First().IsForeignKey && appPrefs.IncludeForeignKeys))
            {
                var propertyName = appPrefs.NameFkAsForeignTable ? fk.UniquePropertyName : fk.Columns.First().Name;
                propertyName = Formatter.FormatSingular(propertyName);
                var fieldName = FixPropertyWithSameClassName(propertyName, Table.Name);
                
                var propertyMapType = "HasRequired";
                if (fk.IsNullable)
                {
                    propertyMapType = "HasOptional";
                }

                var codeSnippet = string.Format(TABS + "{0}(x => x.{1}).WithMany(t => t.{2}).HasForeignKey(d => d.{3});", propertyMapType, fieldName, fk.Columns.First().ForeignKeyTableName, fk.Columns.First().ForeignKeyColumnName);
                constructor.Statements.Add(new CodeSnippetStatement(codeSnippet));
            }

            foreach (var column in Table.Columns.Where(x => !x.IsPrimaryKey && (!x.IsForeignKey || !appPrefs.IncludeForeignKeys)))
            {
                var propertyName = Formatter.FormatText(column.Name);
                var fieldName = FixPropertyWithSameClassName(propertyName, Table.Name);
                var columnMapping = new DBColumnMapper().Map(column, fieldName, Formatter, appPrefs.IncludeLengthAndScale);
                constructor.Statements.Add(new CodeSnippetStatement(TABS + columnMapping));
            }

            if (appPrefs.IncludeHasMany)
            {
                Table.HasManyRelationships.ToList().ForEach(x => constructor.Statements.Add(new EFOneToMany(Formatter, pascalCaseTextFormatter).Create(x)));
            }

            newType.Members.Add(constructor);
            return compileUnit;
        }

        private static string FixPropertyWithSameClassName(string property, string className)
        {
            return property.ToLowerInvariant() == className.ToLowerInvariant() ? property + "Val" : property;
        }

        protected override string AddStandardHeader(string entireContent)
        {
            entireContent = "using " + appPrefs.NameSpace + "; " + entireContent;
            entireContent = string.Format("using System.ComponentModel.DataAnnotations.Schema;{{0}}using System.Data.Entity.ModelConfiguration;{{0}}{0}", Environment.NewLine) + entireContent;
            return base.AddStandardHeader(entireContent);
        }

        private static CodeSnippetStatement GetIdMapCodeSnippetStatement(ApplicationPreferences appPrefs, Table table, string pkColumnName, string propertyName, string pkColumnType, ITextFormatter formatter)
        {
            var dataTypeMapper = new DataTypeMapper();
            bool isPkTypeIntegral = (dataTypeMapper.MapFromDBType(appPrefs.ServerType, pkColumnType, null, null, null)).IsTypeIntegral();

            string idGeneratorType = (isPkTypeIntegral ? "GeneratedBy.Identity()" : "GeneratedBy.Assigned()");
            var fieldName = FixPropertyWithSameClassName(propertyName, table.Name);
            var pkAlsoFkQty = (from fk in table.ForeignKeys.Where(fk => fk.UniquePropertyName == pkColumnName) select fk).Count();
            if (pkAlsoFkQty > 0) fieldName = fieldName + "Id";
            return new CodeSnippetStatement(string.Format(TABS + "Id(x => x.{0}).{1}.Column(\"{2}\");", formatter.FormatText(fieldName), idGeneratorType, pkColumnName));
        }

        private static CodeSnippetStatement GetIdMapCodeSnippetStatement(PrimaryKey primaryKey, Table table, ITextFormatter formatter)
        {
            var keyPropertyBuilder = new StringBuilder(primaryKey.Columns.Count);
            bool first = true;
            foreach (var pkColumn in primaryKey.Columns)
            {
                var propertyName = formatter.FormatText(pkColumn.Name);
                var fieldName = FixPropertyWithSameClassName(propertyName, table.Name);
                var pkAlsoFkQty = (from fk in table.ForeignKeys.Where(fk => fk.UniquePropertyName == pkColumn.Name) select fk).Count();
                if (pkAlsoFkQty > 0) fieldName = fieldName + "Id";
                var tmp = String.Format(".KeyProperty(x => x.{0}, \"{1}\")", fieldName, pkColumn.Name);
                keyPropertyBuilder.Append(first ? tmp : "\n" + TABS + "             " + tmp);
                first = false;
            }

            return new CodeSnippetStatement(TABS + string.Format("CompositeId(){0};", keyPropertyBuilder));
        }
    }

    public class EFOneToMany
    {
        private readonly PascalCaseTextFormatter pascalCaseTextFormatter;

        public EFOneToMany(ITextFormatter formatter, PascalCaseTextFormatter pascalCaseTextFormatter)
        {
            this.pascalCaseTextFormatter = pascalCaseTextFormatter;
            Formatter = formatter;
        }

        private ITextFormatter Formatter { get; set; }

        public CodeSnippetStatement Create(HasMany hasMany)
        {
            var hasManySnippet = string.Format("HasMany(x => x.{0}).WithMany(x => x.{1})", Formatter.FormatPlural(hasMany.Reference), pascalCaseTextFormatter.FormatSingular(hasMany.PKTableName));
            var keySnippet = string.Format(".Map(m => {{m.ToTable(\"{0}\"); m.MapLeftKey(\"{1}\"); m.MapRightKey(\"{2}\");}})", hasMany.Reference, hasMany.ReferenceColumn, hasMany.ReferenceColumn);
            return new CodeSnippetStatement(string.Format(AbstractGenerator.TABS + "{0}{1};", hasManySnippet, keySnippet));
        }
    }
}