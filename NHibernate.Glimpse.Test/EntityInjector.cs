using NHibernate.DependencyInjection;
using NHibernate.Glimpse.Test.Models;

namespace NHibernate.Glimpse.Test
{
    //rippo: this is the provider implementation for DI with NH - 
    //this is optional (not required for entities with default constructors)
    public class EntityInjector : IEntityInjector
    {
        private const string CatBehavior = "purrrr..... purrrrr.....";

        public object[] GetConstructorParameters(System.Type type)
        {
            if (type == typeof(Cat)) return new[] {CatBehavior};
            return null;
        }
    }
}