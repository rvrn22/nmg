namespace NMG.Core
{
    public class SqlMappingGenerator : BaseMappingGenerator 
    {
        public SqlMappingGenerator(string path, string tableName, string nameSpace, string assemblyName, ColumnDetails columnDetails) : base(path, tableName, nameSpace, assemblyName, string.Empty, columnDetails)
        {
        }
    }
}