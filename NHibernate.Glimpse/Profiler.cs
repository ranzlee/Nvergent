using System.Collections.Generic;
using System.Linq;
using System.Web;
using NHibernate.Glimpse.Core;

namespace NHibernate.Glimpse
{
    public class Profiler : IHttpHandler
    {
        public const string Key = "key";
        public const string Help = "help";

        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            if (context == null) return;
            var key = context.Request.QueryString[Key];
            if (!string.IsNullOrWhiteSpace(key))
            {
                switch (key.ToLower().Trim())
                {
                    case Help:
                        ShowHelp(context);
                        break;
                    default:
                        ShowDetails(context, key);
                        break;
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
            context.Response.Write("<div style='font-family: arial;'>This plugin attempts to be as unobtrusive to NHibernate as possible. ");
            context.Response.Write("The only NHibernate configuration automatically overridden is the ");
            context.Response.Write("LoggerFactory (in appSettings). This was primarily to avoid a dependency on Log4Net. ");
            context.Response.Write("By default, SQL, IDbCommand, IDataReader, IDbConnection, IDbTransaction, flush, and entity load logging is provided. Additional profile data is ");
            context.Response.Write("available with some minor configuration options and API calls.</div>");
            context.Response.Write("<br />");
            context.Response.Write("<div style='font-family: arial;'>Add this appSettings key and value to enable the logging sinks.</div>");
            context.Response.Write("<div style='font-family: courier;'>&lt;add key=\"NHibernate.Glimpse.Loggers\" value=\"command,connection,flush,load,transaction\" /&gt;</div>");
            context.Response.Write("<br />");
            context.Response.Write("<div style='font-family: arial;'>Add this NHibernate property to format SQL.</div>");
            context.Response.Write("<div style='font-family: courier;'>&lt;property name=\"format_sql\"&gt;true&lt;/property&gt;</div>");
            context.Response.Write("<br/>");
            context.Response.Write("<div style='font-family: arial;'>Add this listener for additional entity load statistics. The listener extends from the default post load listener.</div>");
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
            context.Response.Write("<div style='font-family: arial;'><strong>Important additional usage notes:</strong> Binding entities directly to MVC views can cause problems with Glimpse. ");
            context.Response.Write("This is because Glimpse can cause lazy loaded collections to load as a result of view model inspection. ");
            context.Response.Write("I strongly recommend that you never bind an entity directly to a view - use a view model instead. ");
            context.Response.Write("This is a better design choice IMO, regardless of Glimpse. ");
            context.Response.Write("If you must bind your entities to your views and you notice performance problems when using Glimpse, blacklist the Views plugin (see the Glimpse readme for details).</div>");
            context.Response.Write("<div style='font-family: courier;'>&lt;pluginBlacklist&gt;...&lt;add plugin=\"Glimpse.Mvc3.Plugin.Views\"/&gt;...</div>");
            context.Response.Write("<br />");
            context.Response.Write("<div style='font-family: arial;'>Also, depending on how the NHibernate unit of work is implemented in your application, you might notice that ");
            context.Response.Write("the log is missing information (i.e. the last command exection, transaction commit, or connection close).  This is ");
            context.Response.Write("a timing issue and is outside of the plugin's control.  If you notice this, you can flush the ISession prior to ending the request ");
            context.Response.Write("to atleast get the SQL, command, reader, and flush statistics.");
            context.Response.Write("</div>");
            context.Response.Write("<br />");
            context.Response.Write("<div style='font-family: arial;'>Please remember that async request statistics are available by clicking \"Launch\" for the request ");
            context.Response.Write("in the Glimpse \"Ajax\" tab, then returning to the \"NHibernate\" tab. Redirect statistics are available by going to the ");
            context.Response.Write("\"Remote\" tab in Glimpse, clicking \"Launch\" for the client, find the request that was redirected and click \"Launch\", ");
            context.Response.Write("then return to the \"NHibernate\" tab.");
            context.Response.Write("</div>");
            context.Response.Write("<br />");
            context.Response.Write("<div style='font-family: arial;'>Mail me directly @ <a href='mailo:randy.w.lee@gmail.com?subject=NHibernate.Glimpse'>randy.w.lee@gmail.com</a> (subject: NHibernate.Glimpse) if you have questions or to report any bugs. Thanks!</div>");
            context.Response.Write("</body></html>");
        }

        private static void ShowDetails(HttpContext context, string key)
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
                                                 LogParser.GetCss(),
                                                 LogParser.GetDebugInfo(info)));
        }
    }
}
