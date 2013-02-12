using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CSharp;
using Microsoft.VisualBasic;
using NMG.Core.Domain;
using System.Text;

namespace NMG.Core.Generator
{
    public class CodeGenerator : AbstractGenerator
    {
        private readonly ApplicationPreferences appPrefs;
        private readonly Language language;

        public CodeGenerator(ApplicationPreferences appPrefs, Table table)
            : base(appPrefs.FolderPath, "Domain", appPrefs.TableName, appPrefs.NameSpace, appPrefs.AssemblyName, appPrefs.Sequence, table, appPrefs)
        {
            this.appPrefs = appPrefs;
            language = appPrefs.Language;
        }

        public override void Generate(bool writeToFile = true)
        {
            var className = string.Format("{0}{1}", appPrefs.ClassNamePrefix, Formatter.FormatSingular(Table.Name));
            var compileUnit = GetCompileUnit(className);

            if (writeToFile)
            {
                WriteToFile(compileUnit, className);
            }
            else
            {
                // Output to property
                GeneratedCode = WriteToString(compileUnit);
            }
        }

        public CodeCompileUnit GetCompileUnit(string className)
        {
            var codeGenerationHelper = new CodeGenerationHelper();
            var compileUnit = codeGenerationHelper.GetCodeCompileUnitWithInheritanceAndInterface(nameSpace, className, appPrefs.InheritenceAndInterfaces);

            var mapper = new DataTypeMapper();
            var newType = compileUnit.Namespaces[0].Types[0];

            newType.IsPartial = appPrefs.GeneratePartialClasses;

            CreateProperties(codeGenerationHelper, mapper, newType);

            // Generate GetHashCode() and Equals() methods.
            if (Table.PrimaryKey != null && Table.PrimaryKey.Type == PrimaryKeyType.CompositeKey)
            {
                var pkColsList = Table.PrimaryKey.Columns.Select(s => Formatter.FormatText(s.Name)).ToList();

                var equalsCode = CreateCompositeKeyEqualsMethod(pkColsList);
                var getHashKeyCode = CreateCompositeKeyGetHashKeyMethod(pkColsList);

                equalsCode.StartDirectives.Add(new CodeRegionDirective(CodeRegionMode.Start, "NHibernate Composite Key Requirements"));
                newType.Members.Add(equalsCode);
                newType.Members.Add(getHashKeyCode);
                getHashKeyCode.EndDirectives.Add(new CodeRegionDirective(CodeRegionMode.End, string.Empty));
            }

            // Dont create a constructor if there are no relationships.
            if (Table.HasManyRelationships.Count == 0)
                return compileUnit;

            var constructorStatements = new CodeStatementCollection();
            foreach (var hasMany in Table.HasManyRelationships)
            {
                newType.Members.Add(codeGenerationHelper.CreateAutoProperty(appPrefs.ForeignEntityCollectionType + "<" + appPrefs.ClassNamePrefix + Formatter.FormatSingular(hasMany.Reference) + ">", Formatter.FormatPlural(hasMany.Reference), appPrefs.UseLazy));
                constructorStatements.Add(new CodeSnippetStatement(string.Format(TABS + "{0} = new {1}<{2}{3}>();", Formatter.FormatPlural(hasMany.Reference), codeGenerationHelper.InstatiationObject(appPrefs.ForeignEntityCollectionType), appPrefs.ClassNamePrefix, Formatter.FormatSingular(hasMany.Reference))));
            }

            var constructor = new CodeConstructor { Attributes = MemberAttributes.Public };
            constructor.Statements.AddRange(constructorStatements);
            newType.Members.Add(constructor);
            return compileUnit;
        }

        private void CreateProperties(CodeGenerationHelper codeGenerationHelper, DataTypeMapper mapper, CodeTypeDeclaration newType)
        {
            switch (appPrefs.FieldGenerationConvention)
            {
                case FieldGenerationConvention.Field:
                    CreateFields(codeGenerationHelper, mapper, newType);
                    break;
                case FieldGenerationConvention.Property:
                    CreateFullProperties(codeGenerationHelper, mapper, newType);
                    break;
                case FieldGenerationConvention.AutoProperty:
                    CreateAutoProperties(codeGenerationHelper, mapper, newType);
                    break;
            }
        }

        private void CreateFields(CodeGenerationHelper codeGenerationHelper, DataTypeMapper mapper, CodeTypeDeclaration newType)
        {
            foreach (var pk in Table.PrimaryKey.Columns)
            {
                var mapFromDbType = mapper.MapFromDBType(this.appPrefs.ServerType, pk.DataType, pk.DataLength, pk.DataPrecision, pk.DataScale);
                newType.Members.Add(codeGenerationHelper.CreateField(mapFromDbType, Formatter.FormatText(pk.Name), true));
            }

            // Note that a foreign key referencing a primary within the same table will end up giving you a foreign key property with the same name as the table.
            foreach (var fk in Table.ForeignKeys.Where(fk => !string.IsNullOrEmpty(fk.References)))
            {
                newType.Members.Add(codeGenerationHelper.CreateField(appPrefs.ClassNamePrefix + Formatter.FormatSingular(fk.References), Formatter.FormatSingular(fk.UniquePropertyName)));
            }

            foreach (var column in Table.Columns.Where(x => x.IsPrimaryKey != true))
            {
                if (!appPrefs.IncludeForeignKeys && column.IsForeignKey)
                    continue;
                var mapFromDbType = mapper.MapFromDBType(this.appPrefs.ServerType, column.DataType, column.DataLength, column.DataPrecision, column.DataScale);
                newType.Members.Add(codeGenerationHelper.CreateField(mapFromDbType, Formatter.FormatText(column.Name), true, column.IsNullable));
            }
        }

        private void CreateFullProperties(CodeGenerationHelper codeGenerationHelper, DataTypeMapper mapper, CodeTypeDeclaration newType)
        {
            foreach (var pk in Table.PrimaryKey.Columns)
            {
                var mapFromDbType = mapper.MapFromDBType(this.appPrefs.ServerType, pk.DataType, pk.DataLength, pk.DataPrecision, pk.DataScale);
                newType.Members.Add(codeGenerationHelper.CreateField(mapFromDbType, Formatter.FormatText(pk.Name), true));
                newType.Members.Add(codeGenerationHelper.CreateProperty(mapFromDbType, Formatter.FormatText(pk.Name), appPrefs.UseLazy));
            }

            // Note that a foreign key referencing a primary within the same table will end up giving you a foreign key property with the same name as the table.
            foreach (var fk in Table.ForeignKeys.Where(fk => !string.IsNullOrEmpty(fk.References)))
            {
                newType.Members.Add(codeGenerationHelper.CreateField(appPrefs.ClassNamePrefix + Formatter.FormatSingular(fk.References), Formatter.FormatSingular(fk.UniquePropertyName)));
                newType.Members.Add(codeGenerationHelper.CreateProperty(appPrefs.ClassNamePrefix + Formatter.FormatSingular(fk.References), Formatter.FormatSingular(fk.UniquePropertyName), appPrefs.UseLazy));
            }

            foreach (var column in Table.Columns.Where(x => x.IsPrimaryKey != true))
            {
                if (!appPrefs.IncludeForeignKeys && column.IsForeignKey)
                    continue;
                var mapFromDbType = mapper.MapFromDBType(this.appPrefs.ServerType, column.DataType, column.DataLength, column.DataPrecision, column.DataScale);
                newType.Members.Add(codeGenerationHelper.CreateField(mapFromDbType, Formatter.FormatText(column.Name), true, column.IsNullable));
                newType.Members.Add(codeGenerationHelper.CreateProperty(mapFromDbType, Formatter.FormatText(column.Name), column.IsNullable, appPrefs.UseLazy));
            }
        }

        private void CreateAutoProperties(CodeGenerationHelper codeGenerationHelper, DataTypeMapper mapper, CodeTypeDeclaration newType)
        {
            if (Table.PrimaryKey != null)
            {
                foreach (var pk in Table.PrimaryKey.Columns)
                {
                    var mapFromDbType = mapper.MapFromDBType(this.appPrefs.ServerType, pk.DataType, pk.DataLength,
                                                             pk.DataPrecision, pk.DataScale);
                    newType.Members.Add(codeGenerationHelper.CreateAutoProperty(mapFromDbType.ToString(),
                                                                                Formatter.FormatText(pk.Name),
                                                                                appPrefs.UseLazy));
                }
            }

            // Note that a foreign key referencing a primary within the same table will end up giving you a foreign key property with the same name as the table.
            foreach (var fk in Table.ForeignKeys.Where(fk => !string.IsNullOrEmpty(fk.References)))
            {
                newType.Members.Add(codeGenerationHelper.CreateAutoProperty(appPrefs.ClassNamePrefix + Formatter.FormatSingular(fk.References), Formatter.FormatSingular(fk.UniquePropertyName), appPrefs.UseLazy));
            }

            foreach (var column in Table.Columns.Where(x => x.IsPrimaryKey != true))
            {
                if (!appPrefs.IncludeForeignKeys && column.IsForeignKey)
                    continue;
                var mapFromDbType = mapper.MapFromDBType(this.appPrefs.ServerType, column.DataType, column.DataLength, column.DataPrecision, column.DataScale);
                newType.Members.Add(codeGenerationHelper.CreateAutoProperty(mapFromDbType, Formatter.FormatText(column.Name), column.IsNullable, appPrefs.UseLazy));
            }
        }

        private CodeMemberMethod CreateCompositeKeyEqualsMethod(IList<string> columns)
        {
            if (columns.Count == 0)
                return null;

            var method = new CodeMemberMethod
            {
                Name = "Equals",
                ReturnType = new CodeTypeReference(typeof(bool)),
                Attributes = MemberAttributes.Public | MemberAttributes.Override,
            };

            method.Parameters.Add(new CodeParameterDeclarationExpression("System.Object", "obj"));

            // Create the if statement to compare if the obj equals another.
            var compareCode = new StringBuilder();

            var className = string.Format("{0}{1}", appPrefs.ClassNamePrefix, Formatter.FormatSingular(Table.Name));
            method.Statements.Add(new CodeSnippetStatement("\t\t\tif (obj == null) return false;"));
            method.Statements.Add(new CodeSnippetStatement(string.Format("\t\t\tvar t = obj as {0};", className)));
            method.Statements.Add(new CodeSnippetStatement("\t\t\tif (t == null) return false;"));

            compareCode.Append("\t\t\tif (");
            var lastCol = columns.LastOrDefault();
            foreach (var column in columns)
            {
                compareCode.Append(string.Format("{0} == t.{0}", column));
                compareCode.Append(column != lastCol ? " && " : ")");
            }
            method.Statements.Add(new CodeSnippetStatement(compareCode.ToString()));

            method.Statements.Add(new CodeSnippetStatement("\t\t\t\treturn true;"));
            method.Statements.Add(new CodeSnippetStatement(string.Empty));
            method.Statements.Add(new CodeSnippetStatement("\t\t\treturn false;"));

            return method;
        }

        private CodeMemberMethod CreateCompositeKeyGetHashKeyMethod(IList<string> columns)
        {
            if (columns.Count == 0)
                return null;

            var method = new CodeMemberMethod
            {
                Name = "GetHashCode",
                ReturnType = new CodeTypeReference(typeof(int)),
                Attributes = MemberAttributes.Public | MemberAttributes.Override,
            };

            // Create the if statement to compare if the obj equals another.
            method.Statements.Add(new CodeSnippetStatement("\t\t\tint hash = 13;"));

            foreach (var column in columns)
            {
                method.Statements.Add(new CodeSnippetStatement(string.Format("\t\t\thash += {0}.GetHashCode();", column)));
            }

            method.Statements.Add(new CodeSnippetStatement(string.Empty));
            method.Statements.Add(new CodeSnippetStatement("\t\t\treturn hash;"));

            return method;
        }

        private void WriteToFile(CodeCompileUnit compileUnit, string className)
        {
            var provider = GetCodeDomProvider();
            var sourceFile = GetCompleteFilePath(provider, className);
            var streamWriter = new StringWriter();
            using (provider)
            {
                var textWriter = new IndentedTextWriter(streamWriter, "    ");
                using (textWriter)
                {
                    using (streamWriter)
                    {
                        var options = new CodeGeneratorOptions { BlankLinesBetweenMembers = false };
                        provider.GenerateCodeFromCompileUnit(compileUnit, textWriter, options);
                    }
                }
            }
            var entireContent = CleanupGeneratedFile(streamWriter.ToString());

            using (var writer = new StreamWriter(sourceFile))
            {
                writer.Write(entireContent);
            }
        }

        private string WriteToString(CodeCompileUnit compileUnit)
        {
            var provider = GetCodeDomProvider();
            var streamWriter = new StringWriter();
            using (provider)
            {
                var textWriter = new IndentedTextWriter(streamWriter, "    ");
                using (textWriter)
                {
                    using (streamWriter)
                    {
                        var options = new CodeGeneratorOptions { BlankLinesBetweenMembers = false };
                        provider.GenerateCodeFromCompileUnit(compileUnit, textWriter, options);
                    }
                }
            }
            
            return CleanupGeneratedFile(streamWriter.ToString());
        }

        private string CleanupGeneratedFile(string entireContent)
        {
            entireContent = RemoveComments(entireContent);
            entireContent = AddStandardHeader(entireContent);
            entireContent = FixAutoProperties(entireContent);

            return entireContent;
        }

        // Hack : Auto property generator is not there in CodeDom.
        private static string FixAutoProperties(string entireContent)
        {
            // Do NOT mess with this... 
            //Indomitable: Just a little :)
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("{");
            builder.Append("        }");
            entireContent = entireContent.Replace(builder.ToString(), "{ }");
            builder.Clear();
            builder.AppendLine("{");
            builder.AppendLine("            get {");
            builder.AppendLine("            }");
            builder.AppendLine("            set {");
            builder.AppendLine("            }");
            builder.Append("        }");
            entireContent = entireContent.Replace(builder.ToString(), "{ get; set; }");
            return entireContent;
        }

        private string AddStandardHeader(string entireContent)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("using System;");
            builder.AppendLine("using System.Text;");
            builder.AppendLine("using System.Collections.Generic;");
            if (appPrefs.ForeignEntityCollectionType.Contains("Iesi.Collections"))
                builder.AppendLine("using Iesi.Collections.Generic;");
            builder.Append(entireContent);
            return builder.ToString();
        }

        private static string RemoveComments(string entireContent)
        {
            int end = entireContent.LastIndexOf("----------");
            entireContent = entireContent.Remove(0, end + 10);
            return entireContent;
        }

        private string GetCompleteFilePath(CodeDomProvider provider, string className)
        {
            string fileName = filePath + className;
            return provider.FileExtension[0] == '.'
                       ? fileName + provider.FileExtension
                       : fileName + "." + provider.FileExtension;
        }

        private CodeDomProvider GetCodeDomProvider()
        {
            return language == Language.CSharp ? (CodeDomProvider)new CSharpCodeProvider() : new VBCodeProvider();
        }
    }
}