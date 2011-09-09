using System.Collections.Generic;
using System.Web;
using NHibernate.Glimpse.Core;

namespace NHibernate.Glimpse
{
    public class LoggerFactory : ILoggerFactory
    {
        private readonly IInternalLogger _sqlLogger = new SqlInternalLogger();
        private readonly IInternalLogger _batcherLogger = new BatcherInternalLogger();
        private readonly IInternalLogger _debugLogger = new DebugLogger();
        private readonly IInternalLogger _noLogger = new NoLogger();

        public IInternalLogger LoggerFor(string keyName)
        {
            if (keyName == null) return _noLogger;
            return keyName.ToLower().Trim() == "nhibernate.sql" ? _sqlLogger : _debugLogger;
        }

        public IInternalLogger LoggerFor(System.Type type)
        {
            //GetLog().Add(string.Format("<div>{0}</div>", type));
            if (type == null) return _noLogger;

            if (type == typeof(AdoNet.AbstractBatcher)) return _batcherLogger;

            //if (type == typeof(Impl.SessionImpl)
            //    || type == typeof(Transaction.ITransactionFactory)
            //    || type == typeof(Transaction.AdoTransaction)
            //    //|| type == typeof(AdoNet.AbstractBatcher)
            //    || type == typeof(AdoNet.ConnectionManager)
            //    || type == typeof(AdoNet.NonBatchingBatcher)
            //    || type == typeof(Driver.ReflectionBasedDriver)
            //    || type == typeof(Driver.NDataReader)
            //    || type == typeof(Driver.NHybridDataReader))
            //{
            //    return _debugLogger;
            //}
            return _noLogger;
        }

        internal static bool LogRequest()
        {
            var context = HttpContext.Current;
            if (context == null) return false;
            var cookie = context.Request.Cookies[Plugin.GlimpseCookie];
            return cookie != null;
        }

        internal static IList<string> GetLog()
        {
            var context = HttpContext.Current;
            if (context == null) return new List<string>();
            var l = (IList<string>) context.Items[Plugin.GlimpseLogKey];
            if (l == null)
            {
                l = new List<string>();
                var url = context.Request.Url;
                l.Add(string.Format("<div class='endpoint'>Endpoint: {0}</div>", HttpUtility.HtmlEncode(url.AbsolutePath)));
                context.Items.Add(Plugin.GlimpseLogKey, l);
            }
            return l;
        }
    }
}