using System;
using System.Threading;
using System.Web;
using NHibernate.Cfg;
using NHibernate.Context;

namespace NHibernate.Session
{
    public class Marshaler
    {
        private readonly object _lock = new object();

        private readonly Configuration _configuration;

        private readonly System.Type _sessionInterceptor;

        private ISessionFactory _factory;

        public event SessionFactoryCreated OnSessionFactoryCreated;

        public Marshaler(Configuration configuration, System.Type sessionInterceptor)
        {
            _configuration = configuration;
            _sessionInterceptor = sessionInterceptor;
        }

        public Marshaler(Configuration configuration) : this(configuration, null) { }

        public bool HasSession
        {
            get { return _factory != null && CurrentSessionContext.HasBind(_factory); }
        }

        public ISession CurrentSession
        {
            get
            {
                if (_factory == null)
                {
                    _factory = (HttpContext.Current != null) ? GetFactory<WebSessionContext>() : GetFactory<ThreadStaticSessionContext>();
                }
                if (CurrentSessionContext.HasBind(_factory)) return _factory.GetCurrentSession();
                var session = (_sessionInterceptor == null) ? _factory.OpenSession() : _factory.OpenSession((IInterceptor)Activator.CreateInstance(_sessionInterceptor));
                session.BeginTransaction();
                CurrentSessionContext.Bind(session);
                return session;
            }
        }

        private ISessionFactory GetFactory<T>() where T : ICurrentSessionContext
        {
            if (_factory == null)
            {
                lock (_lock)
                {
                    Thread.MemoryBarrier();
                    if (_factory == null)
                    {
                        _factory = _configuration.CurrentSessionContext<T>().BuildSessionFactory();
                        var sessionFactoryCreated = OnSessionFactoryCreated;
                        if (sessionFactoryCreated != null)
                        {
                            sessionFactoryCreated.Invoke(this, new SessionFactoryCreatedArgs(_factory));
                        }
                    }
                }
            }
            return _factory;
        }

        public void Commit()
        {
            if (_factory == null) return;
            if (!CurrentSessionContext.HasBind(_factory)) return;
            try
            {
                _factory.GetCurrentSession().Transaction.Commit();
                if (HttpContext.Current == null)
                {
                    _factory.GetCurrentSession().Transaction.Begin();
                }
            }
            catch (Exception)
            {
                _factory.GetCurrentSession().Transaction.Rollback();
                var session = CurrentSessionContext.Unbind(_factory);
                session.Close();
                throw;
            }
        }

        public void End()
        {
            if (_factory == null) return;
            if (!CurrentSessionContext.HasBind(_factory)) return;
            var session = CurrentSessionContext.Unbind(_factory);
            session.Close();
        }
    }
}