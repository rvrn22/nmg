using System;
using NMG.Core;
using NUnit.Framework;
using NMG.Core.Domain;

namespace NMG.Tests
{
    [TestFixture]
    public class DataTypeMapperTest
    {
        private static void AssertMappedTypes(ServerType serverType, DataTypeMapper mapper, Type expectedType, params string[] dataTypes)
        {
            foreach (string dataType in dataTypes)
            {
                Assert.AreEqual(expectedType, mapper.MapFromDBType(serverType, dataType, null, null, null));
            }
        }

        [Test]
        public void Map()
        {
            var mapper = new DataTypeMapper();

            AssertMappedTypes(ServerType.SqlServer, mapper, typeof (int), "int");
            AssertMappedTypes(ServerType.SqlServer, mapper, typeof (DateTime), "datetime", "smalldatetime");
            AssertMappedTypes(ServerType.SqlServer, mapper, typeof (long), "bigint");
            AssertMappedTypes(ServerType.SqlServer, mapper, typeof (string), "nchar");
            AssertMappedTypes(ServerType.SqlServer, mapper, typeof (double), "float");
            
            AssertMappedTypes(ServerType.SqlServer, mapper, typeof (decimal), "decimal", "money", "smallmoney");
            AssertMappedTypes(ServerType.SqlServer, mapper, typeof (byte[]), "binary", "varbinary", "image", "timestamp");

            Assert.AreEqual(typeof (Boolean), mapper.MapFromDBType(ServerType.SqlServer, "bit", null, null, null));
            Assert.AreEqual(typeof (Single), mapper.MapFromDBType(ServerType.SqlServer, "real", null, null, null));
            Assert.AreEqual(typeof (Int16), mapper.MapFromDBType(ServerType.SqlServer, "smallint", null, null, null));
            Assert.AreEqual(typeof (Guid), mapper.MapFromDBType(ServerType.SqlServer, "uniqueidentifier", null, null, null));
            Assert.AreEqual(typeof (Byte), mapper.MapFromDBType(ServerType.SqlServer, "tinyint", null, null, null));
        }
        
        [Test]
        public void OracleMap()
        {
            var mapper = new DataTypeMapper();
            AssertMappedTypes(ServerType.Oracle, mapper, typeof (System.DateTime), "DATE", "TIMESTAMP", "TIMESTAMP WITH TIME ZONE", "TIMESTAMP WITH LOCAL TIME ZONE");
            AssertMappedTypes(ServerType.Oracle, mapper, typeof (System.Int64), "LONG", "INTERVAL YEAR TO MONTH");
            AssertMappedTypes(ServerType.Oracle, mapper, typeof (byte[]), "BLOB", "BFILE *", "LONG RAW");
            AssertMappedTypes(ServerType.Oracle, mapper, typeof (System.Double), "BINARY_DOUBLE");
            AssertMappedTypes(ServerType.Oracle, mapper, typeof (System.Single), "BINARY_FLOAT");
            AssertMappedTypes(ServerType.Oracle, mapper, typeof (System.Decimal), "REAL", "FLOAT", "BINARY_INTEGER");
            
            Assert.AreEqual(typeof (TimeSpan), mapper.MapFromDBType(ServerType.Oracle, "INTERVAL DAY TO SECOND", null, null, null));
            Assert.AreEqual(typeof (Int32), mapper.MapFromDBType(ServerType.Oracle, "NUMBER", null, 9, 0));
            Assert.AreEqual(typeof (Int64), mapper.MapFromDBType(ServerType.Oracle, "NUMBER", null, 14, 0));
            Assert.AreEqual(typeof (Double), mapper.MapFromDBType(ServerType.Oracle, "NUMBER", null, 11, 6));
            Assert.AreEqual(typeof (Decimal), mapper.MapFromDBType(ServerType.Oracle, "NUMBER", null, 24, 10));
        }
        
    }
}