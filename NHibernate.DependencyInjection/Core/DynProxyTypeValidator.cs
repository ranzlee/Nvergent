namespace NHibernate.DependencyInjection.Core
{
    internal class DynProxyTypeValidator : Proxy.DynProxyTypeValidator
    {
        protected override bool HasVisibleDefaultConstructor(System.Type type)
        {
            return base.HasVisibleDefaultConstructor(type) || BytecodeProvider.EntityInjector.IsVaildWithoutDefaultConstructor(type);
        }
    }
}