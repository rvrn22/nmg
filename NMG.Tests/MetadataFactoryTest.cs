using NMG.Core;
using NMG.Core.Domain;
using NMG.Core.Reader;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace NMG.Tests
{
    [TestFixture]
    public class MetadataFactoryTest
    {
        [Test]
        public void ShouldCreateTheAppropriateMetadataReader()
        {
            var metadataReader = MetadataFactory.GetReader(ServerType.Oracle, "conn");
            Assert.That(metadataReader, Is.TypeOf(typeof(OracleMetadataReader)));
            
            metadataReader = MetadataFactory.GetReader(ServerType.SqlServer, "conn");
            Assert.That(metadataReader, Is.TypeOf(typeof(SqlServerMetadataReader)));
        }
    }
}