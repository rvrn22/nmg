using System.Collections.Generic;
using System.Xml;
using NMG.Core;
using NMG.Core.Domain;
using NMG.Core.Generator;
using NUnit.Framework;

namespace NMG.Tests.Generator
{
    [TestFixture]
    public class OracleMappingGeneratorTest
    {
        [Test]
        public void ShouldGenerateMappingForOracleTable()
        {
            const string generatedXML =
                "<?xml version=\"1.0\"?><hibernate-mapping assembly=\"myAssemblyName\" namespace=\"myNameSpace\" xmlns=\"urn:nhibernate-mapping-2.2\"><class name=\"Customer\" table=\"Customer\" lazy=\"true\" xmlns=\"\"><id name=\"Id\" column=\"Id\" /></class></hibernate-mapping>";
            var preferences = new ApplicationPreferences
                                  {
                                      FolderPath = "\\",
                                      TableName = "Customer",
                                      AssemblyName = "myAssemblyName",
                                      NameSpace = "myNameSpace",
                                      Sequence = "mySequenceNumber",
                                  };
            var primaryKey = new PrimaryKey {Columns = new List<Column> {new Column {Name = "Id"}}};
            var generator = new OracleMappingGenerator(preferences, new Table {PrimaryKey = primaryKey});
            XmlDocument document = generator.CreateMappingDocument();
            Assert.AreEqual(generatedXML, document.InnerXml);
        }
    }
}