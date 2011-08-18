namespace NHibernate.DependencyInjection
{
    public interface IEntityProvider
    {
        object[] GetConstructorParameters(System.Type type);
    }
}