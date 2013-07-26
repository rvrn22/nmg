using System.Xml;
using NMG.Core.Domain;

namespace NMG.Core.Generator
{
    /// <summary>
    /// CUBRID implementation of the MappingGenerator
    /// http://www.cubrid.org
    /// NMG support - v1.0
    /// </summary>
    public class CUBRIDMappingGenerator : MappingGenerator
    {
        public CUBRIDMappingGenerator(ApplicationPreferences applicationPreferences, Table table) : base(applicationPreferences, table)
        {
        }

        /// <summary>
        /// CUBRID supports AUTO_INCREMENT attribute as IDENTITY column
        /// </summary>
        protected override void AddIdGenerator(XmlDocument xmldoc, XmlElement idElement)
        {
            var generatorElement = xmldoc.CreateElement("generator");
            generatorElement.SetAttribute("class", "identity");
            idElement.AppendChild(generatorElement);
        }

        protected override string CleanupGeneratedFile(string generatedContent)
        {
            return generatedContent;
        }
    }
}