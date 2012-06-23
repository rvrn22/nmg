using System;
using System.IO;
using NMG.Core.Domain;
using NMG.Core.TextFormatter;

namespace NMG.Core.Generator
{
    public abstract class AbstractGenerator : IGenerator
    {
        protected Table Table;
        protected string assemblyName;
        protected string filePath;
        protected string nameSpace;
        protected string sequenceName;
        protected string tableName;
		internal const string TABS = "\t\t\t";
    	protected string ClassNamePrefix { get; set;}

        protected AbstractGenerator(string filePath, string specificFolder, string tableName, string nameSpace, string assemblyName, string sequenceName, Table table, ApplicationPreferences appPrefs)
        {
            this.filePath = filePath;
            if(appPrefs.GenerateInFolders)
            {
                this.filePath = Path.Combine(filePath, specificFolder);
                if(!this.filePath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                {
                    this.filePath = this.filePath + Path.DirectorySeparatorChar;
                }
            }
            this.tableName = tableName;
            this.nameSpace = nameSpace;
            this.assemblyName = assemblyName;
            this.sequenceName = sequenceName;
            Table = table;
            Formatter = TextFormatterFactory.GetTextFormatter(appPrefs);
        }

        public bool UsesSequence
        {
            get
            {
                return !String.IsNullOrEmpty(sequenceName);
            }
        }

        #region IGenerator Members

        public ITextFormatter Formatter { get; set; }

        public abstract void Generate();

        #endregion
    }
}