using System;
using System.Collections.Generic;

namespace NHibernate.Glimpse.Core
{
    internal class LogStatistic
    {
        internal LogStatistic()
        {
            StackFrames = new List<string>();
        }

        internal string Sql { get; set; }

        internal string Metric { get; set; }

        internal string ConnectionNotification { get; set; }

        internal string TransactionNotification { get; set; }

        internal DateTime Timestamp { get; set; }

        internal IList<string> StackFrames { get; set; }
    }
}