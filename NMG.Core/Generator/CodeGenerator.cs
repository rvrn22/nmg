using System;
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
            var compileUnit = new CodeCompileUnit();
            var codeNamespace = new CodeNamespace(nameSpace);

            var mapper = new DataTypeMapper();
            var newType = new CodeTypeDeclaration(tableName) {Attributes = MemberAttributes.Public};
            foreach (var columnDetail in columnDetails)
            {
                var codeGenerationHelper = new CodeGenerationHelper();
                string propertyName = columnDetail.ColumnName.GetPreferenceFormattedText(applicationPreferences);
                Type mapFromDbType = mapper.MapFromDBType(columnDetail.DataType, columnDetail.DataLength, columnDetail.DataPrecision, columnDetail.DataScale);

                if (applicationPreferences.FieldGenerationConvention == FieldGenerationConvention.Property)
                {
                   newType.Members.Add(codeGenerationHelper.CreateProperty(mapFromDbType, propertyName));
                   newType.Members.Add(codeGenerationHelper.CreateField(mapFromDbType, propertyName.MakeFirstCharLowerCase()));
                }
                else if (applicationPreferences.FieldGenerationConvention == FieldGenerationConvention.AutoProperty)
                {
                    var codeMemberProperty = codeGenerationHelper.CreateProperty(mapFromDbType, propertyName);
                    newType.Members.Add(codeMemberProperty);
                }
                else
                {
                    newType.Members.Add(codeGenerationHelper.CreateField(mapFromDbType, propertyName));
                }
            }
            var constructor = new CodeConstructor {Attributes = MemberAttributes.Public};
            newType.Members.Add(constructor);

            codeNamespace.Types.Add(newType);
            compileUnit.Namespaces.Add(codeNamespace);

            WriteToFile(compileUnit, tableName.GetFormattedText());
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
            using (var writer = new StreamWriter(sourceFile))
            {
                writer.Write(entireContent);
            }
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