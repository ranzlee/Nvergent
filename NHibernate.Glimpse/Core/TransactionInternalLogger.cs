using System;
using System.Collections.Generic;
using System.Web;

namespace NHibernate.Glimpse.Core
{
    internal class TransactionInternalLogger : IInternalLogger
    {
        public void Debug(object message)
        {
            if (message == null) return;
            if (!LoggerFactory.LogRequest()) return;
            var context = HttpContext.Current;
            if (context == null) return;
            var l = (IList<LogStatistic>)context.Items[Plugin.GlimpseSqlStatsKey];
            if (l == null)
            {
                l = new List<LogStatistic>();
                context.Items.Add(Plugin.GlimpseSqlStatsKey, l);
            }
            var timestamp = DateTime.Now;
            l.Add(new LogStatistic
                      {
                          TransactionNotification =
                              string.Format("{0}{1}", message.ToString().Trim().UppercaseFirst(),
                                            string.Format(" @ {0}.{1}.{2}.{3}",
                                                          timestamp.Hour.ToString().PadLeft(2, '0'),
                                                          timestamp.Minute.ToString().PadLeft(2, '0'),
                                                          timestamp.Second.ToString().PadLeft(2, '0'),
                                                          timestamp.Millisecond.ToString().PadLeft(3, '0')))
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