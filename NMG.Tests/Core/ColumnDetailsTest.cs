using System;
using NMG.Core;
using NMG.Core.Domain;
using NUnit.Framework;

namespace NMG.Tests.Core
{
    [TestFixture]
    public class ColumnDetailsTest
    {
        [Test]
        public void ShouldMapDBTypesToDotNetTypes()
        {
            var columnDetail = new ColumnDetail("Id", "DATE", 10, 8, 10, true);
            Assert.AreEqual(typeof(DateTime).Name, columnDetail.MappedType);
            Assert.AreEqual("DATE", columnDetail.DataType);
            Assert.AreEqual("Id", columnDetail.ColumnName);
        }
    }

    [TestFixture]
    public class DataTypeMapperTest
    {
        [Test]
        public void Map()
        {
            var mapper = new DataTypeMapper();
            Assert.AreEqual(typeof(int), mapper.MapFromDBType("int", null, null, null));
            Assert.AreEqual(typeof(int), mapper.MapFromDBType("INTERVAL YEAR TO MONTH", null, null, null));
            Assert.AreEqual(typeof(int), mapper.MapFromDBType("BINARY_INTEGER", null, null, null));
            Assert.AreEqual(typeof(DateTime), mapper.MapFromDBType("DATE", null, null, null));
            Assert.AreEqual(typeof(DateTime), mapper.MapFromDBType("datetime", null, null, null));
            Assert.AreEqual(typeof(DateTime), mapper.MapFromDBType("TIMESTAMP", null, null, null));
            Assert.AreEqual(typeof(DateTime), mapper.MapFromDBType("TIMESTAMP WITH TIME ZONE", null, null, null));
            Assert.AreEqual(typeof(DateTime), mapper.MapFromDBType("TIMESTAMP WITH LOCAL TIME ZONE", null, null, null));

            Assert.AreEqual(typeof(long), mapper.MapFromDBType("NUMBER", null, null, null));
            Assert.AreEqual(typeof(long), mapper.MapFromDBType("nchar", null, null, null));
            Assert.AreEqual(typeof(long), mapper.MapFromDBType("LONG", null, null, null));

            Assert.AreEqual(typeof(double), mapper.MapFromDBType("BINARY_DOUBLE", null, null, null));
            Assert.AreEqual(typeof(float), mapper.MapFromDBType("BINARY_FLOAT", null, null, null));
            Assert.AreEqual(typeof(float), mapper.MapFromDBType("FLOAT", null, null, null));

            Assert.AreEqual(typeof(byte[]), mapper.MapFromDBType("BLOB", null, null, null));
            Assert.AreEqual(typeof(byte[]), mapper.MapFromDBType("BFILE *", null, null, null));
            Assert.AreEqual(typeof(byte[]), mapper.MapFromDBType("LONG RAW", null, null, null));

            Assert.AreEqual(typeof(TimeSpan), mapper.MapFromDBType("INTERVAL DAY TO SECOND", null, null, null));
        }
    }
}