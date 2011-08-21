using System;

namespace NHibernate.Glimpse.Core
{
    internal class SqlStatistic
    {
        public string Sql { get; set; }

        public DateTime Timestamp { get; set; }

        public string Method { get; set; }

        public string Member { get; set; }
    }
}