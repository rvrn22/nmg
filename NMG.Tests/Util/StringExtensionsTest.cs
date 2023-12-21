using NMG.Core.Util;
using NUnit.Framework;

namespace NMG.Tests.Util
{
    [TestFixture]
    public class StringExtensionsTest
    {
        [Test]
        public void StringLiteral()
        {
            Assert.AreEqual("\"`Snow Ball`\"", "Snow Ball".ToStringLiteral());
        }

        [Test]
        public void FirstCharAsLower()
        {
            const string name = "HitMan";
            Assert.AreEqual("hitMan", name.MakeFirstCharLowerCase());
        }

        [Test]
        public void FirstCharAsUpper()
        {
            const string name = "hitMan";
            Assert.AreEqual("HitMan", name.MakeFirstCharUpperCase());
        }

        [Test]
        public void FormattedString()
        {
            const string name = "the_name_is_BOND_james_bond";
            Assert.AreEqual("TheNameIsBondJamesBond", name.GetFormattedText());
        }

        [Test]
        public void TitleCase()
        {
            const string name = "the name is bond. james bond";
            Assert.AreEqual("The Name Is Bond. James Bond", name.MakeTitleCase());
        }
    }
}