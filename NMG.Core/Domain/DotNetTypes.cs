using System;
using System.Collections.Generic;

namespace NMG.Core.Domain
{
    public class DotNetTypes : List<string>
    {
        public DotNetTypes()
        {
            Add(typeof(String).Name);
            Add(typeof(Int16).Name);
            Add(typeof(Int32).Name);
            Add(typeof(Int64).Name);
                 
            Add(typeof(double).Name);

            Add(typeof(DateTime).Name);
            Add(typeof(TimeSpan).Name);

            Add(typeof(Byte).Name);
            Add(typeof(byte[]).Name);

            Add(typeof(Boolean).Name);
            Add(typeof(Single).Name);
            Add(typeof(Guid).Name);
        }
    }
}