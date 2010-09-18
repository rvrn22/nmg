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
            string fileName = filePath + Formatter.FormatSingular(tableName) + ".hbm.xml";
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
            classElement.SetAttribute("name", Formatter.FormatSingular(tableName));
            classElement.SetAttribute("table", tableName);
            classElement.SetAttribute("lazy", "true");
            root.AppendChild(classElement);
            PrimaryKey primaryKey = Table.PrimaryKey;

            if(UsesSequence)
            {
                XmlElement idElement = xmldoc.CreateElement("id");

                XmlElement generatorElement = xmldoc.CreateElement("generator");
                generatorElement.SetAttribute("class", "sequence");
                XmlElement paramElement = xmldoc.CreateElement("param");
                generatorElement.AppendChild(paramElement);
                paramElement.SetAttribute("name", "sequence");
                paramElement.InnerText = sequenceName;

                idElement.AppendChild(generatorElement);
                classElement.AppendChild(idElement);
            }
            else if (primaryKey.Type == PrimaryKeyType.PrimaryKey)
            {
                XmlElement idElement = xmldoc.CreateElement("id");
                idElement.SetAttribute("name", Formatter.FormatText(primaryKey.Columns[0].Name));
                idElement.SetAttribute("column", primaryKey.Columns[0].Name);

                classElement.AppendChild(idElement);
            }
            else if (primaryKey.Type == PrimaryKeyType.CompositeKey)
            {
                XmlElement idElement = xmldoc.CreateElement("composite-id");
                foreach (Column key in primaryKey.Columns)
                {
                    XmlElement keyProperty = xmldoc.CreateElement("key-property");
                    keyProperty.SetAttribute("name", Formatter.FormatText(key.Name));
                    keyProperty.SetAttribute("column", key.Name);

                    idElement.AppendChild(keyProperty);

                    classElement.AppendChild(idElement);
                }
            }

            foreach (ForeignKey foreignKey in Table.ForeignKeys)
            {
                XmlElement fkProperty = xmldoc.CreateElement("many-to-one");
                fkProperty.SetAttribute("name", Formatter.FormatSingular(foreignKey.References));
                fkProperty.SetAttribute("column", foreignKey.Name);

                classElement.AppendChild(fkProperty);
            }

            foreach (Column column in Table.Columns.Where(x => x.IsPrimaryKey != true && x.IsForeignKey != true))
            {
                XmlElement property = xmldoc.CreateElement("property");
                property.SetAttribute("name", Formatter.FormatText(column.Name));
                property.SetAttribute("column", column.Name);

                classElement.AppendChild(property);
            }

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