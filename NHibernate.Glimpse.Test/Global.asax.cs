using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace NHibernate.Glimpse.Test
{
    public class MvcApplication : HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

        }

        void Application_BeginRequest(Object source, EventArgs e)
        {
            var app = (HttpApplication)source;
            var context = app.Context;
            // Attempt to peform first request initialization
            FirstRequestInitialization.Initialize(context);
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }

        void Application_EndRequest(Object source, EventArgs e)
        {
            
            UnitOfWork.Commit();
            UnitOfWork.End();    
            
        }
    }
}