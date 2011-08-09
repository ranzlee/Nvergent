using System.Collections.Generic;

namespace NHibernate.Glimpse.Core
{
    public class EntityLoadedStatistic
    {
        private readonly System.Type _entity;
        private long _count = 1;
        private readonly IList<string> _ids = new List<string>();
        
        public System.Type Entity
        {
            get { return _entity; }
        }

        public long Count
        {
            get { return _count; }
            set { _count = value; }
        }

        public IList<string> Ids
        {
            get { return _ids; }
        }

        internal EntityLoadedStatistic(System.Type entity, string id)
        {
            if (entity == null) entity = typeof(object);
            if (id == null) id = string.Empty;
            _entity = entity;
            _ids.Add(id);
        }

        internal void Increment(string id)
        {
            if (id == null) id = string.Empty;
            _ids.Add(id);
            _count += 1;
        }
    }
}