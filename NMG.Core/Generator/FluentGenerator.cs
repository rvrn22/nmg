using System.CodeDom.Compiler;
using System.IO;
using Microsoft.CSharp;
using Microsoft.VisualBasic;
using NMG.Core.Domain;
using NMG.Core.Util;

namespace NMG.Core.Generator
{
    public class FluentGenerator : AbstractGenerator
    {
        private readonly ApplicationPreferences applicationPreferences;
        private readonly Language language;

        public FluentGenerator(ApplicationPreferences applicationPreferences, ColumnDetails columnDetails)
            : base(
                applicationPreferences.FolderPath, applicationPreferences.TableName, applicationPreferences.NameSpace,
                applicationPreferences.AssemblyName, applicationPreferences.Sequence, columnDetails)
        {
            this.applicationPreferences = applicationPreferences;
            language = this.applicationPreferences.Language;
        }

        public override void Generate()
        {
            string entityName = tableName.GetFormattedText();
            string className = entityName + "Map";
            string completeFilePath = GetCompleteFilePath(GetCodeDomProvider(), className);
            const string template = "public class {0} : ClassMap<{1}>";
            string contents = string.Format(template, className, entityName);
            contents += "{ \n \n }";
            File.WriteAllText(completeFilePath, contents);
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
            return language == Language.CSharp ? (CodeDomProvider)new CSharpCodeProvider() : new VBCodeProvider();
        }
    }
}
