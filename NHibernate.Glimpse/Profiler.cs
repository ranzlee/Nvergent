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
            context.Items.Add(Plugin.IngnoreResponseKey, true);
            context.Response.Flush();
            context.Response.End();
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
