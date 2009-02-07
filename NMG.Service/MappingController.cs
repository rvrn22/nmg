using NMG.Core;

namespace NMG.Service
{
    public class MappingController
    {
        private readonly ServerType serverType;
        private string folderPath;
        private readonly string tableName;
        private readonly string nameSpace;
        private readonly string assemblyName;
        private readonly string sequence;
        private readonly ColumnDetails columnDetails;

        public MappingController(ServerType serverType, string folderPath, string tableName, string nameSpace, string assemblyName, string sequence, ColumnDetails columnDetails)
        {
            this.serverType = serverType;
            this.folderPath = folderPath;
            this.tableName = tableName;
            this.nameSpace = nameSpace;
            this.assemblyName = assemblyName;
            this.sequence = sequence;
            this.columnDetails = columnDetails;
        }

        public void Generate()
        {
            if (!folderPath.EndsWith("\\"))
            {
                folderPath += "\\";
            }
            BaseMappingGenerator generator;
            if(serverType == ServerType.Oracle)
            {
                generator = new OracleMappingGenerator(folderPath, tableName, nameSpace, assemblyName, sequence, columnDetails);
            }else
            {
                generator = new SqlMappingGenerator(folderPath, tableName, nameSpace, assemblyName, columnDetails);
            }
            var codeGenerator = new CodeGenerator(folderPath, tableName, nameSpace, assemblyName, sequence, columnDetails);                
            generator.Generate();
            codeGenerator.Generate();
        }
    }
}
