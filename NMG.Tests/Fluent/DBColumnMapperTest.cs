using NMG.Core.Domain;
using NMG.Core.Fluent;
using NMG.Core.TextFormatter;
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
            var column = new Column
                             {
                                 Name = "Age",
                                 DataType = "Int",
                                 IsNullable = true
                             };
            Assert.That(mapper.Map(column, new PascalCaseTextFormatter()), Is.EqualTo("Map(x => x.Age).Column(\"Age\");"));
        }

        [Test]
        public void ShouldMapDBColumnWithProperties()
        {
            var mapper = new DBColumnMapper();
            var column = new Column
                             {
                                 Name = "Name",
                                 DataLength = 16,
                                 DataType = "varchar",
                                 IsForeignKey = false,
                                 IsNullable = false,
                                 IsPrimaryKey = false,
                                 MappedDataType = "string"
                             };
            Assert.That(mapper.Map(column, new PascalCaseTextFormatter()), Is.EqualTo("Map(x => x.Name).Column(\"Name\").Not.Nullable().Length(16);"));
        }
    }
}