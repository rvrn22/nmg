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
        public void UnformattedSingular()
        {
            var formatter = new UnformattedTextFormatter();
            const string columnName = "Column_Names";

            Assert.AreEqual("Column_Name", formatter.FormatSingular(columnName));
        }

        [Test]
        public void UnformattedPlural()
        {
            var formatter = new UnformattedTextFormatter();
            const string columnName = "Column_Name";

            Assert.AreEqual("Column_Names", formatter.FormatPlural(columnName));
        }

        [Test]
        public void CamelCaseSingular()
        {
            var formatter = new CamelCaseTextFormatter();

            Assert.AreEqual("columnName", formatter.FormatSingular("Column_Name"));
            Assert.AreEqual("columnName", formatter.FormatSingular("COLUMN_NAME"));
        }

        [Test]
        public void CamelCasePlural()
        {
            var formatter = new CamelCaseTextFormatter();
            Inflector.EnableInflection = true;
            Assert.AreEqual("columnNames", formatter.FormatPlural("Column_Name"));
            Assert.AreEqual("columnNames", formatter.FormatPlural("COLUMN_NAME"));
        }

        [Test]
        public void PascalCaseSingular()
        {
            var formatter = new PascalCaseTextFormatter();
            Inflector.EnableInflection = true;
            Assert.AreEqual("ColumnName", formatter.FormatSingular("Column_Names"));
            Assert.AreEqual("ColumnName", formatter.FormatSingular("COLUMN_NAMES"));
        }

        [Test]
        public void PascalCasePlural()
        {
            var formatter = new PascalCaseTextFormatter();
            Inflector.EnableInflection = true;
            Assert.AreEqual("ColumnNames", formatter.FormatPlural("Column_Name"));
            Assert.AreEqual("ColumnNames", formatter.FormatPlural("COLUMN_NAME"));
        }

        [Test]
        public void ConvertStringToCamelCase()
        {
            var formatter = new CamelCaseTextFormatter();

            Assert.AreEqual("columnName", formatter.FormatText("Column_Name"));
            Assert.AreEqual("columnName", formatter.FormatText("COLUMN_NAME"));
            Assert.AreEqual("hitMan", formatter.FormatText("Hit_Man"));
            Assert.AreEqual("hitman", formatter.FormatText("Hit Man"));
            Assert.AreEqual("hitman", formatter.FormatText("HitMan"));
        }

        [Test] public void ConvertStringToPascalCase()
        {
            var formatter = new PascalCaseTextFormatter();

            Assert.AreEqual("ColumnName", formatter.FormatText("column_name"));
            Assert.AreEqual("TheNameIsBondJamesBond", formatter.FormatText("the_name_is_BOND_james_bond"));
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