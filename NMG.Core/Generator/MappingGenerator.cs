using System.IO;
using System.Linq;
using System.Xml;
using NMG.Core.Domain;
using NMG.Core.Fluent;
using NMG.Core.TextFormatter;
using NMG.Core.Util;

namespace NMG.Core.Generator
{
    public abstract class MappingGenerator : AbstractGenerator
    {
        private readonly ApplicationPreferences applicationPreferences;

        protected MappingGenerator(ApplicationPreferences applicationPreferences, Table table)
            : base(
                applicationPreferences.FolderPath, applicationPreferences.TableName, applicationPreferences.NameSpace,
                applicationPreferences.AssemblyName, applicationPreferences.Sequence, table, applicationPreferences)
        {
            this.applicationPreferences = applicationPreferences;
        }

        protected abstract void AddIdGenerator(XmlDocument xmldoc, XmlElement idElement);

        public override void Generate()
        {
            string fileName = filePath + tableName.GetFormattedText().MakeSingular() + ".hbm.xml";
            using (var stringWriter = new StringWriter())
            {
                XmlDocument xmldoc = CreateMappingDocument();
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
            XmlDeclaration xmlDeclaration = xmldoc.CreateXmlDeclaration("1.0", string.Empty, string.Empty);
            xmldoc.AppendChild(xmlDeclaration);
            XmlElement root = xmldoc.CreateElement("hibernate-mapping", "urn:nhibernate-mapping-2.2");
            root.SetAttribute("assembly", assemblyName);
            root.SetAttribute("namespace", nameSpace);
            xmldoc.AppendChild(root);

            XmlElement classElement = xmldoc.CreateElement("class");
            classElement.SetAttribute("name", tableName.GetFormattedText().MakeSingular());
            classElement.SetAttribute("table", tableName);
            classElement.SetAttribute("lazy", "true");
            root.AppendChild(classElement);
            PrimaryKey primaryKey = Table.PrimaryKey;

            if (primaryKey.Type == PrimaryKeyType.PrimaryKey)
            {
                XmlElement idElement = xmldoc.CreateElement("id");
                idElement.SetAttribute("name", primaryKey.Columns[0].Name.GetFormattedText());
                idElement.SetAttribute("column", primaryKey.Columns[0].Name);

                classElement.AppendChild(idElement);
            }
            else if (primaryKey.Type == PrimaryKeyType.CompositeKey)
            {
                XmlElement idElement = xmldoc.CreateElement("composite-id");
                foreach (Column key in primaryKey.Columns)
                {
                    XmlElement keyProperty = xmldoc.CreateElement("key-property");
                    keyProperty.SetAttribute("name", key.Name.GetFormattedText());
                    keyProperty.SetAttribute("column", key.Name);

                    idElement.AppendChild(keyProperty);

                    classElement.AppendChild(idElement);
                }
            }

            foreach (ForeignKey foreignKey in Table.ForeignKeys)
            {
                XmlElement fkProperty = xmldoc.CreateElement("many-to-one");
                fkProperty.SetAttribute("name", foreignKey.References.GetFormattedText().MakeSingular());
                fkProperty.SetAttribute("column", foreignKey.Name);

                classElement.AppendChild(fkProperty);
            }

            foreach (Column column in Table.Columns.Where(x => x.IsPrimaryKey != true && x.IsForeignKey != true))
            {
                string columnMapping = new DBColumnMapper().Map(column);
                XmlElement property = xmldoc.CreateElement("property");
                property.SetAttribute("name", column.Name.GetFormattedText());
                property.SetAttribute("column", column.Name);

                classElement.AppendChild(property);
            }

            //var primaryKeyColumn = columnDetails.Find(detail => detail.IsPrimaryKey);
            //if (primaryKeyColumn != null)
            //{
            //    var idElement = xmldoc.CreateElement("id");
            //    string propertyName = primaryKeyColumn.ColumnName.GetPreferenceFormattedText(applicationPreferences);
            //    if (applicationPreferences.FieldGenerationConvention == FieldGenerationConvention.Property)
            //    {
            //        idElement.SetAttribute("name", propertyName.MakeFirstCharLowerCase());
            //    }else
            //    {
            //        idElement.SetAttribute("name", propertyName);
            //    }
            //    var mapper = new DataTypeMapper();
            //    Type mapFromDbType = mapper.MapFromDBType(primaryKeyColumn.DataType, primaryKeyColumn.DataLength, primaryKeyColumn.DataPrecision, primaryKeyColumn.DataScale);
            //    idElement.SetAttribute("type", mapFromDbType.Name);
            //    idElement.SetAttribute("column", primaryKeyColumn.ColumnName);
            //    if (applicationPreferences.FieldGenerationConvention != FieldGenerationConvention.AutoProperty)
            //    {
            //        idElement.SetAttribute("access", "field");
            //    }
            //    classElement.AppendChild(idElement);
            //    AddIdGenerator(xmldoc, idElement);
            //}

            //AddAllProperties(xmldoc, classElement);
            return xmldoc;
        }

        private void AddAllProperties(XmlDocument xmldoc, XmlNode classElement)
        {
            //foreach (var columnDetail in columnDetails)
            //{
            //    if (columnDetail.IsPrimaryKey)
            //        continue;
            //    var xmlNode = xmldoc.CreateElement("property");
            //    string propertyName = columnDetail.ColumnName.GetPreferenceFormattedText(applicationPreferences);
            //    if (applicationPreferences.FieldGenerationConvention == FieldGenerationConvention.Property)
            //    {
            //        xmlNode.SetAttribute("name", propertyName.MakeFirstCharLowerCase());    
            //    }else
            //    {
            //        xmlNode.SetAttribute("name", propertyName);
            //    }

            //    xmlNode.SetAttribute("column", columnDetail.ColumnName);
            //    if (applicationPreferences.FieldGenerationConvention != FieldGenerationConvention.AutoProperty)
            //    {
            //        xmlNode.SetAttribute("access", "field");
            //    }
            //    classElement.AppendChild(xmlNode);
            //}
        }
    }
}