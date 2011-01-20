using System.Collections.Generic;
using System.Xml;
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
            const string generatedXML =
                "<?xml version=\"1.0\"?><hibernate-mapping assembly=\"myAssemblyName\" namespace=\"myNameSpace\" xmlns=\"urn:nhibernate-mapping-2.2\"><class name=\"Customer\" table=\"Customer\" lazy=\"true\" xmlns=\"\"><id name=\"Id\"><generator class=\"identity\" /><column name=\"Id\" sql-type=\"Int\" not-null=\"true\" /></id></class></hibernate-mapping>";
            var preferences = new ApplicationPreferences
                                  {
                                      FolderPath = "\\",
                                      TableName = "Customer",
                                      AssemblyName = "myAssemblyName",
                                      NameSpace = "myNameSpace",
                                      Sequence = "mySequenceNumber",
                                  };
            var pkColumn = new Column { Name = "Id", IsPrimaryKey = true, DataType = "Int" };
            var columns = new List<Column> {pkColumn};
            var primaryKey = new PrimaryKey {Columns = columns};
            var generator = new SqlMappingGenerator(preferences, new Table {PrimaryKey = primaryKey, Columns = columns});
            XmlDocument document = generator.CreateMappingDocument();
            Assert.AreEqual(generatedXML, document.InnerXml);
        }
    }
}