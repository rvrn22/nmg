using System.Collections.Generic;
using NMG.Core.Domain;

namespace NMG.Core
{
    public class ApplicationPreferences
    {
        public ApplicationPreferences()
        {
            FieldNamingConvention = FieldNamingConvention.SameAsDatabase;
            FieldGenerationConvention = FieldGenerationConvention.Field;
            Prefix = string.Empty;
        }

        public string TableName { get; set; }

        public string FolderPath { get; set; }

        public string DomainFolderPath { get; set; }

        public string NameSpace { get; set; }

        public string NameSpaceMap { get; set; }

        public string AssemblyName { get; set; }

        public ServerType ServerType { get; set; }

        public string ConnectionString { get; set; }

        public string Sequence { get; set; }

        public bool IsFluent { get; set; }

        public bool IsNhFluent { get; set; }

        public bool IsCastle { get; set; }

        public bool IsByCode { get ; set; }

        public bool GeneratePartialClasses { get; set; }

        public string Prefix { get; set; }

        public string ForeignEntityCollectionType { get; set; }

        public string InheritenceAndInterfaces { get; set; }

        public string ClassNamePrefix { get; set; }

        public Language Language { get; set; }

        public FieldNamingConvention FieldNamingConvention { get; set; }

        public FieldGenerationConvention FieldGenerationConvention { get; set; }

        public string EntityName { get; set; }

        public bool GenerateWcfDataContract { get; set; }

        public bool GenerateInFolders { get; set; }
        
        public bool UseLazy { get; set; }
        
        public bool IncludeForeignKeys { get; set; }

        public bool IncludeLengthAndScale { get; set; }

        public List<string> FieldPrefixRemovalList { get; set; }

        public ValidationStyle ValidatorStyle { get; set; }

        public static ApplicationPreferences Default()
        {
            var preferences = new ApplicationPreferences
                                  {
                                      FieldGenerationConvention = FieldGenerationConvention.AutoProperty,
                                      FieldNamingConvention = FieldNamingConvention.SameAsDatabase,
                                      Prefix = string.Empty,
                                      IsNhFluent = true,
                                      Language = Language.CSharp,
                                      ForeignEntityCollectionType = "IList",
                                      InheritenceAndInterfaces = "",
                                      UseLazy = true
                                  };
            return preferences;
        }
    }
}