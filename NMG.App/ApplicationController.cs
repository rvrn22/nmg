using NMG.Core;
using NMG.Core.Domain;
using NMG.Core.Generator;

namespace NHibernateMappingGenerator
{
    public class ApplicationController
    {
        private readonly ApplicationPreferences applicationPreferences;
        private readonly CastleGenerator castleGenerator;
        private readonly CodeGenerator codeGenerator;
        private readonly FluentGenerator fluentGenerator;
        private readonly MappingGenerator mappingGenerator;

        public ApplicationController(ApplicationPreferences applicationPreferences, Table table)
        {
            this.applicationPreferences = applicationPreferences;
            applicationPreferences.FolderPath = AddSlashToFolderPath(applicationPreferences.FolderPath);
            codeGenerator = new CodeGenerator(applicationPreferences, table);
            fluentGenerator = new FluentGenerator(applicationPreferences, table);
            castleGenerator = new CastleGenerator(applicationPreferences, table);
            if (applicationPreferences.ServerType == ServerType.Oracle)
            {
                mappingGenerator = new OracleMappingGenerator(applicationPreferences, table);
            }
            else
            {
                mappingGenerator = new SqlMappingGenerator(applicationPreferences, table);
            }
        }

        public void Generate()
        {
            codeGenerator.Generate();
            if (applicationPreferences.IsFluent)
            {
                fluentGenerator.Generate();
            }
            else if (applicationPreferences.IsCastle)
            {
                castleGenerator.Generate();
            }
            else
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