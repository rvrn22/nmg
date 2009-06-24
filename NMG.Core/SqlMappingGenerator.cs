using System.Xml;
using NMG.Core.Domain;

namespace NMG.Core
{
    public class SqlMappingGenerator : MappingGenerator
    {
        public SqlMappingGenerator(string path, string tableName, string nameSpace, string assemblyName, ColumnDetails columnDetails,
                                   Preferences preferences) : base(path, tableName, nameSpace, assemblyName, string.Empty, columnDetails, preferences)
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