namespace NMG.Core.Reader
{
    public interface IConstraintTypeResolver
    {
        bool IsPrimaryKey(string constraintType);
        bool IsForeignKey(string constraintType);
        bool IsUnique(string constraintType);
        bool IsCheck(string constraintType);
    }
}