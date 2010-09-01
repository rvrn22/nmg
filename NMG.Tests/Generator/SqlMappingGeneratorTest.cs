using System.Collections.Generic;
using NMG.Core;
using NMG.Core.Domain;
using NMG.Core.Generator;
using NUnit.Framework;

namespace NMG.Tests.Generator
{
    [TestFixture]
    public class SqlMappingGeneratorTest
    {
        [Test]
        public void ShouldGenerateMappingForSqlServerTable()
        {
            const string generatedXML = "<?xml version=\"1.0\"?><hibernate-mapping assembly=\"myAssemblyName\" namespace=\"myNameSpace\" xmlns=\"urn:nhibernate-mapping-2.2\"><class name=\"Customer\" table=\"Customer\" lazy=\"true\" xmlns=\"\"><id name=\"Id\" column=\"Id\" /></class></hibernate-mapping>";
            var preferences = new ApplicationPreferences
                                  {
                                      FolderPath = "\\",
                                      TableName = "Customer",
                                      AssemblyName = "myAssemblyName",
                                      NameSpace = "myNameSpace",
                                      Sequence = "mySequenceNumber",
                                  };
            var primaryKey = new PrimaryKey { Columns = new List<Column> { new Column { Name = "Id" } } };
            var generator = new SqlMappingGenerator(preferences, new Table { PrimaryKey = primaryKey });
            var document = generator.CreateMappingDocument();
            Assert.AreEqual(generatedXML, document.InnerXml);
        }
    }
}