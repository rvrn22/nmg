using System.Collections.Generic;
using NMG.Core.Domain;

namespace NMG.Core.Reader
{
    public interface IMetadataReader
    {
        ColumnDetails GetTableDetails(string selectedTableName);
        List<string> GetTables();
        List<string> GetSequences();
    }
}