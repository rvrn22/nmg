using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using Microsoft.CSharp;
using Microsoft.VisualBasic;
using NMG.Core.Domain;
using NMG.Core.Util;

namespace NMG.Core.Generator
{
    public class CodeGenerator : AbstractGenerator
    {
        private readonly ApplicationPreferences applicationPreferences;
        private readonly Language language;

        public CodeGenerator(ApplicationPreferences applicationPreferences, ColumnDetails columnDetails)
            : base(
                applicationPreferences.FolderPath, applicationPreferences.TableName, applicationPreferences.NameSpace,
                applicationPreferences.AssemblyName, applicationPreferences.Sequence, columnDetails)
        {
            this.applicationPreferences = applicationPreferences;
            language = applicationPreferences.Language;
        }

        public override void Generate()
        {
            var compileUnit = GetCompileUnit();
            WriteToFile(compileUnit, tableName.GetFormattedText());
        }

        public CodeCompileUnit GetCompileUnit()
        {
            var codeGenerationHelper = new CodeGenerationHelper();
            var compileUnit = codeGenerationHelper.GetCodeCompileUnit(nameSpace, tableName);

            var mapper = new DataTypeMapper();
            var newType = compileUnit.Namespaces[0].Types[0];
            foreach (var columnDetail in columnDetails)
            {
                var propertyName = columnDetail.ColumnName.GetPreferenceFormattedText(applicationPreferences);
                var mapFromDbType = mapper.MapFromDBType(columnDetail.DataType, columnDetail.DataLength, columnDetail.DataPrecision, columnDetail.DataScale);

                if (columnDetail.IsPrimaryKey)
                {
                    var metadataReader = MetadataFactory.GetReader(applicationPreferences.ServerType, applicationPreferences.ConnectionString);
                    var foreignKeyTables = metadataReader.GetForeignKeyTables(columnDetail.ColumnName);
                    foreach (var foreignKeyTable in foreignKeyTables)
                    {
                        newType.Members.Add(codeGenerationHelper.CreateAutoProperty("IList<" + foreignKeyTable + ">", foreignKeyTable));
                    }
                }

                switch (applicationPreferences.FieldGenerationConvention)
                {
                    case FieldGenerationConvention.Property:
                        newType.Members.Add(codeGenerationHelper.CreateProperty(mapFromDbType, propertyName.MakeFirstCharUpperCase()));
                        newType.Members.Add(codeGenerationHelper.CreateField(mapFromDbType, propertyName.MakeFirstCharLowerCase()));
                        break;
                    case FieldGenerationConvention.AutoProperty:
                        var codeMemberProperty = codeGenerationHelper.CreateAutoProperty(mapFromDbType, propertyName);
                        newType.Members.Add(codeMemberProperty);
                        break;
                    default:
                        newType.Members.Add(codeGenerationHelper.CreateField(mapFromDbType, propertyName));
                        break;
                }
            }
            var constructor = new CodeConstructor {Attributes = MemberAttributes.Public};
            newType.Members.Add(constructor);
            return compileUnit;
        }

        private void WriteToFile(CodeCompileUnit compileUnit, string className)
        {
            var provider = GetCodeDomProvider();
            string sourceFile = GetCompleteFilePath(provider, className);
            using (provider)
            {
                var streamWriter = new StreamWriter(sourceFile);
                var textWriter = new IndentedTextWriter(streamWriter, "    ");
                using (textWriter)
                {
                    using (streamWriter)
                    {
                        var options = new CodeGeneratorOptions {BlankLinesBetweenMembers = false};
                        provider.GenerateCodeFromCompileUnit(compileUnit, textWriter, options);
                    }
                }
            }
            CleanupGeneratedFile(sourceFile);
        }

        private static void CleanupGeneratedFile(string sourceFile)
        {
            string entireContent;
            using (var reader = new StreamReader(sourceFile))
            {
                entireContent = reader.ReadToEnd();
            }
            entireContent = RemoveComments(entireContent);
            entireContent = AddStandardHeader(entireContent);
            entireContent = FixAutoProperties(entireContent);
            using (var writer = new StreamWriter(sourceFile))
            {
                writer.Write(entireContent);
            }
        }

        // Hack : Auto property generator is not there in CodeDom.
        private static string FixAutoProperties(string entireContent)
        {
            entireContent = entireContent.Replace(@"get {
            }", "get;");
            entireContent = entireContent.Replace(@"set {
            }", "set;");
            return entireContent;
        }

        private static string AddStandardHeader(string entireContent)
        {
            entireContent = "using System.Text; \n" + entireContent;
            entireContent = "using System.Collections.Generic; \n" + entireContent;
            entireContent = "using System; \n" + entireContent;
            return entireContent;
        }

        private static string RemoveComments(string entireContent)
        {
            int end = entireContent.LastIndexOf("----------");
            entireContent = entireContent.Remove(0, end + 10);
            return entireContent;
        }

        private string GetCompleteFilePath(CodeDomProvider provider, string className)
        {
            var fileName = filePath + className;
            return provider.FileExtension[0] == '.'
                       ? fileName + provider.FileExtension
                       : fileName + "." + provider.FileExtension;
        }

        private CodeDomProvider GetCodeDomProvider()
        {
            return language == Language.CSharp ? (CodeDomProvider) new CSharpCodeProvider() : new VBCodeProvider();
        }
    }
}