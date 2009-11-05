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
            const string generatedXML = "<?xml version=\"1.0\"?><hibernate-mapping assembly=\"myAssemblyName\" xmlns=\"urn:nhibernate-mapping-2.2\"><class name=\"myNameSpace.Customer, myAssemblyName\" table=\"Customer\" lazy=\"true\" xmlns=\"\" /></hibernate-mapping>";
            var preferences = new ApplicationPreferences
                                  {
                                      FolderPath = "\\",
                                      TableName = "Customer",
                                      AssemblyName = "myAssemblyName",
                                      NameSpace = "myNameSpace",
                                      Sequence = "mySequenceNumber",
                                  };
            var generator = new OracleMappingGenerator(preferences, new ColumnDetails());
            var document = generator.CreateMappingDocument();
            Assert.AreEqual(generatedXML, document.InnerXml);
        }
    }
}