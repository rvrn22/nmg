using NMG.Core;
using NMG.Core.Domain;

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

        public void Generate(Language language)
        {
            AddSlashToFolderPath();
            MappingGenerator generator = GetGenerator();
            var codeGenerator = new CodeGenerator(folderPath, tableName, nameSpace, assemblyName, sequence, columnDetails, language);                
            generator.Generate();
            codeGenerator.Generate();
        }

        private MappingGenerator GetGenerator()
        {
            MappingGenerator generator;
            if(serverType == ServerType.Oracle)
            {
                generator = new OracleMappingGenerator(folderPath, tableName, nameSpace, assemblyName, sequence, columnDetails);
            }else
            {
                generator = new SqlMappingGenerator(folderPath, tableName, nameSpace, assemblyName, columnDetails);
            }
            return generator;
        }

        private void AddSlashToFolderPath()
        {
            if (!folderPath.EndsWith("\\"))
            {
                folderPath += "\\";
            }
        }
    }
}
