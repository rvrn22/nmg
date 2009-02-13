using System.Collections.Generic;
using NMG.Core;
using NUnit.Framework;

namespace NMG.Tests
{
    [TestFixture]
    public class OracleMappingGeneratorTest
    {
        [Test]
        public void ShouldGenerateMappingForOracleTable()
        {
            var generator = new OracleMappingGenerator("\\", new List<string>(), "myNameSpace", "myAssemblyName", "mySequenceName",new ColumnDetails());
            generator.Generate();
        }
    }
}
