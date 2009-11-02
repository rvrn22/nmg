using NMG.Core.Domain;
using NMG.Core.Generator;

namespace NMG.Core
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

        public void Generate(Language language, Preferences preferences)
        {
            AddSlashToFolderPath();
            var mappingGenerator = GetMappingGenerator(preferences);
            mappingGenerator.Generate();
            var codeGenerator = new CodeGenerator(folderPath, tableName, nameSpace, assemblyName, sequence, columnDetails, language, preferences);
            codeGenerator.Generate();
        }

        private IGenerator GetMappingGenerator(Preferences preferences)
        {
            MappingGenerator generator;
            if(serverType == ServerType.Oracle)
            {
                generator = new OracleMappingGenerator(folderPath, tableName, nameSpace, assemblyName, sequence, columnDetails, preferences);
            }else
            {
                generator = new SqlMappingGenerator(folderPath, tableName, nameSpace, assemblyName, columnDetails, preferences);
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