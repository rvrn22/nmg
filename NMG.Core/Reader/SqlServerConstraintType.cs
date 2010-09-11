using System;

namespace NMG.Core.Reader
{
    // Type safe enumerator
    public sealed class SqlServerConstraintType
    {
        public static readonly SqlServerConstraintType PrimaryKey = new SqlServerConstraintType(1, "PRIMARY KEY");
        public static readonly SqlServerConstraintType ForeignKey = new SqlServerConstraintType(2, "FOREIGN KEY");
        public static readonly SqlServerConstraintType Check = new SqlServerConstraintType(3, "CHECK");
        public static readonly SqlServerConstraintType Unique = new SqlServerConstraintType(4, "UNIQUE");
        private readonly String name;
        private readonly int value;

        private SqlServerConstraintType(int value, String name)
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