using System;
using System.Collections.Generic;

namespace NHibernate.Glimpse.Core
{
    internal class RequestDebugInfo
    {
        internal RequestDebugInfo()
        {
            Details = new List<DebugInfoDetail>();
        }

        public string Url { get; set; }

        public Guid GlimpseKey { get; set; }

        public int Selects { get; set; }
        
        public int Inserts { get; set; }
        
        public int Updates { get; set; }
        
        public int Deletes { get; set; }
        
        public int Batch { get; set; }
        
        public long EntitiesLoaded { get; set; }

        public string Summary { get; set; }

        public List<DebugInfoDetail> Details { get; set; }

        public string EntityDetails { get; set; }
    }
}