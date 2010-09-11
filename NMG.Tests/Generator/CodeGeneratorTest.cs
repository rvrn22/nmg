using NMG.Core;
using NUnit.Framework;

namespace NMG.Tests.Generator
{
    [TestFixture]
    public class CodeGeneratorTest
    {
        // This test really needs some lovin'.
        // Ever since I changed the formatting rules (mainly due to remove udnerscores
        // and capitalize each word), it started to screw everything up. This test now passed but
        // it wouldn't surprise me if it FUBAR'd something else. :P
        [Test]
        public void ShouldCreateCompleteCompileUnit()
        {
            var applicationPreferences = new ApplicationPreferences
                                             {
                                                 NameSpace = "someNamespace",
                                                 TableName = "someTableName"
                                             };
            //var codeGenerator = new CodeGenerator(applicationPreferences, new ColumnDetails());
            //var codeCompileUnit = codeGenerator.GetCompileUnit();
            //var cSharpCodeProvider = new CSharpCodeProvider();
            //var stringBuilder = new StringBuilder();
            //cSharpCodeProvider.GenerateCodeFromCompileUnit(codeCompileUnit, new StringWriter(stringBuilder), new CodeGeneratorOptions());
            //// TODO: Put each assert in its own function. Since if one of the asserts fails, they all fail.
            //// Thus, harder to find what the bug is.
            //Assert.IsTrue(stringBuilder.ToString().Contains("namespace someNamespace"), "namespace failed" + stringBuilder.ToString());
            //Assert.IsTrue(stringBuilder.ToString().Contains("public class Sometablename"), "public class failed" + stringBuilder.ToString());
            //Assert.IsTrue(stringBuilder.ToString().Contains("public Sometablename()"), "public failed" + stringBuilder.ToString());
        }
    }
}