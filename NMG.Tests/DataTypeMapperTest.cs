using System;
using NMG.Core;
using NUnit.Framework;

namespace NMG.Tests
{
    [TestFixture]
    public class DataTypeMapperTest
    {
        [Test]
        public void Map()
        {
            var mapper = new DataTypeMapper();
            
            AssertMappedTypes(mapper, typeof(int), "int", "INTERVAL YEAR TO MONTH", "BINARY_INTEGER");
            AssertMappedTypes(mapper, typeof(DateTime), "DATE", "datetime", "TIMESTAMP", "TIMESTAMP WITH TIME ZONE", "TIMESTAMP WITH LOCAL TIME ZONE", "smalldatetime");
            AssertMappedTypes(mapper, typeof(long), "NUMBER", "nchar", "LONG", "bigint");
            AssertMappedTypes(mapper, typeof(double), "BINARY_DOUBLE", "float");
            AssertMappedTypes(mapper, typeof(float), "BINARY_FLOAT", "FLOAT");
            AssertMappedTypes(mapper, typeof(decimal), "decimal", "money", "smallmoney");
            AssertMappedTypes(mapper, typeof(byte[]), "BLOB", "BFILE *", "LONG RAW", "binary", "varbinary", "image", "timestamp");


            Assert.AreEqual(typeof(TimeSpan), mapper.MapFromDBType("INTERVAL DAY TO SECOND", null, null, null));
            Assert.AreEqual(typeof(Boolean), mapper.MapFromDBType("bit", null, null, null));
            Assert.AreEqual(typeof(Single), mapper.MapFromDBType("real", null, null, null));
            Assert.AreEqual(typeof(Int16), mapper.MapFromDBType("smallint", null, null, null));
            Assert.AreEqual(typeof(Guid), mapper.MapFromDBType("uniqueidentifier", null, null, null));
            Assert.AreEqual(typeof(Byte), mapper.MapFromDBType("tinyint", null, null, null));
        }

        private static void AssertMappedTypes(DataTypeMapper mapper, Type expectedType, params string[] dataTypes)
        {
            foreach (var dataType in dataTypes)
            {
                Assert.AreEqual(expectedType, mapper.MapFromDBType(dataType, null, null, null));
            }
        }
    }
}