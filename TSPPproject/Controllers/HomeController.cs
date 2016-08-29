using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.Mvc;
using TSPPproject.Models;
using TSPPproject.Models.ViewModels;
using EntityState = System.Data.EntityState;


namespace TSPPproject.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            using (VolunteerHelpEntities db = new VolunteerHelpEntities())
            {
                ViewData.Add("items", db.News.Where(x => true).ToList());
                db.News.OrderBy(x => x.Id_News);
                return View();
            }
        }
        public ActionResult Create()
        {
            return View();
        }
       

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(News content)
        {
            if (ModelState.IsValid)
            {
                db.News.Add(content);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(content);
        }


        /*[HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddComent(NewsResponses content, int? id)
        {
            if (ModelState.IsValid)
            {
                
                db.NewsResponses.Add(content);
                db.SaveChanges();
                return RedirectToAction("Page");
            }

            return View(content);
        }*/

        public ActionResult PageEdit(int? id)
        {
            using (VolunteerHelpEntities db = new VolunteerHelpEntities())
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                News content = db.News.Find(id);
                if (content == null)
                {
                    return HttpNotFound();
                }
                return View(content);
            }

        }

        


        [HttpPost, ValidateInput(false)]

        public ActionResult PageEdit(News content)
        {
            using (VolunteerHelpEntities db = new VolunteerHelpEntities())
            {
                if (ModelState.IsValid)
                {
                    db.Entry(content).State = (System.Data.Entity.EntityState) EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                return View(content);
            }

        }
        VolunteerHelpEntities db = new VolunteerHelpEntities();
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            News content = db.News.Find(id);
            if (content == null)
            {
                return HttpNotFound();
            }
            return View(content);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            News content = db.News.Find(id);
            db.News.Remove(content);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Page(int id)
        {
            VolunteerHelpEntities db = new VolunteerHelpEntities();
            var content = db.News.SingleOrDefault(x => x.Id_News == id);

            return View(content);
        }

        public ActionResult About()
        {
            IQueryable < RefugeeCountGroup > viewmodel = from student in db.Refugees
                                                 group student by student.Id_Location into dateGroup
                                                 select new RefugeeCountGroup()
                                                 {
                                                     Id_Location = dateGroup.Key,
                                                     Name = dateGroup.Select(c=>c.Location.Name).FirstOrDefault(),
                                                     RefugeeCount = dateGroup.Count()
                                                 };




            //Commenting out LINQ to show how to do the same thing in SQL.
            //rf = from refugee in db.Refugees
            //    group refugee by refugee.Id_Location into dateGroup
            //    select new RefugeeCountGroup()
            //    {
            //        Id_Location = dateGroup.Key,
            //        RefugeeCount = dateGroup.Count()
            //    };

            // SQL version of the above LINQ code.
            //string query = "SELECT Location, COUNT(*) AS RefugeeCount "
            //    + "FROM Refugee "
            //    + "GROUP BY Id_Location";
            //IEnumerable<RefugeeCountGroup> data = db.Database.SqlQuery<RefugeeCountGroup>(query);

            return View(viewmodel.ToList());
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}