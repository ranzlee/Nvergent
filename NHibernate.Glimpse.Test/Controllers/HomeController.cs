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
            DoCommands(0);
            return View("Index");
        }

        public ActionResult ExecuteCommandsWithRedirect()
        {
            
            UnitOfWork.Session.QueryOver<Cat>().List();    
            
            return RedirectToAction("ExecuteCommands");
        }

        [HttpPost]
        public ActionResult ExecuteCommandsAsync()
        {
            DoCommands(0);
            return Content(string.Empty);
        }

        private void DoCommands(int id)
        {
            var c = new Cat("meow") 
            { BirthDate = DateTime.Now.AddYears(-2), Gender = "Female", Name = "Fluffy" };
            var c2 = new Cat("meow") 
            { BirthDate = DateTime.Now.AddYears(-1), Gender = "Female", Name = "Fluffy's Baby" };
            UnitOfWork.Session.Save(c);
            c.Kittens.Add(c2);
            c2.Parent = c;
            UnitOfWork.Session.Flush();
            UnitOfWork.Session.Clear();
            var cat = UnitOfWork.Session.QueryOver<Cat>().Where(i => i.Id == c.Id).SingleOrDefault();
            cat.Name = "Fluff";
        }
    }
}
