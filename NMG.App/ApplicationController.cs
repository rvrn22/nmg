using NMG.Core;
using NMG.Core.Domain;
using NMG.Core.Generator;

namespace NHibernateMappingGenerator
{
    public class ApplicationController
    {
        private readonly ApplicationPreferences applicationPreferences;
        private readonly MappingGenerator mappingGenerator;
        private readonly CodeGenerator codeGenerator;
        private readonly FluentGenerator fluentGenerator;

        public ApplicationController(ApplicationPreferences applicationPreferences, ColumnDetails columnDetails)
        {
            this.applicationPreferences = applicationPreferences;
            applicationPreferences.FolderPath = AddSlashToFolderPath(applicationPreferences.FolderPath);
            codeGenerator = new CodeGenerator(applicationPreferences, columnDetails);
            fluentGenerator = new FluentGenerator(applicationPreferences, columnDetails);
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
            codeGenerator.Generate();
            if(applicationPreferences.IsFluent)
            {
                fluentGenerator.Generate();
            }else
            {
                mappingGenerator.Generate(); 
            }
        }

        private static string AddSlashToFolderPath(string folderPath)
        {
            if (!folderPath.EndsWith("\\"))
            {
                folderPath += "\\";
            }
            return folderPath;
        }
    }
}