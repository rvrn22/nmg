using NMG.Core;
using NMG.Core.Domain;
using NMG.Core.Generator;
using NUnit.Framework;

namespace NMG.Tests
{
    [TestFixture]
    public class SqlMappingGeneratorTest
    {
        [Test]
        public void ShouldGenerateMappingForSqlServerTable()
        {
            const string generatedXML = "<?xml version=\"1.0\" encoding=\"utf-8\"?><hibernate-mapping assembly=\"myAssemblyName\" xmlns=\"urn:nhibernate-mapping-2.2\"><class name=\"myNameSpace.Customer, myAssemblyName\" table=\"Customer\" lazy=\"true\" xmlns=\"\" /></hibernate-mapping>";
            var generator = new SqlMappingGenerator("\\", "Customer", "myNameSpace", "myAssemblyName", new ColumnDetails(), new Preferences());
            var document = generator.CreateMappingDocument();
            Assert.AreEqual(generatedXML, document.InnerXml);
        }
    }
}