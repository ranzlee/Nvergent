using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NHibernate.Glimpse.Core
{
    internal class SessionContext
    {
        private static bool IsUsingGlimpse()
        {
            var context = HttpContext.Current;
            if (context == null) return false;
            var cookie = context.Request.Cookies[Plugin.GlimpseCookie];
            return cookie != null;
        }

        internal static IList<EntityLoadedStatistic> GetStatistics()
        {
            if (!IsUsingGlimpse()) return new List<EntityLoadedStatistic>();
            var l = (IList<EntityLoadedStatistic>)HttpContext.Current.Items[Plugin.GlimpseEntityLoadStatsKey];
            if (l == null)
            {
                l = new List<EntityLoadedStatistic>();
                HttpContext.Current.Items.Add(Plugin.GlimpseEntityLoadStatsKey, l);
            }
            return l;
        }

        public static void SetEntityStatistics(System.Type entity, string id)
        {
            if (!IsUsingGlimpse()) return;
            var l = GetStatistics();
            var entityStat = l.Where(i => i.Entity == entity).FirstOrDefault();
            if (entityStat == null)
            {
                l.Add(new EntityLoadedStatistic(entity, id));
            }
            else
            {
                entityStat.Increment(id);
            }    
        }

        public static long TotalEntitiesLoaded
        {
            get
            {
                if (!IsUsingGlimpse()) return 0;
                var l = GetStatistics();
                return l.Sum(i => i.Count);    
            }
        }
    }
}