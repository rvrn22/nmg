using System.Linq;
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

        public override void Generate(bool writeToFile = true)
        {
            string fileName = filePath + Formatter.FormatSingular(tableName) + ".hbm.xml";
            using (var stringWriter = new StringWriter())
            {
                XmlDocument xmldoc = CreateMappingDocument();
                xmldoc.Save(stringWriter);
                string generatedXML = RemoveEmptyNamespaces(stringWriter.ToString());

                GeneratedCode = generatedXML;
                if (writeToFile)
                {
                    using (var writer = new StreamWriter(fileName))
                    {
                        writer.Write(generatedXML);
                        writer.Flush();
                    }
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


            if (primaryKey != null)
            {
                if (primaryKey.Type == PrimaryKeyType.CompositeKey)
                {
                    XmlElement idElement = xmldoc.CreateElement("composite-id");
                    foreach (Column key in primaryKey.Columns)
                    {
                        XmlElement keyProperty;
                        if (key.IsForeignKey && applicationPreferences.IncludeForeignKeys)
                        {
                            keyProperty = xmldoc.CreateElement("key-many-to-one");
                            keyProperty.SetAttribute("name", Formatter.FormatSingular(key.ForeignKeyTableName));
                            keyProperty.SetAttribute("column", key.Name);
                        } else
                        {
                            keyProperty = xmldoc.CreateElement("key-property");
                            keyProperty.SetAttribute("name", Formatter.FormatText(key.Name));
                            keyProperty.SetAttribute("column", key.Name);
                        }
                        idElement.AppendChild(keyProperty);
                        classElement.AppendChild(idElement);
                    }
                } else
                {
                    XmlElement keyProperty = xmldoc.CreateElement("id");
                    Column primaryKeyColum = primaryKey.Columns.Single();
                    keyProperty.SetAttribute("name", Formatter.FormatText(primaryKeyColum.Name));
                    keyProperty.SetAttribute("column", primaryKeyColum.Name); //If ID Column is attribute.
                    if (primaryKeyColum.IsIdentity)
                    {
                        XmlElement generatorElement = xmldoc.CreateElement("generator");
                        generatorElement.SetAttribute("class", "identity");
                        keyProperty.AppendChild(generatorElement);
                    }
                    classElement.AppendChild(keyProperty);
                }
            }

            foreach (Column column in Table.Columns.Where(c => !c.IsPrimaryKey).OrderByDescending(c => c.IsForeignKey))
            {
                XmlElement element = column.IsForeignKey && applicationPreferences.IncludeForeignKeys ? xmldoc.CreateElement("many-to-one") 
                                                                                                      : xmldoc.CreateElement("property");
                if (column.IsForeignKey && applicationPreferences.IncludeForeignKeys && applicationPreferences.NameFkAsForeignTable)
                    element.SetAttribute("name", Formatter.FormatSingular(column.ForeignKeyTableName));
                else
                    element.SetAttribute("name", Formatter.FormatText(column.Name));

                XmlElement columnElement = xmldoc.CreateElement("column");
                columnElement.SetAttribute("name", column.Name);
                columnElement.SetAttribute("sql-type", column.DataType);
                columnElement.SetAttribute("not-null", (column.IsNullable ? "false" : "true"));
                if (column.IsUnique)
                {
                    columnElement.SetAttribute("unique", "true");
                }
                element.AppendChild(columnElement);
                classElement.AppendChild(element);
            }


            foreach (var hasMany in Table.HasManyRelationships)
            {
                XmlElement bagElement = applicationPreferences.ForeignEntityCollectionType.Contains("Set") ? xmldoc.CreateElement("set") : xmldoc.CreateElement("bag");
                bagElement.SetAttribute("name", Formatter.FormatPlural(hasMany.Reference));
                if (applicationPreferences.IncludeHasMany)
                    bagElement.SetAttribute("inverse", "true");
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