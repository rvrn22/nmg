using System.Collections.Generic;
using NMG.Core;

namespace NMG.Service
{
    public abstract class DBController
    {
        protected readonly string connectionStr;

        protected DBController(string connectionStr)
        {
            this.connectionStr = connectionStr;
        }

        public abstract ColumnDetails GetTableDetails(string selectedTableName);
        public abstract List<string> GetTables();
        public abstract List<string> GetSequences();
    }
}
