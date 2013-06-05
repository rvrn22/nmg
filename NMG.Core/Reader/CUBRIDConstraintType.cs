using System;

namespace NMG.Core.Reader
{
    /// <summary>
    /// CUBRID implementation of the Constraint types
    /// http://www.cubrid.org
    /// NMG support - v1.0
    /// </summary>
    public sealed class CUBRIDConstraintType
    {
        public static readonly CUBRIDConstraintType PrimaryKey = new CUBRIDConstraintType(1, "PRIMARY KEY");
        public static readonly CUBRIDConstraintType ForeignKey = new CUBRIDConstraintType(2, "FOREIGN KEY");
        public static readonly CUBRIDConstraintType Unique = new CUBRIDConstraintType(3, "UNIQUE");
        private readonly String name;
        private readonly int value;

        private CUBRIDConstraintType(int value, String name)
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
