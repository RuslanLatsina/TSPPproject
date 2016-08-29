using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TSPPproject.Models;
using TSPPproject.Models.ViewModels;
using PagedList;

namespace TSPPproject.Controllers
{
    public class RefugeesController : Controller
    {
        private VolunteerHelpEntities db = new VolunteerHelpEntities();

        // GET: Refugees
        public ActionResult Index(int? id, int? whatHelpsID, int? selectedLocation, string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.DateSortParm = sortOrder == "Location" ? "location_desc" : "Location";



            var viewModel = new RefugeeViewModel();

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;



            //if (id != null)
            //{
            //    ViewBag.Id_Volunteer = id.Value;
            //    viewModel.WhatHelpses = viewModel.Volunteers.Single(i => i.Id_Volunteer == id.Value).What_helps;
            //}

            var locations = db.Locations.OrderBy(q => q.Name).ToList();
            ViewBag.selectedLocation = new SelectList(locations, "Id_Location", "Name", selectedLocation);
            var Id_Location = selectedLocation.GetValueOrDefault();

            var refugee = db.Refugees
                .Where(c => !selectedLocation.HasValue || c.Id_Location == Id_Location)
                .OrderBy(d => d.Id_Refugee)
                .Include(d => d.Location)
                .Include(i => i.What_helps);


            if (!String.IsNullOrEmpty(searchString))
            {
                refugee = refugee.Where(s => s.LastName.Contains(searchString)
                                       || s.FirstName.Contains(searchString));
            }
            switch (sortOrder)
            {
                case "name_desc":
                    refugee = refugee.OrderByDescending(s => s.LastName);
                    break;
                case "Date":
                    refugee = refugee.OrderBy(s => s.Id_Location);
                    break;
                case "date_desc":
                    refugee = refugee.OrderByDescending(s => s.Id_Location);
                    break;
                default:  // Name ascending 
                    refugee = refugee.OrderBy(s => s.LastName);
                    break;
            }

            int pageSize = 3;
            int pageNumber = (page ?? 1);



            //if (id != null)
            //{
            //    ViewBag.Id_Volunteer = id.Value;
            //    viewModel.WhatHelpses = viewModel.Volunteers.Where(
            //        i => i.Id_Refugee == id.Value).Single().What_helps;
            //}

            return View(refugee.ToPagedList(pageNumber, pageSize));
        }

        // GET: Volunteers/Details/5
        public ActionResult Details(int? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var refugee = db.Refugees.Find(id);
            if (refugee == null)
            {
                return HttpNotFound();
            }
            return View(refugee);
        }

        // GET: Volunteers/Create
        public ActionResult Create()
        {
            var refugee = new Refugee();
            refugee.What_helps = new List<What_helps>();
            PopulateAssignedHelpData(refugee);
            PopulateLocationDropDownList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Refugee refugee, string[] selectedCourses)
        {
            if (selectedCourses != null)
            {
                refugee.What_helps = new List<What_helps>();
                foreach (var help in selectedCourses)
                {
                    var helpToAdd = db.What_helps.Find(int.Parse(help));
                    refugee.What_helps.Add(helpToAdd);
                }
            }
            if (ModelState.IsValid)
            {
                db.Refugees.Add(refugee);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            PopulateAssignedHelpData(refugee);
            PopulateLocationDropDownList(refugee.Id_Refugee);

            return View(refugee);
        }

        private void PopulateAssignedHelpData(Refugee refugee)
        {
            var allHelps = db.What_helps;
            var refugeeGetHelps = new HashSet<int>(refugee.What_helps.Select(c => c.Id_What_helps));
            var viewModel = new List<AssignedHelpData>();
            foreach (var help in allHelps)
            {
                viewModel.Add(new AssignedHelpData()
                {
                    Id_What_helps = help.Id_What_helps,
                    Name = help.Name,
                    Type_of_help = help.Type_of_help,
                    Assigned = refugeeGetHelps.Contains(help.Id_What_helps)
                });
            }
            ViewBag.What_helps = viewModel;
        }
        private void PopulateLocationDropDownList(object selectedLocation = null)
        {
            var locationQuery = from d in db.Locations
                                orderby d.Name
                                select d;
            ViewBag.Id_Location = new SelectList(locationQuery, "Id_Location", "Name", selectedLocation);
        }

        // GET: Volunteers/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var refugee = db.Refugees
                .Include(d => d.Location)
                .Include(i => i.What_helps)
                .Where(i => i.Id_Refugee == id)
                .Single();
            PopulateAssignedHelpData(refugee);
            if (refugee == null)
            {
                return HttpNotFound();
            }
            PopulateLocationDropDownList(refugee.Id_Location);
            return View(refugee);
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int? id, string[] selectedCourses)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var refugeeToUpdate = db.Refugees
            .Include(d => d.Location)
            .Include(i => i.What_helps)
            .Where(i => i.Id_Refugee == id)
            .Single();
            if (TryUpdateModel(refugeeToUpdate, "",
               new string[] { "FirstName", "LastName", "Id_Location", "Phone", "Email" }))
            {
                try
                {
                    UpdateInstructorHelps(selectedCourses, refugeeToUpdate);
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    //Log the error (uncomment dex variable name and add a line here to write a log.
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }
            PopulateAssignedHelpData(refugeeToUpdate);
            PopulateLocationDropDownList(refugeeToUpdate.Id_Location);
            return View(refugeeToUpdate);
        }
        private void UpdateInstructorHelps(string[] selectedCourses, Refugee refugeeToUpdate)
        {
            if (selectedCourses == null)
            {
                refugeeToUpdate.What_helps = new List<What_helps>();
                return;
            }

            var selectedHelpHS = new HashSet<string>(selectedCourses);
            var refugeeGetHelps = new HashSet<int>
                (refugeeToUpdate.What_helps.Select(c => c.Id_What_helps));
            foreach (var help in db.What_helps)
            {
                if (selectedHelpHS.Contains(help.Id_What_helps.ToString()))
                {
                    if (!refugeeGetHelps.Contains(help.Id_What_helps))
                    {
                        refugeeToUpdate.What_helps.Add(help);
                    }
                }
                else
                {
                    if (refugeeGetHelps.Contains(help.Id_What_helps))
                    {
                        refugeeToUpdate.What_helps.Remove(help);
                    }
                }
            }
        }

        // GET:  unteers/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Refugee refugee = db.Refugees.Find(id);
            if (refugee == null)
            {
                return HttpNotFound();
            }
            return View(refugee);
        }

        // POST: Course/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var refugee = db.Refugees
            .Include(d => d.Location)
            .Include(i => i.What_helps)
            .Where(i => i.Id_Refugee == id)
            .Single();
            db.Refugees.Remove(refugee);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
