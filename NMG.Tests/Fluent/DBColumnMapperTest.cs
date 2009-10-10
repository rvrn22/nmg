using NMG.Core.Domain;
using NMG.Core.Fluent;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace NMG.Tests.Fluent
{
    [TestFixture]
    public class DBColumnMapperTest
    {
        [Test]
        public void ShouldMapDBColumn()
        {
            var mapper = new DBColumnMapper();
            var columnDetail = new ColumnDetail("Age", "Int", 0, 0, 0, true);
            Assert.That(mapper.Map(columnDetail), Is.EqualTo("Map(x => x.Age);"));
        }
        
        [Test]
        public void ShouldMapDBColumnWithProperties()
        {
            var mapper = new DBColumnMapper();
            var columnDetail = new ColumnDetail("Name", "varchar", 16, 0, 0, false);
            Assert.That(mapper.Map(columnDetail), Is.EqualTo("Map(x => x.Name).Length(16).Not.Nullable();"));
        }
    }
}