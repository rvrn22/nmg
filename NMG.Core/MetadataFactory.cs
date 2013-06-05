using NMG.Core.Domain;
using NMG.Core.Reader;

namespace NMG.Core
{
    public class MetadataFactory
    {
        public static IMetadataReader GetReader(ServerType serverType, string connectionStr)
        {
            switch (serverType)
            {
                case ServerType.Oracle:
                    return new OracleMetadataReader(connectionStr);
                case ServerType.SqlServer:
                    return new SqlServerMetadataReader(connectionStr);
                case ServerType.MySQL:
                    return new MysqlMetadataReader(connectionStr);
                case ServerType.SQLite:
                    return new SqliteMetadataReader(connectionStr);
                case ServerType.Sybase:
                    return new SybaseMetadataReader(connectionStr);
                case ServerType.Ingres:
                    return new IngresMetadataReader(connectionStr);
                case ServerType.CUBRID:
                    return new CUBRIDMetadataReader(connectionStr);
                default:
                    return new NpgsqlMetadataReader(connectionStr);
            }
        }
    }
}