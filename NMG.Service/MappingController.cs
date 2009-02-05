using NMG.Core;

namespace NMG.Service
{
    public class MappingController
    {
        private string folderPath;
        private readonly string tableName;
        private readonly string nameSpace;
        private readonly string assemblyName;
        private readonly string sequnce;
        private readonly ColumnDetails columnDetails;

        public MappingController(string folderPath, string tableName, string nameSpace, string assemblyName, string sequnce, ColumnDetails columnDetails)
        {
            this.folderPath = folderPath;
            this.tableName = tableName;
            this.nameSpace = nameSpace;
            this.assemblyName = assemblyName;
            this.sequnce = sequnce;
            this.columnDetails = columnDetails;
        }

        public void Generate()
        {
            if (!folderPath.EndsWith("\\"))
            {
                folderPath += "\\";
            }
            var generator = new OracleMappingGenerator(folderPath, tableName, nameSpace, assemblyName, sequnce, columnDetails);
            var codeGenerator = new CodeGenerator(folderPath, tableName, nameSpace, assemblyName, sequnce, columnDetails);
            generator.Generate();
            codeGenerator.Generate();
        }
    }
}
