using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NMG.Core.Reader
{
    public sealed class MysqlConstraintType
    {
        public static readonly MysqlConstraintType PrimaryKey = new MysqlConstraintType(1, "PRI");
        public static readonly MysqlConstraintType ForeignKey = new MysqlConstraintType(2, "MUL");
        public static readonly MysqlConstraintType Check = new MysqlConstraintType(3, "CHECK");
        public static readonly MysqlConstraintType Unique = new MysqlConstraintType(4, "UNIQUE");
        private readonly String name;
        private readonly int value;

        private MysqlConstraintType(int value, String name)
        {
            this.name = name;
            this.value = value;
        }

        public override String ToString()
        {
            return name;
        }
    }
}
