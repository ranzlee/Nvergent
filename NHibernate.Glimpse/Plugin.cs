using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using Glimpse.Core.Extensibility;
using NHibernate.Glimpse.Core;
using NHibernate.Impl;

namespace NHibernate.Glimpse
{
    [GlimpsePlugin(SessionRequired = true, ShouldSetupInInit = true)]
    public class Plugin : IGlimpsePlugin, IProvideGlimpseHelp//, IProvideGlimpseStructuredLayout
    {
        private static readonly object Lock = new object();
        internal static readonly IList<ISessionFactory> SessionFactories = new List<ISessionFactory>(); 
        internal const string GlimpseCookie = "NHibernate.Glimpse";
        internal const string GlimpseSqlStatsKey = "NHibernate.Glimpse.Sql.Stats";
        internal const string GlimpseEntityLoadStatsKey = "NHibernate.Glimpse.Entity.Load.Stats";
        internal const string GlimpseLogKey = "NHibernate.Glimpse.Log";
        internal const string IngnoreResponseKey = "NHibernate.Glimpse.Ignore.Response";
        internal const string IsBeingRedirectedKey = "NHibernate.Glimpse.Response.Redirect"; 
        internal static readonly ConcurrentDictionary<string, IList<RequestDebugInfo>> Statistics = new ConcurrentDictionary<string, IList<RequestDebugInfo>>();
        internal static readonly ConcurrentDictionary<string, IList<IList<string>>> Logs = new ConcurrentDictionary<string, IList<IList<string>>>();

        public object GetData(HttpContextBase context)
        {
            if (context == null) return string.Empty;
            var path = (string.IsNullOrWhiteSpace(context.Request.ApplicationPath))
                               ? string.Empty
                               : context.Request.ApplicationPath.TrimEnd(new[] { '/' });
            var cookie = GetCookie(context);
            var stat = Core.SqlLogParser.Transform(context);
            if (stat == null) return string.Empty;
            var stats = Statistics.GetOrAdd(cookie.Value, new List<RequestDebugInfo>());
            var log = (context.Items[GlimpseLogKey] == null) ? new List<string>() : (IList<string>)context.Items[GlimpseLogKey];
            if (log == null) return string.Empty;
            var logs = Logs.GetOrAdd(cookie.Value, new List<IList<string>>());
            ProcessTransientLog(context, stats, logs);
            stats.Add(stat);
            logs.Add(log);
            var data = new List<object[]>();
            var columns = new List<object> { "Request", "Selects", "Inserts", "Updates", "Deletes", "Batch Commands" };
            if (SessionContext.GetStatistics().Count > 0) columns.Add("Entities Loaded");
            columns.Add("SQL");
            columns.Add("Details");
            data.Add(columns.ToArray());
            var i = stats.Count - 1;
            foreach (var item in stats.Reverse())
            {
                var values = new List<object>
                            {
                                item.Url,
                                item.Selects,
                                item.Inserts,
                                item.Updates,
                                item.Deletes,
                                item.Batch
                            };
                if (SessionContext.GetStatistics().Count > 0)
                {
                    values.Add(item.EntitiesLoaded);
                }
                values.Add(string.Format("!<a href='{0}/nhibernate.glimpse.axd?key={1}&show=sql' target='_blank'>SQL</a>!", path, item.GlimpseKey));
                values.Add(string.Format("!<a href='{0}/nhibernate.glimpse.axd?key={1}&show=debug&index={2}' target='_blank'>Details</a>!", path, item.GlimpseKey, i));
                data.Add(values.ToArray());
                i -= 1;
            }
            object[] factoryHeader = null;
            var factoryData = new List<object> {new object[] {"Key", "Value"}};
            lock (Lock)
            {
                if (!SessionFactories.Any(f => f.Statistics.IsStatisticsEnabled)) return data;
                foreach (var sessionFactory in SessionFactories)
                {
                    factoryData.Add(new object[] { "Close Statement Count", sessionFactory.Statistics.CloseStatementCount });
                    factoryData.Add(new object[] { "Collection Fetch Count", sessionFactory.Statistics.CollectionFetchCount });
                    factoryData.Add(new object[] { "Collection Load Count", sessionFactory.Statistics.CollectionLoadCount });
                    factoryData.Add(new object[] { "Collection Recreate Count", sessionFactory.Statistics.CollectionRecreateCount });
                    factoryData.Add(new object[] { "Collection Remove Count", sessionFactory.Statistics.CollectionRemoveCount });
                    factoryData.Add(new object[] { "Collection Role Names", sessionFactory.Statistics.CollectionRoleNames });
                    factoryData.Add(new object[] { "Collection Update Count", sessionFactory.Statistics.CollectionUpdateCount });
                    factoryData.Add(new object[] { "Connect Count", sessionFactory.Statistics.ConnectCount });
                    factoryData.Add(new object[] { "Entity Delete Count", sessionFactory.Statistics.EntityDeleteCount });
                    factoryData.Add(new object[] { "Entity Fetch Count", sessionFactory.Statistics.EntityFetchCount });
                    factoryData.Add(new object[] { "Entity Insert Count", sessionFactory.Statistics.EntityInsertCount });
                    factoryData.Add(new object[] { "Entity Load Count", sessionFactory.Statistics.EntityLoadCount });
                    factoryData.Add(new object[] { "Entity Names", sessionFactory.Statistics.EntityNames });
                    factoryData.Add(new object[] { "Entity Update Count", sessionFactory.Statistics.EntityUpdateCount });
                    factoryData.Add(new object[] { "Flush Count", sessionFactory.Statistics.FlushCount });
                    factoryData.Add(new object[] { "Optimistic Failure Count", sessionFactory.Statistics.OptimisticFailureCount });
                    factoryData.Add(new object[] { "Prepare Statement Count", sessionFactory.Statistics.PrepareStatementCount });
                    factoryData.Add(new object[] { "Queries", sessionFactory.Statistics.Queries });
                    factoryData.Add(new object[] { "Query Cache Hit Count", sessionFactory.Statistics.QueryCacheHitCount });
                    factoryData.Add(new object[] { "Query Cache Miss Count", sessionFactory.Statistics.QueryCacheMissCount });
                    factoryData.Add(new object[] { "Query Cache Put Count", sessionFactory.Statistics.QueryCachePutCount });
                    factoryData.Add(new object[] { "Query Execution Count", sessionFactory.Statistics.QueryExecutionCount });
                    factoryData.Add(new object[] { "Query Execution Max Time", sessionFactory.Statistics.QueryExecutionMaxTime });
                    factoryData.Add(new object[] { "Query Execution Max Time QueryString", sessionFactory.Statistics.QueryExecutionMaxTimeQueryString });
                    factoryData.Add(new object[] { "Second Level Cache Hit Count", sessionFactory.Statistics.SecondLevelCacheHitCount });
                    factoryData.Add(new object[] { "Second Level Cache Miss Count", sessionFactory.Statistics.SecondLevelCacheMissCount });
                    factoryData.Add(new object[] { "Second Level Cache Put Count", sessionFactory.Statistics.SecondLevelCachePutCount });
                    factoryData.Add(new object[] { "Second Level Cache Region Names", sessionFactory.Statistics.SecondLevelCacheRegionNames });
                    factoryData.Add(new object[] { "Session Close Count", sessionFactory.Statistics.SessionCloseCount });
                    factoryData.Add(new object[] { "Session Open Count", sessionFactory.Statistics.SessionOpenCount });
                    factoryData.Add(new object[] { "Start Time", sessionFactory.Statistics.StartTime });
                    factoryData.Add(new object[] { "Successful Transaction Count", sessionFactory.Statistics.SuccessfulTransactionCount });
                    factoryData.Add(new object[] { "Transaction Count", sessionFactory.Statistics.TransactionCount });
                    var impl = sessionFactory as SessionFactoryImpl;
                    factoryHeader = new object[] { string.Format("Factory: {0}", (impl == null) ? string.Empty : impl.Uuid), factoryData.ToArray() };
                }    
            }
            var mainStatisticsData = new object[] { "Statistics", factoryHeader };
            var mainLogData = new object[] { "Log", data };
            var mainHeader = new object[] { "Key", "Value" };
            var main = new object[] { mainHeader, mainStatisticsData, mainLogData };
            return main;
        }

        public void SetupInit()
        {
            
        }

        public string Name
        {
            get { return "NHibernate"; }
        }

        /// <summary>
        /// Register an ISessionFactory for statistics logging
        /// NOTE: This requires the property generate_statistics = true
        /// </summary>
        /// <param name="sessionFactory">ISessionFactory</param>
        public static void RegisterSessionFactory(ISessionFactory sessionFactory)
        {
            if (sessionFactory == null) throw new NullReferenceException("sessionFactory");
            lock (Lock)
            {
                if (!SessionFactories.Contains(sessionFactory)) SessionFactories.Add(sessionFactory);    
            }
        }

        public static bool KeepLogHistory { get; set; }

        private static HttpCookie GetCookie(HttpContextBase context)
        {
            var cookie = context.Request.Cookies[GlimpseCookie];
            if (cookie == null)
            {
                cookie = new HttpCookie(GlimpseCookie, Guid.NewGuid().ToString()) { HttpOnly = true };
                context.Request.Cookies.Add(cookie);
            }
            var ignoreResponse = context.Items[IngnoreResponseKey];
            if (ignoreResponse == null || !bool.Parse(ignoreResponse.ToString()))
            {
                context.Response.Cookies.Add(cookie);
            }
            return cookie;
        }

        private static void ProcessTransientLog(HttpContextBase context, ICollection<RequestDebugInfo> stats, ICollection<IList<string>> logs)
        {
            if (KeepLogHistory) return;
            if (context.Session == null || context.Session.Mode == SessionStateMode.Off)
            {
                stats.Clear();
                logs.Clear();
                return;
            }
            if (context.Request.Params["x-requested-with"] != null && context.Request.Params["x-requested-with"].ToUpper().Trim() == "XMLHTTPREQUEST")
            {
                return;
            }
            if (context.Session[IsBeingRedirectedKey] == null)
            {
                stats.Clear();
                logs.Clear();
            }
            else
            {
                context.Session[IsBeingRedirectedKey] = null;
            }
            if (context.Response.IsRequestBeingRedirected) context.Session[IsBeingRedirectedKey] = true;
        }

        public string HelpUrl
        {
            get
            {
                var path = (string.IsNullOrWhiteSpace(HttpContext.Current.Request.ApplicationPath))
                               ? string.Empty
                               : HttpContext.Current.Request.ApplicationPath.TrimEnd(new[] { '/' });
                return string.Format("{0}/nhibernate.glimpse.axd?key=help", path);
            }
        }

        public GlimpseStructuredLayout StructuredLayout
        {
            get
            {
                var cell = new GlimpseStructuredLayoutCell();
                cell.Data = "select * from entity where id = @id";
                //cell.Prefix = "my prefix";
                cell.IsCode = true;
                cell.CodeType = "SQL";
                //cell.Structure = "<a href='#'>Hello World</a>";
                cell.ClassName = "glimpse-trigger";
                var section = new GlimpseStructuredLayoutSection();
                section.Add(cell);
                var layout = new GlimpseStructuredLayout();
                layout.Add(section);
                return layout;
            }
        }
    }
}