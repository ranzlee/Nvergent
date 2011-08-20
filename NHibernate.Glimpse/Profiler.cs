using System.Collections.Generic;
using System.Linq;
using System.Web;
using NHibernate.Glimpse.Core;

namespace NHibernate.Glimpse
{
    public class Profiler : IHttpHandler
    {
        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            if (context == null) return;
            var key = context.Request.QueryString["key"];
            if (!string.IsNullOrWhiteSpace(key))
            {
                if (key.ToLower().Trim() == "help")
                {
                    ShowHelp(context);
                }
                else
                {
                    var show = context.Request.QueryString["show"];
                    if (show == "sql")
                    {
                        ShowSql(context, key);
                    }
                    else
                    {
                        var index = context.Request.QueryString["index"];
                        int i;
                        if (index != null && int.TryParse(index, out i))
                        {
                            switch (show)
                            {
                                case "debug":
                                    ShowLog(context, i);
                                    break;
                            }
                        }
                    }    
                }
            }
            context.Items.Add(Plugin.IngnoreResponseKey, true);
            context.Response.Flush();
            context.Response.End();
        }

        private static void ShowHelp(HttpContext context)
        {
            context.Response.Write("<html><head></head><body>");
            context.Response.Write("<h2 style='font-family: arial;'>NHibernate.Glimpse</h2>");
            context.Response.Write("<br />");
            context.Response.Write("<div style='font-family: arial;'>This plugin attempts to be as unobtrusive to NHibernate as possible. ");
            context.Response.Write("Therefore, the only NHibernate configuration automatically set is the ");
            context.Response.Write("LoggerFactory (in appsettings). This was primarily to avoid a dependency on Log4Net. ");
            context.Response.Write("By default, SQL and detail logging is provided, but other profile data can ");
            context.Response.Write("obtained with some minor configuration options and API calls.</div>");
            context.Response.Write("<br />");
            context.Response.Write("<div style='font-family: arial;'>Add this NHibernate property for proper SQL formatting.</div>");
            context.Response.Write("<div style='font-family: courier;'>&lt;property name=\"format_sql\"&gt;true&lt;/property&gt;</div>");
            context.Response.Write("<br/>");
            context.Response.Write("<div style='font-family: arial;'>Add this listener for entity load statistics. The listener extends from the default post load listener.</div>");
            context.Response.Write("<div style='font-family: courier;'>&lt;listener type=\"post-load\" class=\"NHibernate.Glimpse.EntityPostLoadListener, NHibernate.Glimpse\"/&gt;</div>");
            context.Response.Write("<div style='font-family: arial;'>Note: I've had mixed results with configuring the listener in the web.config. ");
            context.Response.Write("As an alternative, configure the listener in code prior to building your session factory.</div>");
            context.Response.Write("<div style='font-family: courier;'>config.SetListener(NHibernate.Event.ListenerType.PostLoad, new NHibernate.Glimpse.EntityPostLoadListener());</div>");
            context.Response.Write("<br />");
            context.Response.Write("<div style='font-family: arial;'>Add this NHibernate property to track factory statistics.</div>");
            context.Response.Write("<div style='font-family: courier;'>&lt;property name=\"generate_statistics\"&gt;true&lt;/property&gt;<div>");
            context.Response.Write("<div style='font-family: arial;'>You also need to register the factory after building it.</div>");
            context.Response.Write("<div style='font-family: courier;'>NHibernate.Glimpse.Plugin.RegisterSessionFactory(YourSessionFactory);<div>");
            context.Response.Write("<br />");
            context.Response.Write("<div style='font-family: arial;'>For persistent logging (i.e. so that the log is not reset after each request), set this property.</div>");
            context.Response.Write("<div style='font-family: courier;'>NHibernate.Glimpse.Plugin.KeepLogHistory = true;</div>");
            context.Response.Write("<br />");
            context.Response.Write("</body></html>");
        }

        private static void ShowSql(HttpContext context, string key)
        {
            if (context == null) return;
            var cookie = context.Request.Cookies[Plugin.GlimpseCookie];
            if (cookie == null) return;
            RequestDebugInfo info = null;
            IList<RequestDebugInfo> infos;
            Plugin.Statistics.TryGetValue(cookie.Value, out infos);
            if (infos != null)
            {
                info = infos.Where(i => i.GlimpseKey.ToString() == key).FirstOrDefault();
            }
            if (info == null) info = new RequestDebugInfo();
            context.Response.Write(string.Format("<html><head>{0}</head><body>{1}</body></html>",
                                                 Core.Profiler.GetCss(),
                                                 Core.Profiler.GetDebugInfo(info)));
        }

        private static void ShowLog(HttpContext context, int index)
        {
            if (context == null) return;
            var cookie = context.Request.Cookies[Plugin.GlimpseCookie];
            if (cookie == null) return;
            IList<IList<string>> logs;
            Plugin.Logs.TryGetValue(cookie.Value, out logs);
            IList<string> log = null;
            if (logs != null)
            {
                log = logs[index];
            }
            if (log == null) log = new List<string>();
            context.Response.Write("<html><head><style> body { margin: 0; font-family: Arial; font-size: 10px; } .endpoint { padding-top: 10px; font-size: 12pt; font-weight: bold; color: #6666FF; background-color: #E5E5E5; border-bottom-style: solid; border-bottom-width: 1px; border-bottom-color: #808080; } div{ padding-top: 10px; } .infomark{ background-color: #CCFF99; }</style></head><body style='font-family:Arial;font-size:10pt;'>");
            foreach (var item in log)
            {
                context.Response.Write(item);
            }
            context.Response.Write("</body></html>");
        }
    }
}
