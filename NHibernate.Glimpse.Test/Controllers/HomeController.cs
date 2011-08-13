using System;
using System.Web.Mvc;
using NHibernate.Glimpse.Test.Models;

namespace NHibernate.Glimpse.Test.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ExecuteCommands()
        {
            DoCommands();
            return View("Index");
        }

        public ActionResult ExecuteCommandsWithRedirect()
        {
            using (var session = MvcApplication.SessionFactory.OpenSession())
            {
                session.QueryOver<Cat>().List();    
            }
            return RedirectToAction("ExecuteCommands");
        }

        [HttpPost]
        public ActionResult ExecuteCommandsAsync()
        {
            DoCommands();
            return Content(string.Empty);
        }

        private void DoCommands()
        {
            var c = new Cat { BirthDate = DateTime.Now.AddYears(-2), Gender = "Female", Name = "Fluffy" };
            var c2 = new Cat { BirthDate = DateTime.Now.AddYears(-1), Gender = "Female", Name = "Fluffy's Baby" };
            using (var session = MvcApplication.SessionFactory.OpenSession())
            {
                HttpContext.Items.Add("session", session);
                using (var t = session.BeginTransaction())
                {
                    session.Save(c);
                    c.Kittens.Add(c2);
                    c2.Parent = c;
                    session.Flush();
                    session.Clear();
                    var cat = session.QueryOver<Cat>().Where(i => i.Id == c.Id).SingleOrDefault();
                    cat.Name = "Fluff";
                    t.Commit();
                }    
            }
            using (var session = MvcApplication.SessionFactory.OpenSession())
            {
                var fluffysBaby = session.QueryOver<Cat>().Where(i => i.Id == c2.Id).SingleOrDefault();
            }
        }
    }
}
