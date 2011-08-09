using System;
using NHibernate.Event;
using NHibernate.Event.Default;
using NHibernate.Glimpse.Core;

namespace NHibernate.Glimpse
{
    [Serializable]
    public class EntityPostLoadListener : DefaultPostLoadEventListener
    {
        public override void OnPostLoad(PostLoadEvent @event)
        {
            base.OnPostLoad(@event);
            var id = @event.Id ?? string.Empty;
            SessionContext.SetEntityStatistics(@event.Entity.GetType(), id.ToString());
        }
    }
}