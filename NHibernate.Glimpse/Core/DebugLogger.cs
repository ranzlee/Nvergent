using System;
using System.Linq;
using System.Web;

namespace NHibernate.Glimpse.Core
{
    internal class DebugLogger : IInternalLogger
    {
        public void Debug(object message)
        {
            if (message == null) return;
            if (!LoggerFactory.LogRequest()) return;
            LoggerFactory.GetLog().Add(string.Format("<div>{0}</div>", HttpUtility.HtmlEncode(message)));
        }

        public void Debug(object message, Exception exception)
        {
            if (message == null) return;
            if (!LoggerFactory.LogRequest()) return;
            LoggerFactory.GetLog().Add(string.Format("<div>{0}</div>", HttpUtility.HtmlEncode(string.Format("{0} : {1}", message, (exception == null) ? "null exception" : exception.Message))));
        }

        public void DebugFormat(string format, params object[] args)
        {
            if (format == null) return;
            if (!LoggerFactory.LogRequest()) return;
            var localArgs = args.Select(arg => arg ?? string.Empty).ToList();
            LoggerFactory.GetLog().Add(string.Format("<div class='infomark'>{0}</div>", HttpUtility.HtmlEncode(string.Format(format, localArgs.ToArray()))));
        }

        public void Error(object message) { }

        public void Error(object message, Exception exception) { }
        
        public void ErrorFormat(string format, params object[] args) { }
        
        public void Fatal(object message) { }
        
        public void Fatal(object message, Exception exception) { }
        
        public void Info(object message) { }
        
        public void Info(object message, Exception exception) { }
        
        public void InfoFormat(string format, params object[] args) { }
        
        public void Warn(object message) { }
        
        public void Warn(object message, Exception exception) { }
        
        public void WarnFormat(string format, params object[] args) { }

        public bool IsDebugEnabled { get { return LoggerFactory.LogRequest(); } }

        public bool IsErrorEnabled { get { return false; } }

        public bool IsFatalEnabled { get { return false; } }

        public bool IsInfoEnabled { get { return false; } }

        public bool IsWarnEnabled { get { return false; } }
    }
}