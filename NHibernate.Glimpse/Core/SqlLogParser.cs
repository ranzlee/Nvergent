using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web;

namespace NHibernate.Glimpse.Core
{
    public static class SqlLogParser
    {
        private static string _stylesheet;
        private static string _script;
        private static readonly object Lock = new object();

        internal static RequestDebugInfo Transform(HttpContextBase context)
        {
            var selects = 0;
            var updates = 0;
            var deletes = 0;
            var inserts = 0;
            var batchCommands = 0;
            var events = (IList<SqlStatistic>)HttpContext.Current.Items[Plugin.GlimpseSqlStatsKey];
            if (events == null) return null;
            var url = context.Request.Url;
            var info = new RequestDebugInfo {GlimpseKey = Guid.NewGuid()};
            if (url != null) info.Url = url.AbsolutePath;
            foreach (var loggingEvent in events)
            {
                var detail = loggingEvent.Sql.TrimStart(' ', '\n', '\r');
                if (detail.StartsWith("select", StringComparison.OrdinalIgnoreCase)) selects++;
                if (detail.StartsWith("update", StringComparison.OrdinalIgnoreCase)) updates++;
                if (detail.StartsWith("delete", StringComparison.OrdinalIgnoreCase)) deletes++;
                if (detail.StartsWith("insert", StringComparison.OrdinalIgnoreCase)) inserts++;
                if (detail.StartsWith("batch commands:", StringComparison.OrdinalIgnoreCase)) batchCommands++;
                detail = string.Format("<pre class='brush: sql'>{0}</pre>", detail.Replace(", @p", ",\n\t@p"));
                info.Details.Add(new DebugInfoDetail{Description = detail, Timestamp = loggingEvent.Timestamp });
            }
            info.Selects = selects;
            info.Inserts = inserts;
            info.Updates = updates;
            info.Deletes = deletes;
            info.Batch = batchCommands;
            info.EntitiesLoaded = SessionContext.TotalEntitiesLoaded;
            info.Summary = string.Format("{0} selects | {1} updates | {2} inserts | {3} deletes | {4} batch",
                                            selects,
                                            updates,
                                            inserts,
                                            deletes,
                                            batchCommands);
            if (SessionContext.GetStatistics().Count > 0)
            {
                info.Summary += string.Format(" | {0} entities loaded", SessionContext.TotalEntitiesLoaded);
            }
            var sb = new StringBuilder();
            const string divId = "EntityIdDiv";
            if (SessionContext.GetStatistics().Count > 0)
            {
                foreach (var entityStat in SessionContext.GetStatistics())
                {
                    var entityIdDiv = divId + Guid.NewGuid().GetHashCode();
                    sb.AppendFormat("<div class='entityheader'><a href=\"javascript:toggle('{0}');\" class='entitylink'>{1}</a> - loaded {2}</div>", entityIdDiv, entityStat.Entity, entityStat.Count);
                    sb.AppendFormat("<div id='{0}' style='display : none'>", entityIdDiv);
                    var odd = false;
                    foreach (var id in entityStat.Ids)
                    {
                        var style = (odd) ? "oddentity" : "evenentity";
                        sb.AppendFormat("<div class='{0}'>{1}</div>", style, id);
                        odd = !odd;
                    }
                    sb.Append("</div>");
                }
                info.EntityDetails = sb.ToString();
            }
            return info;
        }
        
        internal static string GetCss()
        {
            if (_stylesheet != null) return _stylesheet;
            lock (Lock)
            {
                if (_stylesheet != null) return _stylesheet;
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("NHibernate.Glimpse.Content.GlimpseProfiler.css"))
                {
                    if (stream == null)
                    {
                        _stylesheet = string.Empty;
                    }
                    else
                    {
                        var css = new StreamReader(stream).ReadToEnd();
                        _stylesheet = css + Environment.NewLine;
                    }
                }
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("NHibernate.Glimpse.Content.SyntaxHighlighter.css"))
                {
                    if (stream != null)
                    {
                        var css = new StreamReader(stream).ReadToEnd();
                        _stylesheet += css;
                    }
                }
                _stylesheet = "<style type='text/css'>" + Environment.NewLine + _stylesheet + Environment.NewLine + "</style>";
                return _stylesheet;
            }
        }

        internal static string GetDebugInfo(RequestDebugInfo info)
        {
            var sb = new StringBuilder();
            sb.Append(@"<script type='text/javascript'>");
            if (_script == null)
            {
                lock (Lock)
                {
                    Thread.MemoryBarrier();
                    if (_script == null)
                    {
                        using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("NHibernate.Glimpse.Content.SyntaxHighlighterCore.js"))
                        {
                            _script = (stream == null) 
                                ? string.Empty 
                                : new StreamReader(stream).ReadToEnd();
                        }
                        using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("NHibernate.Glimpse.Content.SyntaxHighlighterBrushSql.js"))
                        {
                            if (stream != null)
                            {
                                _script += new StreamReader(stream).ReadToEnd();
                            }
                        }
                    }
                }
            }
            sb.Append(_script);
            sb.Append(@"SyntaxHighlighter.all();");
            sb.Append(@"function toggle(entityDiv) {");
            sb.Append(@"  var ele = document.getElementById(entityDiv);");
            sb.Append(@"  var visible = ele.getAttribute('style');");
            sb.Append(@"  if (visible === 'display : none') {");
            sb.Append(@"      ele.setAttribute('style', 'display : inline;');");
            sb.Append(@"  }else {");
            sb.Append(@"      ele.setAttribute('style', 'display : none');");    
            sb.Append(@"  }");
            sb.Append(@"}");
            sb.Append(@"</script>");
            sb.Append(@"<div>");
            sb.Append(GetDebugContent(info));    
            sb.Append(@"</div>");
            return sb.ToString();
        }

        internal static string GetDebugContent(RequestDebugInfo info)
        {
            var sb = new StringBuilder();
            if (info == null)
            {
                sb.Append("No debug information");
            }
            else
            {
                sb.AppendFormat(
                    "<div class='endpoint'>Endpoint: {0}: {1}</div>",
                    info.Url,
                    info.Summary);
                sb.Append(info.EntityDetails);
                foreach (var detail in info.Details)
                {
                    sb.AppendFormat("<div class='detail'>{1}</div><div class='metric'>{0}</div>",
                                    string.Format("{0}.{1}.{2}.{3}",
                                                    detail.Timestamp.Hour.ToString().PadLeft(2, '0'),
                                                    detail.Timestamp.Minute.ToString().PadLeft(2, '0'),
                                                    detail.Timestamp.Second.ToString().PadLeft(2, '0'),
                                                    detail.Timestamp.Millisecond.ToString().PadLeft(3, '0')),
                                    detail.Description);
                }
            }
            return sb.ToString();
        }
    }
}