using System.Collections.Generic;
using NMG.Core;

namespace NMG.Service
{
    public class MappingController
    {
        private readonly ServerType serverType;
        private string folderPath;
        private readonly List<string> tableNames;
        private readonly string nameSpace;
        private readonly string assemblyName;
        private readonly string sequence;
        private readonly ColumnDetails columnDetails;

        public MappingController(ServerType serverType, string folderPath, List<string> tableNames, string nameSpace, string assemblyName, string sequence, ColumnDetails columnDetails)
        {
            this.serverType = serverType;
            this.folderPath = folderPath;
            this.tableNames = tableNames;
            this.nameSpace = nameSpace;
            this.assemblyName = assemblyName;
            this.sequence = sequence;
            this.columnDetails = columnDetails;
        }

        public void Generate(Language language)
        {
            AddSlashToFolderPath();
            BaseMappingGenerator generator = GetGenerator();
            var codeGenerator = new CodeGenerator(folderPath, tableNames, nameSpace, assemblyName, sequence, columnDetails, language);                
            generator.Generate();
            codeGenerator.Generate();
        }

        private BaseMappingGenerator GetGenerator()
        {
            BaseMappingGenerator generator;
            if(serverType == ServerType.Oracle)
            {
                generator = new OracleMappingGenerator(folderPath, tableNames, nameSpace, assemblyName, sequence, columnDetails);
            }else
            {
                generator = new SqlMappingGenerator(folderPath, tableNames, nameSpace, assemblyName, columnDetails);
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
