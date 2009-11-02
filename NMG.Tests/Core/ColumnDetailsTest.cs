using System;
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
}