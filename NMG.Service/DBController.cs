namespace NMG.Service
{
    public class DBController
    {
        protected readonly string connectionStr;

        public DBController(string connectionStr)
        {
            this.connectionStr = connectionStr;
        }
    }
}
