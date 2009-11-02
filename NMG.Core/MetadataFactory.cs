using NMG.Core.Domain;
using NMG.Core.Reader;

namespace NMG.Core
{
    public class MetadataFactory
    {
        public static IMetadataReader GetReader(ServerType serverType, string connectionStr)
        {
            IMetadataReader metadataReader;
            if (serverType == ServerType.Oracle)
            {
                metadataReader = new OracleMetadataReader(connectionStr);
            }
            else
            {
                metadataReader = new SqlServerMetadataReader(connectionStr);
            }
            return metadataReader;
        }
    }
}