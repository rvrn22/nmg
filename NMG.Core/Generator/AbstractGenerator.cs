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
        protected Table Table;

        protected AbstractGenerator(string filePath, string tableName, string nameSpace, string assemblyName, string sequenceName, Table table)
        {
            this.filePath = filePath;
            this.tableName = tableName;
            this.nameSpace = nameSpace;
            this.assemblyName = assemblyName;
            this.sequenceName = sequenceName;
            this.Table = table;
        } 

        public abstract void Generate();
    }
}