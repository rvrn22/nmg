using System;

namespace NMG.Core.Reader
{
    // Type safe enumerator
    public sealed class OracleConstraintType
    {
        public static readonly OracleConstraintType PrimaryKey = new OracleConstraintType(1, "P");
        public static readonly OracleConstraintType ForeignKey = new OracleConstraintType(2, "R");
        public static readonly OracleConstraintType Check = new OracleConstraintType(3, "C");
        public static readonly OracleConstraintType Unique = new OracleConstraintType(4, "U");
        private readonly String name;
        private readonly int value;

        private OracleConstraintType(int value, String name)
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