using NHibernate.DependencyInjection;
using NHibernate.Glimpse.Test.Models;

namespace NHibernate.Glimpse.Test
{
    //rippo: this is the provider implementation for DI with NH
    public class EntityProvider : IEntityProvider
    {
        public object CreateInstance(System.Type type)
        {
            return type == typeof(Cat) ? new Cat("purrrr..... purrrrr.....") : null;
        }

        //rippo: I'm thinking of removing this because now a default constructor is always required,
        //even though you can overload constructors for DI
        public bool IsVaildWithoutDefaultConstructor(System.Type type)
        {
            return true;
        }
    }
}