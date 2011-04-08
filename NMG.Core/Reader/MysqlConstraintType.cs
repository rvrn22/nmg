using System;

namespace NMG.Core.Reader
{
    public sealed class MysqlConstraintType
    {
        public static readonly MysqlConstraintType PrimaryKey = new MysqlConstraintType("PRI");
        public static readonly MysqlConstraintType ForeignKey = new MysqlConstraintType("MUL");
        public static readonly MysqlConstraintType Check = new MysqlConstraintType("CHECK");
        public static readonly MysqlConstraintType Unique = new MysqlConstraintType("UNIQUE");
        private readonly String name;

        private MysqlConstraintType(String name)
        {
            this.name = name;
        }

        public override String ToString()
        {
            return name;
        }
    }
}
