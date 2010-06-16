using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.CSharp;
using Microsoft.VisualBasic;
using NMG.Core.Domain;

namespace NMG.Core.Generator
{
    public abstract class AbstractCodeGenerator : AbstractGenerator
    {
        protected Language language;
        protected readonly List<string> AlreadyMappedColumnNames = new List<string> { "Description", "Inactive", "CreatedDate", "CreatedBy", "UpdatedDate", "UpdatedBy" };

        protected AbstractCodeGenerator(string filePath, string tableName, string nameSpace, string assemblyName,
                                        string sequenceName, ColumnDetails columnDetails)
            : base(filePath, tableName, nameSpace, assemblyName, sequenceName, columnDetails)
        {
        }

        public string GenerateCode(CodeCompileUnit compileUnit, string className)
        {
            var provider = GetCodeDomProvider();
            var stringBuilder = new StringBuilder();
            using (provider)
            {
                var stringWriter = new StringWriter(stringBuilder);
                provider.GenerateCodeFromCompileUnit(compileUnit, stringWriter, new CodeGeneratorOptions());
            }
            return CleanupCode(stringBuilder.ToString());
        }

        protected void WriteToFile(string content, string fileName)
        {
            var provider = GetCodeDomProvider();
            string sourceFile = GetCompleteFilePath(provider, fileName);
            using (provider)
            {
                if (!File.Exists(sourceFile))
                {
                    File.WriteAllText(sourceFile, content);
                }
            }
        }

        private string CleanupCode(string entireContent)
        {
            entireContent = RemoveComments(entireContent);
            entireContent = AddStandardHeader(entireContent);
            return FixAutoProperties(entireContent);
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

        protected virtual string AddStandardHeader(string entireContent)
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

        private string GetCompleteFilePath(CodeDomProvider provider, string entityFileName)
        {
            if (!filePath.EndsWith("\\"))
                filePath += "\\";
            var fileName = filePath + entityFileName;
            return provider.FileExtension[0] == '.'
                       ? fileName + provider.FileExtension
                       : fileName + "." + provider.FileExtension;
        }

        protected CodeDomProvider GetCodeDomProvider()
        {
            return language == Language.CSharp ? (CodeDomProvider)new CSharpCodeProvider() : new VBCodeProvider();
        }
    }
}