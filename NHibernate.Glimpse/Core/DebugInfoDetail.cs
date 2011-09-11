using System;
using System.Collections.Generic;

namespace NHibernate.Glimpse.Core
{
    internal class DebugInfoDetail
    {
        internal DebugInfoDetail()
        {
            StackFrames = new List<string>();
            Timestamp = DateTime.MinValue;
        }

        internal DateTime Timestamp { get; set; }

        internal string Description { get; set; }

        internal bool IsSqlNotification { get; set; }

        internal bool IsConnectionNotification { get; set; }

        internal bool IsTransactionNotification { get; set; }

        internal bool IsFlushNotification { get; set; }

        internal bool IsLoadNotification { get; set; }

        internal IList<string> StackFrames { get; set; }
    }
}