using System.CodeDom.Compiler;
using System.IO;
using System.Text;
using Microsoft.CSharp;
using NMG.Core.Generator;
using NUnit.Framework;

namespace NMG.Tests.Generator
{
    [TestFixture]
    public class CodeGenerationHelperTest
    {
        private CSharpCodeProvider cSharpCodeProvider;
        private StringBuilder stringBuilder;

        [SetUp]
        public void SetUp()
        {
            cSharpCodeProvider = new CSharpCodeProvider();
            stringBuilder = new StringBuilder();
        }

        [Test]
        public void ShouldGenerateField()
        {
            var codeGenerationHelper = new CodeGenerationHelper();
            var codeMemberField = codeGenerationHelper.CreateField(typeof (string), "name");
            var codeCompileUnit = codeGenerationHelper.GetCodeCompileUnit("someNamespace", "someType");
            codeCompileUnit.Namespaces[0].Types[0].Members.Add(codeMemberField);
            cSharpCodeProvider.GenerateCodeFromCompileUnit(codeCompileUnit, new StringWriter(stringBuilder), new CodeGeneratorOptions());
            Assert.IsTrue(stringBuilder.ToString().Contains("private string name;"));
        }

        [Test]
        public void ShouldGenerateProperty()
        {
            var codeGenerationHelper = new CodeGenerationHelper();
            var memberProperty = codeGenerationHelper.CreateProperty(typeof(string), "Name");
            var codeCompileUnit = codeGenerationHelper.GetCodeCompileUnit("someNamespace", "someType");
            codeCompileUnit.Namespaces[0].Types[0].Members.Add(memberProperty);
            cSharpCodeProvider.GenerateCodeFromCompileUnit(codeCompileUnit, new StringWriter(stringBuilder), new CodeGeneratorOptions());
            Assert.IsTrue(stringBuilder.ToString().Contains(@"public virtual string Name {
            get {
                return this.name;
            }
            set {
                this.name = value;
            }
        }"));
        }

        [Test]
        public void ShouldGenerateAutoProperty()
        {
            var codeGenerationHelper = new CodeGenerationHelper();
            var autoProperty = codeGenerationHelper.CreateAutoProperty(typeof(string), "Name");
            var codeCompileUnit = codeGenerationHelper.GetCodeCompileUnit("someNamespace", "someType");
            codeCompileUnit.Namespaces[0].Types[0].Members.Add(autoProperty);
            cSharpCodeProvider.GenerateCodeFromCompileUnit(codeCompileUnit, new StringWriter(stringBuilder), new CodeGeneratorOptions());
            Assert.IsTrue(stringBuilder.ToString().Contains(@"public virtual string Name {
            get {
            }
            set {
            }
        }"));
        }
    }
}