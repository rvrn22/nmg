using System.Collections.Generic;
using NMG.Core.Domain;

namespace NMG.Service
{
    public interface IMetadataReader
    {
        ColumnDetails GetTableDetails(string selectedTableName);
        List<string> GetTables();
        List<string> GetSequences();
    }
}
