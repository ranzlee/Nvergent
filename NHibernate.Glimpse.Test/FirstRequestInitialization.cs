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

                //rippo: this is the initializer for the bytecodeprovider
                Initializer.RegisterBytecodeProvider(new EntityInjector());

                //rippo: if you don't care to use DI and all of your entities have default constructors, 
                //just use the overload initializer
                //Initializer.RegisterEntityProvider();

                var config = new Configuration();
                config.AddClass(typeof (Cat));

                var tool = new SchemaExport(config);
                tool.Execute(false, true, false);

                config.SetListener(ListenerType.PostLoad, new EntityPostLoadListener());
                LoggerProvider.SetLoggersFactory(new LoggerFactory());
                
                MvcApplication.SessionFactory = config.BuildSessionFactory();
                Plugin.RegisterSessionFactory(MvcApplication.SessionFactory);
                Plugin.KeepLogHistory = true;

                _initializedAlready = true;
            }
        }
    }
}