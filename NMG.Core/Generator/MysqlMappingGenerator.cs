using System.Xml;
using NMG.Core.Domain;

namespace NMG.Core.Generator
{
    public class MysqlMappingGenerator : MappingGenerator
    {
        public MysqlMappingGenerator(ApplicationPreferences applicationPreferences, Table table)
            : base(applicationPreferences, table)
        {
        }

        protected override void AddIdGenerator(XmlDocument xmldoc, XmlElement idElement)
        {
            var generatorElement = xmldoc.CreateElement("generator");
            generatorElement.SetAttribute("class", "identity");
            idElement.AppendChild(generatorElement);
        }
    }
}