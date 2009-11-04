using System.Xml;
using NMG.Core.Domain;

namespace NMG.Core.Generator
{
    public class SqlMappingGenerator : MappingGenerator
    {
        public SqlMappingGenerator(ApplicationPreferences applicationPreferences, ColumnDetails columnDetails) : base(applicationPreferences, columnDetails)
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