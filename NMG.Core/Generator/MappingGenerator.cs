using System.IO;
using System.Xml;
using NMG.Core.Domain;

namespace NMG.Core.Generator
{
    public abstract class MappingGenerator : AbstractGenerator
    {
        protected MappingGenerator(ApplicationPreferences applicationPreferences, Table table) : base(applicationPreferences.FolderPath, "Mapping", applicationPreferences.TableName, applicationPreferences.NameSpace, applicationPreferences.AssemblyName, applicationPreferences.Sequence, table, applicationPreferences)
        {
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


            if (primaryKey.Type == PrimaryKeyType.CompositeKey)
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


            foreach (Column column in Table.Columns)
            {
                XmlElement property = null;
                XmlElement property2;

                if (column.IsForeignKey)
                {
                    property = xmldoc.CreateElement("many-to-one");
                    property.SetAttribute("insert", "false");
                    property.SetAttribute("update", "false");
                    property.SetAttribute("lazy", "false");
                    property2 = xmldoc.CreateElement("property");
                }
                else if (column.IsPrimaryKey)
                {
                    property2 = xmldoc.CreateElement("id");
                    XmlElement generatorElement = xmldoc.CreateElement("generator");
                    generatorElement.SetAttribute("class", "identity");
                    property2.AppendChild(generatorElement);
                }
                else
                {
                    property2 = xmldoc.CreateElement("property");
                }


                if (property != null)
                    property.SetAttribute("name", Formatter.FormatText(column.Name));
                property2.SetAttribute("name", Formatter.FormatText(column.Name));
                XmlElement columnProperty = xmldoc.CreateElement("column");
                if (property != null)
                    property.AppendChild(columnProperty);

                columnProperty.SetAttribute("name", column.Name);
                columnProperty.SetAttribute("sql-type", column.DataType);
                columnProperty.SetAttribute("not-null", (!column.IsNullable).ToString().ToLower());
                property2.AppendChild(columnProperty.Clone());
                if (property != null)
                    classElement.AppendChild(property);
                classElement.AppendChild(property2);
            }

            foreach (var hasMany in Table.HasManyRelationships)
            {
                XmlElement bagElement = xmldoc.CreateElement("bag");

                bagElement.SetAttribute("name", Formatter.FormatPlural(hasMany.Reference));
                bagElement.SetAttribute("inverse", "true");
                bagElement.SetAttribute("cascade", "none");

                classElement.AppendChild(bagElement);

                XmlElement keyElement = xmldoc.CreateElement("key");

                keyElement.SetAttribute("column", hasMany.ReferenceColumn);

                bagElement.AppendChild(keyElement);

                XmlElement oneToManyElement = xmldoc.CreateElement("one-to-many");

                oneToManyElement.SetAttribute("class", Formatter.FormatSingular(hasMany.Reference));

                bagElement.AppendChild(oneToManyElement);
            }

            return xmldoc;
        }
    }
}