using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using NHibernate.AdoNet;
using NHibernate.Connection;
using NHibernate.Engine;
using NHibernate.Event.Default;
using NHibernate.Glimpse.InternalLoggers;
using NHibernate.Impl;
using NHibernate.Transaction;

namespace NHibernate.Glimpse
{
    public class LoggerFactory : ILoggerFactory
    {
        private readonly IInternalLogger _sessionInternalLogger = new SessionInternalLogger();
        private readonly IInternalLogger _connectionLogger = new ConnectionInternalLogger();
        private readonly IInternalLogger _flushLogger = new FlushInternalLogger();
        private readonly IInternalLogger _loadLogger = new LoadInternalLogger();
        private readonly IInternalLogger _transactionLogger = new TransactionInternalLogger();
        private readonly IInternalLogger _sqlLogger = new SqlInternalLogger();
        private readonly IInternalLogger _batcherLogger = new BatcherInternalLogger();
        private readonly IInternalLogger _noLogger = new NoLogger();
        private static readonly IList<string> Loggers = new List<string>();

        public static bool HasCommandLogger
        {
            get { return Loggers.Contains("command"); }
        }

        public static bool HasConnectionLogger
        {
            get { return Loggers.Contains("connection"); }
        }

        public static bool HasFlushLogger
        {
            get { return Loggers.Contains("flush"); }
        }

        public static bool HasLoadLogger
        {
            get { return Loggers.Contains("load"); }
        }

        public static bool HasTransactionLogger
        {
            get { return Loggers.Contains("transaction"); }
        }

        public LoggerFactory()
        {
            if (!ConfigurationManager.AppSettings.AllKeys.Contains("NHibernate.Glimpse.Loggers")) return;
            var reader = new AppSettingsReader();
            var loggersString = reader.GetValue("NHibernate.Glimpse.Loggers", typeof(string));
            if (loggersString == null) return;
            var loggers = loggersString
                .ToString()
                .Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                .ToList();
            foreach (var logger in loggers)
            {
                Loggers.Add(logger.Trim().ToLower());
            }
        }

        public IInternalLogger LoggerFor(string keyName)
        {
            if (keyName == null) return _noLogger;
            return keyName.ToLower().Trim() == "nhibernate.sql" ? _sqlLogger : _noLogger;
        }

        public IInternalLogger LoggerFor(System.Type type)
        {
            if (type == null) return _noLogger;
            return GetLogger(type);
        }

        private IInternalLogger GetLogger(System.Type logger)
        {
            //if (logger == typeof(SessionImpl))
            //{
            //    return _sessionInternalLogger;
            //}

            if (logger == typeof(AbstractBatcher))
            {
                if (HasCommandLogger) return _batcherLogger;
            }
            if (logger == typeof(ConnectionProvider))
            {
                if (HasConnectionLogger) return _connectionLogger;
            }
            if (logger == typeof(DriverConnectionProvider))
            {
                if (HasConnectionLogger) return _connectionLogger;
            }
            if (logger == typeof(AdoTransaction))
            {
                if (HasTransactionLogger) return _transactionLogger;
            }
            if (logger == typeof(TwoPhaseLoad))
            {
                if (HasLoadLogger) return _loadLogger;
            }
            if (logger == typeof(AbstractFlushingEventListener))
            {
                if (HasFlushLogger) return _flushLogger;
            }
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