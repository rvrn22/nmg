using System;
using System.CodeDom;
using NMG.Core.Util;

namespace NMG.Core.Generator
{
    public class CodeGenerationHelper
    {
        public CodeCompileUnit GetCodeCompileUnit(string nameSpace, string className)
        {
            var codeCompileUnit = new CodeCompileUnit();
            var codeNamespace = new CodeNamespace(nameSpace);
            var codeTypeDeclaration = new CodeTypeDeclaration(className);
            codeNamespace.Types.Add(codeTypeDeclaration);
            codeCompileUnit.Namespaces.Add(codeNamespace);
            return codeCompileUnit;
        }

        public CodeMemberProperty CreateProperty(Type type, string propertyName)
        {
            var codeMemberProperty = new CodeMemberProperty
                                         {
                                             Name = propertyName,
                                             HasGet = true,
                                             HasSet = true,
                                             Attributes = MemberAttributes.Public,
                                             Type = new CodeTypeReference(type)
                                         };
            var fieldName = propertyName.MakeFirstCharLowerCase();
            var codeFieldReferenceExpression = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName);
            var returnStatement = new CodeMethodReturnStatement(codeFieldReferenceExpression);
            codeMemberProperty.GetStatements.Add(returnStatement);
            var assignStatement = new CodeAssignStatement(codeFieldReferenceExpression, new CodePropertySetValueReferenceExpression());
            codeMemberProperty.SetStatements.Add(assignStatement);
            return codeMemberProperty;
        }

        public CodeMemberProperty CreateAutoProperty(Type type, string propertyName)
        {
            var codeMemberProperty = new CodeMemberProperty
                                         {
                                             Name = propertyName,
                                             HasGet = true,
                                             HasSet = true,
                                             Attributes = MemberAttributes.Public,
                                             Type = new CodeTypeReference(type) 
                                         };
//            var returnStatement = new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), propertyName));
//            codeMemberProperty.GetStatements.Add(returnStatement);
//            var assignStatement = new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), propertyName), new CodePropertySetValueReferenceExpression());
//            codeMemberProperty.SetStatements.Add(assignStatement);
            return codeMemberProperty;
        }


        public CodeMemberField CreateField(Type type, string fieldName)
        {
            return new CodeMemberField(type, fieldName);
        }
    }
}