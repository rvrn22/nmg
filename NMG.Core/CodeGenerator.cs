using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using Microsoft.CSharp;

namespace NMG.Core
{
    public class CodeGenerator : BaseCodeGenerator
    {
        public CodeGenerator(string filePath, List<string> tableName, string nameSpace, string assemblyName, string sequenceNumber, ColumnDetails columnDetails)
            : base(filePath, tableName, nameSpace, assemblyName, sequenceNumber, columnDetails)
        {
        }

        public override void Generate()
        {
            foreach (var tableName in tableNames)
            {
                var compileUnit = new CodeCompileUnit();
                var codeNamespace = new CodeNamespace(nameSpace);
                var firstimport = new CodeNamespaceImport("System");
                codeNamespace.Imports.Add(firstimport);
                var mapper = new DataTypeMapper();
                var newType = new CodeTypeDeclaration(tableName) {Attributes = MemberAttributes.Public};
                foreach (ColumnDetail columnDetail in columnDetails)
                {
                    string propertyName = columnDetail.ColumnName.GetFormattedText();
                    var field = new CodeMemberField(mapper.MapFromDBType(columnDetail.DataType), propertyName.MakeFirstCharLowerCase());
                    newType.Members.Add(field);
                }
                var constructor = new CodeConstructor {Attributes = MemberAttributes.Public};
                var constructorexp = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("System.Console"), "WriteLine", new CodePrimitiveExpression("Inside Constructor ..."));
                constructor.Statements.Add(constructorexp);
                newType.Members.Add(constructor);

                codeNamespace.Types.Add(newType);
                compileUnit.Namespaces.Add(codeNamespace);

                WriteToFile(compileUnit, tableName.GetFormattedText(), filePath);
            }
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
            using (textWriter)
            {
                using (streamWriter)
                {
                    csharpcodeprovider.GenerateCodeFromCompileUnit(compileUnit, textWriter, new CodeGeneratorOptions());
                }
            }
        }
    }
}