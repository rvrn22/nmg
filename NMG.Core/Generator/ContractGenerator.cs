using System;
using System.CodeDom;
using NMG.Core.Domain;
using NMG.Core.TextFormatter;
using NMG.Core.Util;

namespace NMG.Core.Generator
{
    public class ContractGenerator : AbstractCodeGenerator
    {
        private readonly ApplicationPreferences appPrefs;
        private readonly Table table;

        public ContractGenerator(ApplicationPreferences appPrefs, Table table)  : base(appPrefs.FolderPath, "Contract", appPrefs.TableName, appPrefs.NameSpace, appPrefs.AssemblyName, appPrefs.Sequence, table, appPrefs)
        {
            this.appPrefs = appPrefs;
            this.table = table;
        }

        public override void Generate()
        {
			string className = appPrefs.ClassNamePrefix + Formatter.FormatSingular(Table.Name) + "Data";
			var compileUnit = GetCompileUnit(className);
            var generateCode = GenerateCode(compileUnit, className);
            WriteToFile(generateCode, className);
        }

		public CodeCompileUnit GetCompileUnit(string className)
        {
            var codeGenerationHelper = new CodeGenerationHelper();
			var compileUnit = codeGenerationHelper.GetCodeCompileUnit(nameSpace, className);
            
            var mapper = new DataTypeMapper();
            var newType = compileUnit.Namespaces[0].Types[0];

			var nameArgument = new CodeAttributeArgument("Name", new CodeSnippetExpression("\"" + className + "\" "));
            var nameSpaceArgument = new CodeAttributeArgument("Namespace", new CodeSnippetExpression("\"\""));
            newType.CustomAttributes = new CodeAttributeDeclarationCollection {new CodeAttributeDeclaration("DataContract", nameArgument, nameSpaceArgument)};

            foreach (var column in table.Columns)
            {
                if(column.IsPrimaryKey)
                {
                    foreach (var foreignKeyTable in table.HasManyRelationships)
                    {
						var fkEntityName = appPrefs.ClassNamePrefix + foreignKeyTable.Reference.MakeSingular().GetPreferenceFormattedText(appPrefs);
						newType.Members.Add(codeGenerationHelper.CreateAutoPropertyWithDataMemberAttribute("IList<" + fkEntityName + ">", foreignKeyTable.Reference.MakePlural().GetPreferenceFormattedText(appPrefs)));
                    }

                    var primaryKeyType = mapper.MapFromDBType(this.appPrefs.ServerType, column.DataType, column.DataLength, column.DataPrecision, column.DataScale);
                    newType.Members.Add(codeGenerationHelper.CreateAutoPropertyWithDataMemberAttribute(primaryKeyType.Name, "Id"));
                    continue;
                }
				if (column.IsForeignKey)
                {
                	var fKey = table.ForeignKeyReferenceForColumn(column);
					var typeName = appPrefs.ClassNamePrefix + fKey.MakeSingular().GetPreferenceFormattedText(appPrefs);
					var codeMemberProperty = codeGenerationHelper.CreateAutoPropertyWithDataMemberAttribute(typeName, fKey.MakeSingular().GetPreferenceFormattedText(appPrefs));
                    newType.Members.Add(codeMemberProperty);
                    continue;
                }
                var propertyName = column.Name.GetPreferenceFormattedText(appPrefs);
                var mapFromDbType = mapper.MapFromDBType(this.appPrefs.ServerType, column.DataType, column.DataLength, column.DataPrecision, column.DataScale);

                newType.Members.Add(codeGenerationHelper.CreateAutoPropertyWithDataMemberAttribute(mapFromDbType.Name, propertyName));
            }
            return compileUnit;
        }

        protected override string AddStandardHeader(string entireContent)
        {
            entireContent = string.Format("using {0};", appPrefs.NameSpace) + Environment.NewLine + entireContent;
            return base.AddStandardHeader(entireContent);
        }

    }
}