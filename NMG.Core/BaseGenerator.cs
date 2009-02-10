namespace NMG.Core
{
    public abstract class BaseGenerator : IMappingGenerator
    {
        protected string filePath;
        protected string tableName;
        protected string nameSpace;
        protected string assemblyName;
        protected string sequenceName;
        protected ColumnDetails columnDetails;

        protected BaseGenerator(string filePath, string tableName, string nameSpace, string assemblyName, string sequenceNumber, ColumnDetails columnDetails)
        {
            this.filePath = filePath;
            this.tableName = tableName;
            this.nameSpace = nameSpace;
            this.assemblyName = assemblyName;
            this.sequenceName = sequenceNumber;
            this.columnDetails = columnDetails;
        } 

        public abstract void Generate();
    }
}