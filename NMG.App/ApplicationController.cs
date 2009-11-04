using NMG.Core.Domain;
using NMG.Core.Generator;

namespace NHibernateMappingGenerator
{
    public class ApplicationController
    {
        private readonly MappingGenerator mappingGenerator;
        private readonly CodeGenerator codeGenerator;

        public ApplicationController(NMG.Core.ApplicationPreferences applicationPreferences, ColumnDetails columnDetails)
        {
            applicationPreferences.FolderPath = AddSlashToFolderPath(applicationPreferences.FolderPath);
            codeGenerator = new CodeGenerator(applicationPreferences, columnDetails);

            if (applicationPreferences.ServerType == ServerType.Oracle)
            {
                mappingGenerator = new OracleMappingGenerator(applicationPreferences, columnDetails);
            }
            else
            {
                mappingGenerator = new SqlMappingGenerator(applicationPreferences, columnDetails);
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