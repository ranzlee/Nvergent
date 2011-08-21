using System;

namespace NHibernate.Glimpse.Core
{
    internal class DebugInfoDetail
    {
        public DateTime Timestamp { get; set; }

        public string Description { get; set; }

        public string Method { get; set; }

        public string Member { get; set; }
    }
}