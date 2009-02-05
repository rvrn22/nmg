namespace NMG.Core
{
    public abstract class BaseCodeGenerator : BaseGenerator
    {
        protected BaseCodeGenerator(string filePath, string tableName, string nameSpace, string assemblyName, string sequenceNumber, ColumnDetails columnDetails) : base(filePath, tableName, nameSpace, assemblyName, sequenceNumber, columnDetails)
        {
        }
    }
}