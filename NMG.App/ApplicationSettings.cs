using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;
using NMG.Core;
using NMG.Core.Domain;

namespace NHibernateMappingGenerator
{
    public class ApplicationSettings
    {
        public ApplicationSettings()
        {
            Connections = new List<Connection>();
        }

        public List<Connection> Connections { get; set; }
        public Guid? LastUsedConnection { get; set; }
            
        public string NameSpace { get; set; }

        public string NameSpaceMap { get; set; }

        public string AssemblyName { get; set; }

        public Language Language { get; set; }

        public bool IsFluent { get; set; }

        public bool IsAutoProperty { get; set; }

        public CodeGenerationOptions CodeGenerationOptions { get; set; }

        public FieldGenerationConvention FieldGenerationConvention { get; set; }

        public string FolderPath { get; set; }

        public string DomainFolderPath { get; set; }

        public string ForeignEntityCollectionType { get; set; }

        public string InheritenceAndInterfaces { get; set; }

        public string ClassNamePrefix { get; set; }

        public bool GenerateWcfContracts { get; set; }

        public bool GeneratePartialClasses { get; set; }

        public FieldNamingConvention FieldNamingConvention { get; set; }

        public string Prefix { get; set; }

        public bool IsNhFluent { get; set; }

        public bool IsCastle { get; set; }

        public bool IsByCode { get; set; }

        public bool GenerateInFolders { get; set; }

        public bool UseLazy { get; set; }

        public bool IncludeForeignKeys { get; set; }

        public bool IncludeLengthAndScale { get; set; }

        public List<string> FieldPrefixRemovalList { get; set; }


        public void Save()
        {
            var streamWriter = new StreamWriter(Application.LocalUserAppDataPath + @"\nmg.config", false);
            using (streamWriter)
            {
                var xmlSerializer = new XmlSerializer(typeof (ApplicationSettings));
                xmlSerializer.Serialize(streamWriter, this);
            }
        }

        public static ApplicationSettings Load()
        {
            ApplicationSettings appSettings = null;
            var xmlSerializer = new XmlSerializer(typeof (ApplicationSettings));
            var fi = new FileInfo(Application.LocalUserAppDataPath + @"\nmg.config");
            if (fi.Exists)
            {
                using (FileStream fileStream = fi.OpenRead())
                {
                    appSettings = (ApplicationSettings)xmlSerializer.Deserialize(fileStream);
                }
            }
            return appSettings;
        }
    }

    public class Connection
    {
        public Guid Id { get; set; }
        public string ConnectionString { get; set; }
        public string Name { get; set; }
        public ServerType Type { get; set; }
    }
}