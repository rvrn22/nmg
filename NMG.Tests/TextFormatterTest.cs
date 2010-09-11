using NUnit.Framework;

using NMG.Core.TextFormatter;

namespace NMG.Tests
{
    [TestFixture]
    public class TextFormatterTest
    {
        [Test]
        public void LeaveStringUnformatted()
        {
            var formatter = new UnformattedTextFormatter();
            const string columnName = "Column_Name";

            Assert.AreEqual(columnName, formatter.FormatText(columnName));
        }

        [Test]
        public void ConvertStringToCamelCase()
        {
            var formatter = new CamelCaseTextFormatter();

            Assert.AreEqual("columnName", formatter.FormatText("Column_Name"));
            Assert.AreEqual("columnName", formatter.FormatText("COLUMN_NAME"));
        }

        [Test] public void ConvertStringToPascalCase()
        {
            var formatter = new PascalCaseTextFormatter();

            Assert.AreEqual("ColumnName", formatter.FormatText("column_name"));
        }

        [Test]
        public void ConvertStringWithPrefix()
        {
            var formatter = new PrefixedTextFormatter("_");

            Assert.AreEqual("_ColumnName", formatter.FormatText("ColumnName"));
            Assert.AreEqual("_column_name", formatter.FormatText("column_name"));
        }
    }
}