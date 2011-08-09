using System;
using System.Collections.Generic;
using System.Web;

namespace NHibernate.Glimpse.Core
{
    internal class SqlInternalLogger : IInternalLogger
    {
        public void Debug(object message)
        {
            if (message == null) return;
            if (!LoggerFactory.LogRequest()) return;
            var context = HttpContext.Current;
            if (context == null) return;
            var l = (IList<SqlStatistic>)context.Items[Plugin.GlimpseSqlStatsKey];
            if (l == null)
            {
                l = new List<SqlStatistic>();
                context.Items.Add(Plugin.GlimpseSqlStatsKey, l);
            }
            l.Add(new SqlStatistic { Sql = message.ToString(), Timestamp = DateTime.Now });
        }

        public void Error(object message)
        {

        }

        public void Error(object message, Exception exception)
        {

        }

        public void ErrorFormat(string format, params object[] args)
        {

        }

        public void Fatal(object message)
        {

        }

        public void Fatal(object message, Exception exception)
        {

        }

        public void Debug(object message, Exception exception)
        {

        }

        public void DebugFormat(string format, params object[] args)
        {

        }

        public void Info(object message)
        {

        }

        public void Info(object message, Exception exception)
        {

        }

        public void InfoFormat(string format, params object[] args)
        {

        }

        public void Warn(object message)
        {

        }

        public void Warn(object message, Exception exception)
        {

        }

        public void WarnFormat(string format, params object[] args)
        {

        }

        public bool IsErrorEnabled
        {
            get { return false; }
        }

        public bool IsFatalEnabled
        {
            get { return false; }
        }

        public bool IsDebugEnabled
        {
            get { return LoggerFactory.LogRequest(); }
        }

        public bool IsInfoEnabled
        {
            get { return false; }
        }

        public bool IsWarnEnabled
        {
            get { return false; }
        }
    }
}