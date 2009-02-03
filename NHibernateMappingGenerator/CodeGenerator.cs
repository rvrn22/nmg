using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Data.OracleClient;
using System.IO;
using Microsoft.CSharp;

namespace NHibernateMappingGenerator
{
    public class CodeGenerator
    {
        private readonly string filePath;
        private readonly string nameSpace;
        private readonly string className;
        private readonly ColumnDetails columnDetails;

        public CodeGenerator(string filePath, string nameSpace, string className, ColumnDetails columnDetails)
        {
            this.filePath = filePath;
            this.nameSpace = nameSpace;
            this.className = className;
            this.columnDetails = columnDetails;
        }

        public void Generate()
        {
            var compileUnit = new CodeCompileUnit();
            var codeNamespace = new CodeNamespace(nameSpace);
            var firstimport = new CodeNamespaceImport("System");
            codeNamespace.Imports.Add(firstimport);

            var newType = new CodeTypeDeclaration(className) {Attributes = MemberAttributes.Public};
            foreach (ColumnDetail columnDetail in columnDetails)
            {
                string propertyName = columnDetail.ColumnName.GetFormattedText();
                var field = new CodeMemberField(GetMappedType(columnDetail.DataType), propertyName.MakeFirstCharLowerCase());
                newType.Members.Add(field);
            }
            var constructor = new CodeConstructor {Attributes = MemberAttributes.Public};
            var constructorexp = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("System.Console"), "WriteLine", new CodePrimitiveExpression("Inside Constructor ..."));
            constructor.Statements.Add(constructorexp);
            newType.Members.Add(constructor);

            codeNamespace.Types.Add(newType);
            compileUnit.Namespaces.Add(codeNamespace);

            WriteToFile(compileUnit, className, filePath);
        }

        private static Type GetMappedType(string dataType)
        {
            if(dataType == "DATE")
            {
                return typeof(DateTime);
            }
            if (dataType == "NUMBER")
            {
                return typeof(long);
            }
            return typeof(string);
        }

        private static void WriteToFile(CodeCompileUnit compileUnit, string className, string filePath)
        {
            String sourceFile;

            var csharpcodeprovider = new CSharpCodeProvider();

            if (csharpcodeprovider.FileExtension[0] == '.')
            {
                sourceFile = filePath + className + csharpcodeprovider.FileExtension;
            }
            else
            {
                sourceFile = filePath + className + "." + csharpcodeprovider.FileExtension;
            }

            var streamWriter = new StreamWriter(sourceFile, false);
            var textWriter = new IndentedTextWriter(streamWriter, "    ");
            using(textWriter)
            {
                using(streamWriter)
                {
                    csharpcodeprovider.GenerateCodeFromCompileUnit(compileUnit, textWriter, new CodeGeneratorOptions());        
                }
            }
        }
    }
}