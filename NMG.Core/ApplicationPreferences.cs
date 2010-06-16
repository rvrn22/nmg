using NMG.Core.Domain;

namespace NMG.Core
{
    public class ApplicationPreferences
    {
        public string TableName { get; set; }

        public string FolderPath { get; set; }

        public string NameSpace { get; set; }

        public string AssemblyName { get; set; }

        public ServerType ServerType { get; set; }
        public string ConnectionString { get; set; }

        public string Sequence { get; set; }
        public bool IsFluent { get; set; }
        public string Prefix { get; set; }

        public Language Language { get; set; }
        public FieldNamingConvention FieldNamingConvention { get; set; }
        public FieldGenerationConvention FieldGenerationConvention { get; set; }

        public string EntityName { get; set; }

        public ApplicationPreferences()
        {
            FieldNamingConvention = FieldNamingConvention.SameAsDatabase;
            FieldGenerationConvention = FieldGenerationConvention.Field;
            Prefix = string.Empty;
        }

        public static ApplicationPreferences Default()
        {
            var preferences = new ApplicationPreferences
            {
                FieldGenerationConvention = FieldGenerationConvention.AutoProperty,
                FieldNamingConvention = FieldNamingConvention.SameAsDatabase,
                Prefix = string.Empty,
                IsFluent = true,
                Language = Language.CSharp,
            };
            return preferences;
        }
    }
}