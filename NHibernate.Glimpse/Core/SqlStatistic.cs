using System;
using System.Collections.Generic;

namespace NHibernate.Glimpse.Core
{
    internal class SqlStatistic
    {
        internal SqlStatistic()
        {
            StackFrames = new List<string>();
        }

        internal string Sql { get; set; }

        internal DateTime Timestamp { get; set; }

        internal IList<string> StackFrames { get; set; }
    }
}