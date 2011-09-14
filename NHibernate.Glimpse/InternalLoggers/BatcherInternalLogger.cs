using System;
using System.Collections.Generic;
using System.Web;
using NHibernate.Glimpse.Core;

namespace NHibernate.Glimpse.InternalLoggers
{
    internal class BatcherInternalLogger : IInternalLogger
    {
        public void DebugFormat(string format, params object[] args)
        {
            if (format == null) return;
            if (!LoggerFactory.LogRequest()) return;
            var context = HttpContext.Current;
            if (context == null) return;
            var l = (IList<LogStatistic>)context.Items[Plugin.GlimpseSqlStatsKey];
            if (l == null)
            {
                l = new List<LogStatistic>();
                context.Items.Add(Plugin.GlimpseSqlStatsKey, l);
            }
            l.Add(new LogStatistic
            {
                CommandNotification = string.Format(format.Trim().UppercaseFirst(), args),
            });
        }

        public void Debug(object message)
        {
            if (message == null) return;
            if (!LoggerFactory.LogRequest()) return;
            if (!message.ToString().Trim().ToLower().StartsWith("building an idbcommand object for the sqlstring")) return;
            var context = HttpContext.Current;
            if (context == null) return;
            var l = (IList<LogStatistic>)context.Items[Plugin.GlimpseSqlStatsKey];
            if (l == null)
            {
                l = new List<LogStatistic>();
                context.Items.Add(Plugin.GlimpseSqlStatsKey, l);
            }
            l.Add(new LogStatistic
            {
                CommandNotification = message.ToString().Trim().UppercaseFirst(),
                IsSqlString = true
            });
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