using System;
using System.Collections.Generic;

namespace NHibernate.Glimpse.Core
{
    internal class DebugInfoDetail
    {
        internal DebugInfoDetail()
        {
            StackFrames = new List<string>();
        }

        internal DateTime Timestamp { get; set; }

        internal string Description { get; set; }

        internal IList<string> StackFrames { get; set; }
    }
}