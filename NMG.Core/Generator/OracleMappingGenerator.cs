using System.Xml;
using NMG.Core.Domain;

namespace NMG.Core.Generator
{
    public class OracleMappingGenerator : MappingGenerator
    {
        public OracleMappingGenerator(ApplicationPreferences applicationPreferences, Table table)
            : base(applicationPreferences, table)
        {
        }

        protected override void AddIdGenerator(XmlDocument xmldoc, XmlElement idElement)
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