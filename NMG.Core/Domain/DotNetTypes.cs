using System.Collections.Generic;

namespace NMG.Core.Domain
{
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