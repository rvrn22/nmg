using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace NMG.Core
{
    public abstract class BaseMappingGenerator : BaseGenerator
    {
        protected BaseMappingGenerator(string path, List<string> tableNames, string nameSpace, string assemblyName, string sequenceName, ColumnDetails columnDetails)
            : base(path, tableNames, nameSpace, assemblyName, sequenceName, columnDetails)
        {
        }

        public override void Generate()
        {
            foreach (var tableName in tableNames)
            {
                string fileName = filePath + tableName.GetFormattedText() + ".hbm.xml";
                var fs = new FileStream(fileName, FileMode.Create);

                using (fs)
                {
                    var xmldoc = new XmlDocument();
                    var xmlDeclaration = xmldoc.CreateXmlDeclaration("1.0", "utf-8", "");
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
                    var idElement = xmldoc.CreateElement("id");
                    idElement.SetAttribute("name", "id");
                    var mapper = new DataTypeMapper();
                    idElement.SetAttribute("type", mapper.MapFromDBType(primaryKeyColumn.DataType).Name);
                    idElement.SetAttribute("column", primaryKeyColumn.ColumnName);
                    idElement.SetAttribute("access", "field");
                    classElement.AppendChild(idElement);

                    AddGenerator(xmldoc, idElement);


                    AddAllProperties(xmldoc, classElement);
                    xmldoc.Save(fs);
                }

                var sr = new StreamReader(fileName);
                string generatedXML;
                using (sr)
                {
                    generatedXML = sr.ReadToEnd();
                    generatedXML = generatedXML.Replace("xmlns=\"\"", "");
                }

                var writer = new StreamWriter(fileName);
                using (writer)
                {
                    writer.Write(generatedXML);
                    writer.Flush();
                }
            }

        }

        protected abstract void AddGenerator(XmlDocument xmldoc, XmlElement idElement);

        private void AddAllProperties(XmlDocument xmldoc, XmlNode classElement)
        {
            foreach (var columnDetail in columnDetails)
            {
                if(columnDetail.IsPrimaryKey)
                    continue;
                var xmlNode = xmldoc.CreateElement("property");
                xmlNode.SetAttribute("name", columnDetail.ColumnName.GetFormattedText().MakeFirstCharLowerCase());
                xmlNode.SetAttribute("column", columnDetail.ColumnName);
                xmlNode.SetAttribute("access", "field");
                classElement.AppendChild(xmlNode);
            }
        }
    }
}