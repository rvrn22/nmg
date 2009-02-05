namespace NMG.Core
{
    public class OracleMappingGenerator : BaseMappingGenerator 
    {
        public OracleMappingGenerator(string path, string tableName, string nameSpace, string assemblyName, string sequenceNumber, ColumnDetails columnDetails) : base(path, tableName, nameSpace, assemblyName, sequenceNumber, columnDetails)
        {
        }
    }
}