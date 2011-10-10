using System;
using System.Collections.Generic;

namespace NMG.Core.Domain
{
    public class DotNetTypes : List<string>
    {
        public DotNetTypes()
        {
            Add(typeof (String).FullName);
            Add(typeof (Int16).FullName);
            Add(typeof (Int32).FullName);
            Add(typeof (Int64).FullName);

			Add(typeof(double).FullName);
			Add(typeof(decimal).FullName);

            Add(typeof (DateTime).FullName);
            Add(typeof (TimeSpan).FullName);

            Add(typeof (Byte).FullName);
            Add(typeof (byte[]).FullName);

            Add(typeof (Boolean).FullName);
            Add(typeof (Single).FullName);
            Add(typeof (Guid).FullName);
        }
    }
}