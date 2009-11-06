using System;
using System.IO;
using System.Xml;
using NMG.Core.Domain;
using NMG.Core.Util;

namespace NMG.Core.Generator
{
    public abstract class MappingGenerator : AbstractGenerator
    {
        private readonly ApplicationPreferences applicationPreferences;

        protected MappingGenerator(ApplicationPreferences applicationPreferences, ColumnDetails columnDetails)
            : base(applicationPreferences.FolderPath, applicationPreferences.TableName, applicationPreferences.NameSpace, applicationPreferences.AssemblyName, applicationPreferences.Sequence, columnDetails)
        {
            this.applicationPreferences = applicationPreferences;
        }

        protected abstract void AddIdGenerator(XmlDocument xmldoc, XmlElement idElement);

        public override void Generate()
        {
            string fileName = filePath + tableName.GetFormattedText() + ".hbm.xml";
            using (var stringWriter = new StringWriter())
            {
                var xmldoc = CreateMappingDocument();
                xmldoc.Save(stringWriter);
                string generatedXML = RemoveEmptyNamespaces(stringWriter.ToString());

                using (var writer = new StreamWriter(fileName))
                {
                    writer.Write(generatedXML);
                    writer.Flush();
                }
            }
        }

        private static string RemoveEmptyNamespaces(string mappingContent)
        {
            mappingContent = mappingContent.Replace("utf-16", "utf-8");
            return mappingContent.Replace("xmlns=\"\"", "");
        }

        public XmlDocument CreateMappingDocument()
        {
            var xmldoc = new XmlDocument();
            var xmlDeclaration = xmldoc.CreateXmlDeclaration("1.0", string.Empty, string.Empty);
            xmldoc.AppendChild(xmlDeclaration);
            var root = xmldoc.CreateElement("hibernate-mapping", "urn:nhibernate-mapping-2.2");
            root.SetAttribute("assembly", assemblyName);
            xmldoc.AppendChild(root);

            var classElement = xmldoc.CreateElement("class");
            classElement.SetAttribute("name", nameSpace + "." + tableName.GetFormattedText() + ", " + assemblyName);
            classElement.SetAttribute("table", tableName);
            classElement.SetAttribute("lazy", "true");
            root.AppendChild(classElement);
            var primaryKeyColumn = columnDetails.Find(detail => detail.IsPrimaryKey);
            if (primaryKeyColumn != null)
            {
                var idElement = xmldoc.CreateElement("id");
                string propertyName = primaryKeyColumn.ColumnName.GetPreferenceFormattedText(applicationPreferences);
                if (applicationPreferences.FieldGenerationConvention == FieldGenerationConvention.Property)
                {
                    idElement.SetAttribute("name", propertyName.MakeFirstCharLowerCase());
                }else
                {
                    idElement.SetAttribute("name", propertyName);
                }
                var mapper = new DataTypeMapper();
                Type mapFromDbType = mapper.MapFromDBType(primaryKeyColumn.DataType, primaryKeyColumn.DataLength, primaryKeyColumn.DataPrecision, primaryKeyColumn.DataScale);
                idElement.SetAttribute("type", mapFromDbType.Name);
                idElement.SetAttribute("column", primaryKeyColumn.ColumnName);
                if (applicationPreferences.FieldGenerationConvention != FieldGenerationConvention.AutoProperty)
                {
                    idElement.SetAttribute("access", "field");
                }
                classElement.AppendChild(idElement);
                AddIdGenerator(xmldoc, idElement);
            }

            AddAllProperties(xmldoc, classElement);
            return xmldoc;
        }

        private void AddAllProperties(XmlDocument xmldoc, XmlNode classElement)
        {
            foreach (var columnDetail in columnDetails)
            {
                if (columnDetail.IsPrimaryKey)
                    continue;
                var xmlNode = xmldoc.CreateElement("property");
                string propertyName = columnDetail.ColumnName.GetPreferenceFormattedText(applicationPreferences);
                if (applicationPreferences.FieldGenerationConvention == FieldGenerationConvention.Property)
                {
                    xmlNode.SetAttribute("name", propertyName.MakeFirstCharLowerCase());    
                }else
                {
                    xmlNode.SetAttribute("name", propertyName);
                }
                
                xmlNode.SetAttribute("column", columnDetail.ColumnName);
                if (applicationPreferences.FieldGenerationConvention != FieldGenerationConvention.AutoProperty)
                {
                    xmlNode.SetAttribute("access", "field");
                }
                classElement.AppendChild(xmlNode);
            }
        }
    }
}