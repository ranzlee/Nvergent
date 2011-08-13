using System.Web;
using NHibernate.Cfg;
using NHibernate.DependencyInjection;
using NHibernate.Event;
using NHibernate.Glimpse.Test.Models;
using NHibernate.Tool.hbm2ddl;

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

                Initializer.RegisterEntityProvider(new EntityProvider());

                var config = new Configuration();
                config.AddClass(typeof (Cat));

                var tool = new SchemaExport(config);
                tool.Execute(false, true, false);

                config.SetListener(ListenerType.PostLoad, new EntityPostLoadListener());
                LoggerProvider.SetLoggersFactory(new LoggerFactory());
                
                MvcApplication.SessionFactory = config.BuildSessionFactory();
                Plugin.RegisterSessionFactory(MvcApplication.SessionFactory);
                //Plugin.KeepLogHistory = false;

                _initializedAlready = true;
            }
        }
    }
}