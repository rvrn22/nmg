using System;
using System.CodeDom;
using NMG.Core.Domain;
using NMG.Core.TextFormatter;
using NMG.Core.Util;

namespace NMG.Core.Generator
{
    public class ContractGenerator : AbstractCodeGenerator
    {
        private readonly ApplicationPreferences applicationData;
        private readonly Table tableDetails;
        private readonly string entityName;

        public ContractGenerator(ApplicationPreferences applicationData, Table tableDetails)  : base(applicationData.FolderPath, applicationData.TableName, applicationData.NameSpace, applicationData.AssemblyName, applicationData.Sequence, tableDetails, applicationData)
        {
            this.applicationData = applicationData;
            this.tableDetails = tableDetails;
            entityName = applicationData.EntityName;
        }

        public override void Generate()
        {
            string className = Formatter.FormatSingular(Table.Name) + "Data";
            var compileUnit = GetCompileUnit();
            var generateCode = GenerateCode(compileUnit, className);
            WriteToFile(generateCode, className);
        }

        public CodeCompileUnit GetCompileUnit()
        {
            var codeGenerationHelper = new CodeGenerationHelper();
            var compileUnit = codeGenerationHelper.GetCodeCompileUnit(nameSpace, entityName);
            
            var mapper = new DataTypeMapper();
            var newType = compileUnit.Namespaces[0].Types[0];

            var nameArgument = new CodeAttributeArgument("Name", new CodeSnippetExpression("\""+ applicationData.EntityName + "\" "));
            var nameSpaceArgument = new CodeAttributeArgument("Namespace", new CodeSnippetExpression("\"\""));
            newType.CustomAttributes = new CodeAttributeDeclarationCollection {new CodeAttributeDeclaration("DataContract", nameArgument, nameSpaceArgument)};

            foreach (var columnDetail in tableDetails.Columns)
            {
                if(columnDetail.IsPrimaryKey)
                {
                    foreach (var foreignKeyTable in tableDetails.HasManyRelationships)
                    {
                        var fkEntityName = foreignKeyTable.Reference.MakeSingular();
                        newType.Members.Add(codeGenerationHelper.CreateAutoPropertyWithDataMemberAttribute("IList<" + fkEntityName + ">", foreignKeyTable.ReferenceColumn));
                    }

                    var primaryKeyType = mapper.MapFromDBType(columnDetail.DataType, columnDetail.DataLength, null, null);
                    newType.Members.Add(codeGenerationHelper.CreateAutoPropertyWithDataMemberAttribute(primaryKeyType.Name, "Id"));
                    continue;
                }
                if(columnDetail.IsForeignKey)
                {
                    var typeName = columnDetail.ForeignKeyEntity.MakeSingular();
                    var codeMemberProperty = codeGenerationHelper.CreateAutoPropertyWithDataMemberAttribute(typeName, columnDetail.Name);
                    newType.Members.Add(codeMemberProperty);
                    continue;
                }
                var propertyName = columnDetail.Name.GetPreferenceFormattedText(applicationData);
                var mapFromDbType = mapper.MapFromDBType(columnDetail.DataType, columnDetail.DataLength, null, null);

                newType.Members.Add(codeGenerationHelper.CreateAutoPropertyWithDataMemberAttribute(mapFromDbType.Name, propertyName));
            }
            return compileUnit;
        }

        protected override string AddStandardHeader(string entireContent)
        {
            entireContent = string.Format("using {0};", applicationData.NameSpace) + Environment.NewLine + entireContent;
            return base.AddStandardHeader(entireContent);
        }

    }
}