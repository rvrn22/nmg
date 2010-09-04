using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CSharp;
using Microsoft.VisualBasic;
using NMG.Core.Domain;
using NMG.Core.Reader;
using NMG.Core.Util;
using NMG.Core.TextFormatter;

namespace NMG.Core.Generator
{
    public class CodeGenerator : AbstractGenerator
    {
        private readonly ApplicationPreferences applicationPreferences;
        private readonly Language language;

        public CodeGenerator(ApplicationPreferences applicationPreferences, Table table)
            : base(
                applicationPreferences.FolderPath, applicationPreferences.TableName, applicationPreferences.NameSpace,
                applicationPreferences.AssemblyName, applicationPreferences.Sequence, table)
        {
            this.applicationPreferences = applicationPreferences;
            language = applicationPreferences.Language;
        }

        public override void Generate()
        {
            CodeCompileUnit compileUnit = GetCompileUnit();
            WriteToFile(compileUnit, tableName.GetFormattedText());
        }

        public CodeCompileUnit GetCompileUnit()
        {
            var codeGenerationHelper = new CodeGenerationHelper();
            // This is where we construct the constructor
            CodeCompileUnit compileUnit = codeGenerationHelper.GetCodeCompileUnit(nameSpace,
                                                                                  Table.Name.GetFormattedText().MakeSingular());

            var mapper = new DataTypeMapper();
            CodeTypeDeclaration newType = compileUnit.Namespaces[0].Types[0];

            foreach(var pk in Table.PrimaryKey.Columns)
            {
                Type mapFromDbType = mapper.MapFromDBType(pk.DataType, null, null, null);
                newType.Members.Add(codeGenerationHelper.CreateAutoProperty(
                    mapFromDbType.ToString(),
                    pk.Name.GetFormattedText()
                    ));
            }

            // Note that a foreign key referencing a primary within the same table will end up giving you a foreign key property with the same name as the table.
            foreach (var fk in Table.ForeignKeys)
            {
                Type mapFromDbType = mapper.MapFromDBType(fk.Name, null, null, null);
                
                newType.Members.Add(codeGenerationHelper.CreateAutoProperty(
                    fk.References.GetFormattedText().MakeSingular(),
                    fk.References.GetFormattedText().MakeSingular()
                    ));
            }

            foreach(var hasMany in Table.HasManyRelationships)
            {
                newType.Members.Add(
                    codeGenerationHelper.CreateAutoProperty(
                        "IList<" + hasMany.Reference.GetFormattedText().MakeSingular() + ">", hasMany.Reference.GetFormattedText().MakePlural()));
            }

            foreach(var column in Table.Columns.Where(x => x.IsPrimaryKey != true && x.IsForeignKey != true))
            {
                Type mapFromDbType = mapper.MapFromDBType(column.DataType, null, null, null);
                newType.Members.Add(codeGenerationHelper.CreateAutoProperty(mapFromDbType, column.Name.GetFormattedText(),
                                                                            column.IsNullable));
            }
            //foreach (ColumnDetail columnDetail in columnDetails)
            //{
            //    //if(columnDetail.ColumnName == tableName)
            //    //    columnDetail.ColumnName = columnDetail.ColumnName + "_";

            //    string propertyName = columnDetail.ColumnName.GetPreferenceFormattedText(applicationPreferences);
            //    Type mapFromDbType = mapper.MapFromDBType(columnDetail.DataType, columnDetail.DataLength,
            //                                              columnDetail.DataPrecision, columnDetail.DataScale);

            //    if (columnDetail.IsPrimaryKey)
            //    {
            //        IMetadataReader metadataReader = MetadataFactory.GetReader(applicationPreferences.ServerType,
            //                                                                   applicationPreferences.ConnectionString);
            //        List<string> foreignKeyTables = metadataReader.GetForeignKeyTables(columnDetail.PropertyName);
            //        foreach (string foreignKeyTable in foreignKeyTables)
            //        {
            //            // Pluralize property name
            //            newType.Members.Add(
            //                codeGenerationHelper.CreateAutoProperty(
            //                    "IList<" + foreignKeyTable.GetFormattedText() + ">", foreignKeyTable.GetFormattedText() + "s"));
            //        }
            //    }

            //    if (columnDetail.IsForeignKey)
            //    {
            //        newType.Members.Add(codeGenerationHelper.CreateAutoProperty(
            //            columnDetail.ForeignKeyEntity.GetFormattedText(),
            //            columnDetail.ForeignKeyEntity.GetFormattedText()));
            //    }
            //        // Probably not the best way to solve this. But this way we don't generate a FK
            //        // and an auto property.
            //    else
            //    {
            //        switch (applicationPreferences.FieldGenerationConvention)
            //        {
            //            case FieldGenerationConvention.Property:
            //                newType.Members.Add(codeGenerationHelper.CreateProperty(mapFromDbType,
            //                                                                        propertyName.MakeFirstCharUpperCase()));
            //                newType.Members.Add(codeGenerationHelper.CreateField(mapFromDbType,
            //                                                                     propertyName.MakeFirstCharLowerCase()));
            //                break;
            //            case FieldGenerationConvention.AutoProperty:
            //                CodeMemberProperty codeMemberProperty =
            //                    codeGenerationHelper.CreateAutoProperty(mapFromDbType, propertyName.GetFormattedText(),
            //                                                            columnDetail.IsNullable);
            //                newType.Members.Add(codeMemberProperty);
            //                break;
            //            default:
            //                newType.Members.Add(codeGenerationHelper.CreateField(mapFromDbType,
            //                                                                     propertyName.GetFormattedText()));
            //                break;
            //        }
            //    }
            //}
            var constructor = new CodeConstructor {Attributes = MemberAttributes.Public};
            newType.Members.Add(constructor);
            return compileUnit;
        }

        private void WriteToFile(CodeCompileUnit compileUnit, string className)
        {
            CodeDomProvider provider = GetCodeDomProvider();
            string sourceFile = GetCompleteFilePath(provider, className.MakeSingular());
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
//            entireContent = entireContent.Replace(@"get {
//            }", "get;");
//            entireContent = entireContent.Replace(@"set {
//            }", "set;");
            // Do NOT mess with this...
            entireContent = entireContent.Replace(@"{
        }", "{ }");
            entireContent = entireContent.Replace(
            @"{
            get {
            }
            set {
            }
        }", "{ get; set; }");
            return entireContent;
        }

        private static string AddStandardHeader(string entireContent)
        {
            entireContent = "using System; \n" + entireContent;
            entireContent = "using System.Text; \n" + entireContent;
            entireContent = "using System.Collections.Generic; \n" + entireContent;
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
            string fileName = filePath + className;
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