using NMG.Core.Domain;

namespace NMG.Core.Generator
{
    public abstract class AbstractGenerator : IGenerator
    {
        protected Table Table;
        protected string assemblyName;
        protected string filePath;
        protected string nameSpace;
        protected string sequenceName;
        protected string tableName;

        protected AbstractGenerator(string filePath, string tableName, string nameSpace, string assemblyName,
                                    string sequenceName, Table table)
        {
            this.filePath = filePath;
            this.tableName = tableName;
            this.nameSpace = nameSpace;
            this.assemblyName = assemblyName;
            this.sequenceName = sequenceName;
            Table = table;
        }

        #region IGenerator Members

        public abstract void Generate();

        #endregion
    }
}