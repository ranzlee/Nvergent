namespace NHibernate.DependencyInjection.Core
{
    public class DefaultEntityProvider : IEntityProvider
    {
        public object CreateInstance(System.Type type)
        {
            return null;
        }

        public bool IsVaildWithoutDefaultConstructor(System.Type type)
        {
            return false;
        }
    }
}