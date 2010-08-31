using System.Collections.Generic;
using NMG.Core.Domain;

namespace NMG.Core.Reader
{
    public interface IMetadataReader
    {
        IList<Column> GetTableDetails(Table selectedTableName);
        List<Table> GetTables();
        List<string> GetSequences();
        List<string> GetForeignKeyTables(string columnName);
        //bool UsesCompositeKey(string tableName);
    }
}