using NHibernate.DataAnnotations;

// ReSharper disable CheckNamespace
namespace NHibernate
// ReSharper restore CheckNamespace
{
    public static class Extensions
    {
        public static ISessionValidator GetValidator(this ISession session)
        {
            var interceptor = session.GetSessionImplementation().Interceptor as ValidationInterceptor;
            if (interceptor == null) return null;
            return interceptor.GetSessionAuditor();
        }
    }
}