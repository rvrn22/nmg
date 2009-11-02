using NMG.Core.Domain;

namespace NMG.Core.Generator
{
    public abstract class AbstractGenerator : IGenerator
    {
        protected string filePath;
        protected string tableName;
        protected string nameSpace;
        protected string assemblyName;
        protected string sequenceName;
        protected ColumnDetails columnDetails;

        protected AbstractGenerator(string filePath, string tableName, string nameSpace, string assemblyName, string sequenceName, ColumnDetails columnDetails)
        {
            this.filePath = filePath;
            this.tableName = tableName;
            this.nameSpace = nameSpace;
            this.assemblyName = assemblyName;
            this.sequenceName = sequenceName;
            this.columnDetails = columnDetails;
        } 

        public abstract void Generate();
    }
}