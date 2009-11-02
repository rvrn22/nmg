using NMG.Core;
using NMG.Core.Domain;
using NUnit.Framework;

namespace NMG.Tests
{
    [TestFixture]
    public class MappingControllerTest
    {
        [Test]
        public void ShouldGenerateOneFile()
        {
            var mappingController = new MappingController(ServerType.Oracle, "folderPath", "tableName", "nameSpace", "assemblyName", "sequence", new ColumnDetails());
            mappingController.Generate(Language.CSharp, new Preferences());
        }
    }
}
