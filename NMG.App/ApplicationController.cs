using NMG.Core;
using NMG.Core.Domain;
using NMG.Core.Generator;

namespace NHibernateMappingGenerator
{
    public class ApplicationController
    {
        private readonly MappingGenerator mappingGenerator;
        private readonly CodeGenerator codeGenerator;

        public ApplicationController(ServerType serverType, string folderPath, string tableName, string nameSpace, string assemblyName, string sequence, ColumnDetails columnDetails, Preferences preferences, Language language)
        {
            folderPath = AddSlashToFolderPath(folderPath);
            codeGenerator = new CodeGenerator(folderPath, tableName, nameSpace, assemblyName, sequence, columnDetails, language, preferences);

            if (serverType == ServerType.Oracle)
            {
                mappingGenerator = new OracleMappingGenerator(folderPath, tableName, nameSpace, assemblyName, sequence, columnDetails, preferences);
            }
            else
            {
                mappingGenerator = new SqlMappingGenerator(folderPath, tableName, nameSpace, assemblyName, columnDetails, preferences);
            }
        }

        public void Generate()
        {
            mappingGenerator.Generate();
            codeGenerator.Generate();
        }

        private string AddSlashToFolderPath(string folderPath)
        {
            if (!folderPath.EndsWith("\\"))
            {
                folderPath += "\\";
            }
            return folderPath;
        }
    }
}