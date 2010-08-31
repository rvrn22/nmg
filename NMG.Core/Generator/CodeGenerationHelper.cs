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

        public CodeMemberProperty CreateAutoProperty(Type type, string propertyName, bool fieldIsNull)
        {
            bool setFieldAsNullable = fieldIsNull && IsNullable(type);
            var codeMemberProperty = new CodeMemberProperty
                                         {
                                             Name = propertyName,
                                             HasGet = true,
                                             HasSet = true,
                                             Attributes = MemberAttributes.Public,
                                             Type = (setFieldAsNullable ? new CodeTypeReference(typeof(System.Nullable)) : new CodeTypeReference(type))
                                         };
            if (setFieldAsNullable)
                codeMemberProperty.Type.TypeArguments.Add(type);

            return codeMemberProperty;
        }

        public CodeMemberProperty CreateAutoProperty(string typeName, string propertyName)
        {
            var codeMemberProperty = new CodeMemberProperty
                                         {
                                             Name = propertyName,
                                             HasGet = true,
                                             HasSet = true,
                                             Attributes = MemberAttributes.Public,
                                             Type = new CodeTypeReference(typeName) 
                                         };

            return codeMemberProperty;
        }

        // For Castle
        public CodeMemberProperty CreateAutoProperty(string typeName, string propertyName, CodeAttributeDeclaration attributeArgument)
        {
            var codeMemberProperty = new CodeMemberProperty
            {
                Name = propertyName,
                HasGet = true,
                HasSet = true,
                Attributes = MemberAttributes.Public,
                Type = new CodeTypeReference(typeName)
            };

            codeMemberProperty.CustomAttributes.Add(attributeArgument);

            return codeMemberProperty;
        }

        public CodeMemberField CreateField(Type type, string fieldName)
        {
            return new CodeMemberField(type, fieldName);
        }

        // http://bytes.com/topic/c-sharp/answers/515498-typeof-check-nullability
        // Should probably move this elsewhere...
        private static bool IsNullable(Type type)
        {
            return type.IsValueType ||
                (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>));
        }
    }
}