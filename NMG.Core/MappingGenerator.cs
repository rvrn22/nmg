using System.IO;
using System.Xml;

namespace NMG.Core
{
    public class MappingGenerator
    {
        private readonly string path;
        private readonly string tableName;
        private readonly string nameSpace;
        private readonly string assemblyName;
        private readonly string sequenceNumber;
        private readonly ColumnDetails columnDetails;

        public MappingGenerator(string path, string tableName, string nameSpace, string assemblyName, string sequenceNumber, ColumnDetails columnDetails)
        {
            this.path = path;
            this.tableName = tableName;
            this.nameSpace = nameSpace;
            this.assemblyName = assemblyName;
            this.sequenceNumber = sequenceNumber;
            this.columnDetails = columnDetails;
        }

        public void GenerateMappingFile()
        {
            string fileName = path + tableName.GetFormattedText() + ".hbm.xml";
            var fs = new FileStream(fileName, FileMode.Create);
            var streamReader = new StreamReader("NHibernateTemplate.xml");

            using (fs)
            {
                using (streamReader)
                {
                    string text = streamReader.ReadToEnd();
                    text = text.Replace("@ClassName@", tableName.GetFormattedText());
                    text = text.Replace("@TableName@", tableName);
                    text = text.Replace("@AssemblyName@", assemblyName);
                    text = text.Replace("@NameSpace@", nameSpace);
                    text = text.Replace("@SequenceName@", sequenceNumber);

                    var xmldoc = new XmlDocument();
                    xmldoc.Load(new StringReader(text));

                    foreach (var columnDetail in columnDetails)
                    {
                        if (xmldoc.DocumentElement == null) continue;
                        XmlNode classNode = xmldoc.DocumentElement.FirstChild;
                        XmlNode xmlNode = xmldoc.CreateNode(XmlNodeType.Element, null, "property", null);
                        xmldoc.DocumentElement.SetAttribute("xmlns", "");
                        XmlAttribute nameAttr = xmldoc.CreateAttribute("name");
                        string propertyName = columnDetail.ColumnName.GetFormattedText();
                        nameAttr.Value = propertyName.MakeFirstCharLowerCase();
                        XmlAttribute colAttr = xmldoc.CreateAttribute("column");
                        colAttr.Value = columnDetail.ColumnName;
                        XmlAttribute accessAttr = xmldoc.CreateAttribute("access");
                        accessAttr.Value = "field";
                        xmlNode.Attributes.Append(nameAttr);
                        xmlNode.Attributes.Append(colAttr);
                        xmlNode.Attributes.Append(accessAttr);
                        classNode.AppendChild(xmlNode);
                    }
                    xmldoc.Save(fs);
                }
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

        

        public void GenerateCodeFile()
        {
            var generator = new CodeGenerator(path, nameSpace, tableName.GetFormattedText(), columnDetails);
            generator.Generate();
        }
    }
}