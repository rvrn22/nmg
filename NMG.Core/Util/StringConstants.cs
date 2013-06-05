namespace NMG.Core.Util
{
    public class StringConstants
    {
        public static string ORACLE_CONN_STR_TEMPLATE = "Data Source=XE;User Id=Sample; Password=password;";

        public static string SQL_CONN_STR_TEMPLATE =
            "Data Source=localhost;Initial Catalog=Sample;Integrated Security=SSPI;";

        public static string POSTGRESQL_CONN_STR_TEMPLATE =
            "server=localhost;Port=5432;Database=postgres;User Id=postgres;Password=password;";

        public static string MYSQL_CONN_STR_TEMPLATE =
            "Server=localhost;Port=3306;Database=letrunghieu;Uid=root;Pwd=a;";

        public static string SQLITE_CONN_STR_TEMPLATE =
            "Data Source=local.db;Version=3;New=False;Compress=True;";

        public static string SYBASE_CONN_STR_TEMPLATE =
            "Provider=ASAProv;UID=uidname;PWD=password;DatabaseName=databasename;EngineName=enginename;CommLinks=TCPIP{host=servername}";

        public static string INGRES_CONN_STR_TEMPLATE = "Host=localhost;Port=II7;Database=myDb;User ID=myUser;Password=myPassword;";


        public static string CUBRID_CONN_STR_TEMPLATE =
            "server=localhost;port=33000;database=demodb;user=dba;password=";
    }
}