using System.Web;
using NHibernate.Connection;
using NHibernate.Glimpse.Core;
using NHibernate.Transaction;

namespace NHibernate.Glimpse
{
    public class LoggerFactory : ILoggerFactory
    {
        private readonly IInternalLogger _connectionLogger = new ConnectionInternalLogger();
        private readonly IInternalLogger _transactionLogger = new TransactionInternalLogger();
        private readonly IInternalLogger _sqlLogger = new SqlInternalLogger();
        private readonly IInternalLogger _batcherLogger = new BatcherInternalLogger();
        private readonly IInternalLogger _noLogger = new NoLogger();

        public IInternalLogger LoggerFor(string keyName)
        {
            if (keyName == null) return _noLogger;
            return keyName.ToLower().Trim() == "nhibernate.sql" ? _sqlLogger : _noLogger;
        }

        public IInternalLogger LoggerFor(System.Type type)
        {
            if (type == null) return _noLogger;
            if (type == typeof(AdoNet.AbstractBatcher)) return _batcherLogger;
            if (type == typeof(ConnectionProvider)) return _connectionLogger;
            if (type == typeof(DriverConnectionProvider)) return _connectionLogger;
            if (type == typeof(AdoTransaction)) return _transactionLogger;
            return _noLogger;
        }

        internal static bool LogRequest()
        {
            var context = HttpContext.Current;
            if (context == null) return false;
            var cookie = context.Request.Cookies[Plugin.GlimpseCookie];
            return cookie != null;
        }
    }
}