using System.Collections.Generic;

namespace NHibernate.DataAnnotations.Core
{
    public class EntityPersistenceContext
    {
        public static int Key { get { return typeof(EntityPersistenceContext).GetHashCode(); } }

        public bool ValidateProperties { get { return true; } }

        public object Id { get; internal set; }

        public string FactoryName { get; internal set; }

        public bool IsBeingAdded { get; internal set; }

        public bool IsBeingRemoved { get; internal set; }

        public bool IsBeingModified { get; internal set; }

        private IDictionary<string, object> _previousState = new Dictionary<string, object>();

        public IDictionary<string, object> PreviousState 
        {
            get { return _previousState; }
            internal set { _previousState = value; }
        } 
    }
}