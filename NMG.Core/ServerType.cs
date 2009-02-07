using System.Collections.Generic;

namespace NMG.Core
{
    public enum ServerType
    {
        Oracle,
        SqlServer2005,
        SqlServer2008
    }

    public class DotNetTypes : List<string>
    {
        public DotNetTypes()
        {
            Add("String");
            Add("Int64");
            Add("Int32");
            Add("DateTime");
        }
    }
}