using System;
using System.CodeDom;
using System.Globalization;
using System.Linq;
using System.Text;
using NMG.Core.Domain;
using NMG.Core.ByCode;
using NMG.Core.TextFormatter;

namespace NMG.Core.Generator
{
    public class ByCodeGenerator : AbstractCodeGenerator
    {
        private readonly ApplicationPreferences appPrefs;

        public ByCodeGenerator(ApplicationPreferences appPrefs, Table table) : base(appPrefs.FolderPath, "Mapping", appPrefs.TableName, appPrefs.NameSpace, appPrefs.AssemblyName, appPrefs.Sequence, table, appPrefs)
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

            newType.BaseTypes.Add(string.Format("ClassMapping<{0}{1}>", appPrefs.ClassNamePrefix, Formatter.FormatSingular(Table.Name)));

            var constructor = new CodeConstructor {Attributes = MemberAttributes.Public};
            constructor.Statements.Add(new CodeSnippetStatement(TABS + "Table(\"" + Table.Name + "\");"));
            constructor.Statements.Add(new CodeSnippetStatement(TABS + string.Format("Lazy({0});", appPrefs.UseLazy ? "true" : "false")));
            var mapper = new DBColumnMapper();
            if (UsesSequence)
            {
                constructor.Statements.Add(new CodeSnippetStatement(TABS + mapper.IdSequenceMap(Table.PrimaryKey.Columns[0], appPrefs.Sequence, Formatter)));
            }
            else if (Table.PrimaryKey.Type == PrimaryKeyType.PrimaryKey)
            {
                constructor.Statements.Add(new CodeSnippetStatement(TABS + mapper.IdMap(Table.PrimaryKey.Columns[0], Formatter)));
            }
            else
            {
                
            }

            foreach (var column in Table.Columns.Where(x => x.IsPrimaryKey != true))
            {
                if (!appPrefs.IncludeForeignKeys && column.IsForeignKey)
                    continue;
                constructor.Statements.Add(new CodeSnippetStatement(TABS + mapper.Map(column, Formatter)));
            }
            
            foreach (var fk in Table.ForeignKeys.Where(fk => !string.IsNullOrEmpty(fk.References)))
            {
                constructor.Statements.Add(new CodeSnippetStatement(mapper.Reference(fk, Formatter)));
            }
            
            Table.HasManyRelationships.ToList().ForEach(x => constructor.Statements.Add(new CodeSnippetStatement(mapper.Bag(x, Formatter))));

            newType.Members.Add(constructor);
            return compileUnit;
        }
        
        protected override string AddStandardHeader(string entireContent)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("using System;");
            builder.AppendLine("using System.Text;");
            builder.AppendLine("using System.Collections.Generic;");
            builder.AppendLine("using System.Linq;");
            builder.AppendLine("using NHibernate.Mapping.ByCode.Conformist;");
            builder.AppendLine("using NHibernate.Mapping.ByCode;");
            builder.AppendFormat("using {0};", appPrefs.NameSpace);
            builder.AppendLine();
            if (appPrefs.ForeignEntityCollectionType.Contains("Iesi.Collections"))
                builder.AppendLine("using Iesi.Collections.Generic;");
            builder.Append(entireContent);
            return builder.ToString();
        }
    }
}

