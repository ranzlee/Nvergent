namespace NHibernate.DependencyInjection
{
    public interface IEntityProvider
    {
        object CreateInstance(System.Type type);

        bool IsVaildWithoutDefaultConstructor(System.Type type);
    }
}