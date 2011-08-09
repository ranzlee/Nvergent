using System.Web;
using NHibernate.Event;
using NHibernate.Glimpse.Test.Models;

namespace NHibernate.Glimpse.Test
{
    class FirstRequestInitialization
    {
        private static bool _initializedAlready;
        private static readonly object Lock = new object();

        public static void Initialize(HttpContext context)
        {
            if (_initializedAlready)
            {
                return;
            }
            lock (Lock)
            {
                if (_initializedAlready) return;
                var config = new Cfg.Configuration();
                config.AddClass(typeof (Cat));

                config.SetListener(ListenerType.PostLoad, new EntityPostLoadListener());
                LoggerProvider.SetLoggersFactory(new LoggerFactory());
                
                MvcApplication.SessionFactory = config.BuildSessionFactory();
                Plugin.RegisterSessionFactory(MvcApplication.SessionFactory);

                _initializedAlready = true;
            }
        }
    }
}