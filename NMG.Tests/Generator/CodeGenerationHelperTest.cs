using System.CodeDom;
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
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            cSharpCodeProvider = new CSharpCodeProvider();
            stringBuilder = new StringBuilder();
        }

        #endregion

        private CSharpCodeProvider cSharpCodeProvider;
        private StringBuilder stringBuilder;

        [Test]
        public void ShouldGenerateAutoProperty()
        {
            var codeGenerationHelper = new CodeGenerationHelper();
            CodeMemberProperty autoProperty = codeGenerationHelper.CreateAutoProperty(typeof (string), "Name", false);
            CodeCompileUnit codeCompileUnit = codeGenerationHelper.GetCodeCompileUnit("someNamespace", "someType");
            codeCompileUnit.Namespaces[0].Types[0].Members.Add(autoProperty);
            cSharpCodeProvider.GenerateCodeFromCompileUnit(codeCompileUnit, new StringWriter(stringBuilder),
                                                           new CodeGeneratorOptions());
            StringBuilder builderCompare = new StringBuilder();
            builderCompare.AppendLine("public virtual string Name {");
            builderCompare.AppendLine("            get {");
            builderCompare.AppendLine("            }");
            builderCompare.AppendLine("            set {");
            builderCompare.AppendLine("            }");
            builderCompare.Append("        }");
            Assert.IsTrue(stringBuilder.ToString().Contains(builderCompare.ToString()));    
        }

        [Test]
        public void ShouldGenerateField()
        {
            var codeGenerationHelper = new CodeGenerationHelper();
            CodeMemberField codeMemberField = codeGenerationHelper.CreateField(typeof (string), "name");
            CodeCompileUnit codeCompileUnit = codeGenerationHelper.GetCodeCompileUnit("someNamespace", "someType");
            codeCompileUnit.Namespaces[0].Types[0].Members.Add(codeMemberField);
            cSharpCodeProvider.GenerateCodeFromCompileUnit(codeCompileUnit, new StringWriter(stringBuilder),
                                                           new CodeGeneratorOptions());
            Assert.IsTrue(stringBuilder.ToString().Contains("private string name;"));
        }

        [Test]
        public void ShouldGenerateNullableAutoProperty()
        {
            var codeGenerationHelper = new CodeGenerationHelper();
            CodeMemberProperty autoProperty = codeGenerationHelper.CreateAutoProperty(typeof (int), "Name", true);
            CodeCompileUnit codeCompileUnit = codeGenerationHelper.GetCodeCompileUnit("someNamespace", "someType");
            codeCompileUnit.Namespaces[0].Types[0].Members.Add(autoProperty);
            cSharpCodeProvider.GenerateCodeFromCompileUnit(codeCompileUnit, new StringWriter(stringBuilder),
                                                           new CodeGeneratorOptions());
            
            StringBuilder builderCompare = new StringBuilder();
            builderCompare.AppendLine("public virtual System.Nullable<int> Name {");
            builderCompare.AppendLine("            get {");
            builderCompare.AppendLine("            }");
            builderCompare.AppendLine("            set {");
            builderCompare.AppendLine("            }");
            builderCompare.Append("        }");
            Assert.IsTrue(stringBuilder.ToString().Contains(builderCompare.ToString()), "Was: " + stringBuilder);
        }

        [Test]
        public void ShouldGenerateProperty()
        {
            var codeGenerationHelper = new CodeGenerationHelper();
            CodeMemberProperty memberProperty = codeGenerationHelper.CreateProperty(typeof (string), "Name");
            CodeCompileUnit codeCompileUnit = codeGenerationHelper.GetCodeCompileUnit("someNamespace", "someType");
            codeCompileUnit.Namespaces[0].Types[0].Members.Add(memberProperty);
            cSharpCodeProvider.GenerateCodeFromCompileUnit(codeCompileUnit, new StringWriter(stringBuilder),
                                                           new CodeGeneratorOptions());
            
            StringBuilder builderCompare = new StringBuilder();
            builderCompare.AppendLine("public virtual string Name {");
            builderCompare.AppendLine("            get {");
            builderCompare.AppendLine("                return this.name;");
            builderCompare.AppendLine("            }");
            builderCompare.AppendLine("            set {");
            builderCompare.AppendLine("                this.name = value;");
            builderCompare.AppendLine("            }");
            builderCompare.Append("        }");
            Assert.IsTrue(
                stringBuilder.ToString().Contains(builderCompare.ToString()));
        }

        [Test]
        public void ShouldGeneratePropertyWithNonNullableString()
        {
            var codeGenerationHelper = new CodeGenerationHelper();
            CodeMemberProperty memberProperty = codeGenerationHelper.CreateAutoProperty(typeof (string), "Name", true);
            CodeCompileUnit codeCompileUnit = codeGenerationHelper.GetCodeCompileUnit("someNamespace", "someType");
            codeCompileUnit.Namespaces[0].Types[0].Members.Add(memberProperty);
            cSharpCodeProvider.GenerateCodeFromCompileUnit(codeCompileUnit, new StringWriter(stringBuilder),
                                                           new CodeGeneratorOptions());
            StringBuilder builderCompare = new StringBuilder();
            builderCompare.AppendLine("public virtual string Name {");
            builderCompare.AppendLine("            get {");
            builderCompare.AppendLine("            }");
            builderCompare.AppendLine("            set {");
            builderCompare.AppendLine("            }");
            builderCompare.Append("        }");
            Assert.IsTrue(stringBuilder.ToString().Contains(builderCompare.ToString()), "Was: " + stringBuilder);
        }
    }
}