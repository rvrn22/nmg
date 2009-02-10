using System.Collections.Generic;
using System.Xml;

namespace NMG.Core
{
    public class OracleMappingGenerator : BaseMappingGenerator 
    {
        public OracleMappingGenerator(string path, List<string> tableName, string nameSpace, string assemblyName, string sequenceName, ColumnDetails columnDetails) : base(path, tableName, nameSpace, assemblyName, sequenceName, columnDetails)
        {

        }

        protected override void AddGenerator(XmlDocument xmldoc, XmlElement idElement)
        {
            var generatorElement = xmldoc.CreateElement("generator");
            generatorElement.SetAttribute("class", "sequence");
            idElement.AppendChild(generatorElement);

            var paramElement = xmldoc.CreateElement("param");
            paramElement.SetAttribute("name", "sequence");
            paramElement.InnerText = sequenceName;
            generatorElement.AppendChild(paramElement);
        }
    }
}