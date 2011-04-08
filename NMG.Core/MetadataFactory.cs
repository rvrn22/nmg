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
            else if (serverType == ServerType.SqlServer)
            {
                metadataReader = new SqlServerMetadataReader(connectionStr);
            }
            else if (serverType == ServerType.MySQL)
            {
                metadataReader = new MysqlMetadataReader(connectionStr);
            }
            else
            {
                metadataReader = new NpgsqlMetadataReader(connectionStr);
            }
            return metadataReader;
        }
    }
}