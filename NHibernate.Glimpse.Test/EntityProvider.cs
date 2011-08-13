using NHibernate.DependencyInjection;
using NHibernate.Glimpse.Test.Models;

namespace NHibernate.Glimpse.Test
{
    public class EntityProvider : IEntityProvider
    {
        public object CreateInstance(System.Type type)
        {
            return type == typeof(Cat) ? new Cat() : null;
        }

        public bool IsVaildWithoutDefaultConstructor(System.Type type)
        {
            return false;
        }
    }
}