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

namespace TSPPproject.Controllers
{
    public class VolunteersController : Controller
    {
        private VolunteerHelpEntities db = new VolunteerHelpEntities();

        // GET: Volunteers
        //public ActionResult Index(int? selectedLocation)
        //{
        //    var locations = db.Locations.OrderBy(q => q.Name).ToList();
        //    ViewBag.selectedLocation = new SelectList(locations, "Id_Location", "Name", selectedLocation);
        //    int Id_Location = selectedLocation.GetValueOrDefault();

        //    IQueryable<Volunteer> volonteers = db.Volunteers
        //        .Where(c => !selectedLocation.HasValue || c.Id_Location == Id_Location)
        //        .OrderBy(d => d.Id_Volunteer)
        //        .Include(d => d.Location);
        //    return View(volonteers.ToList());
        //}

        public ActionResult Index(int? id, int? whatHelpsID, int? selectedLocation, int? courseID, string sortOrder, string currentFilter)
        {
            var viewModel = new VolunteerViewModel();
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.DateSortParm = sortOrder == "Rate" ? "rate_desc" : "Rate";

            var locations = db.Locations.OrderBy(q => q.Name).ToList();
            ViewBag.selectedLocation = new SelectList(locations, "Id_Location", "Name", selectedLocation);
            var Id_Location = selectedLocation.GetValueOrDefault();

            viewModel.Volunteers =  db.Volunteers
                .Include(d => d.Location)
                .Include(i => i.What_helps)
                .Where(c => !selectedLocation.HasValue || c.Id_Location == Id_Location)
                .OrderBy(d => d.Id_Volunteer);
            if (id != null)
            {
                ViewBag.Id_Volunteer = id.Value;
                viewModel.WhatHelpses = viewModel.Volunteers.Where(i => i.Id_Volunteer == id.Value).Single().What_helps;
            }

            if (courseID.HasValue)
            {

                var selectedCourse = viewModel.WhatHelpses.Single(x => x.Id_What_helps == courseID.Value);
                db.Entry(selectedCourse).Collection(x => x.Refugees).Load();

                viewModel.Refugees = selectedCourse.Refugees;
            }
            switch (sortOrder)
            {
                case "name_desc":
                    viewModel.Volunteers = viewModel.Volunteers.OrderByDescending(s => s.Name);
                    break;
                case "Rate":
                    viewModel.Volunteers = viewModel.Volunteers.OrderBy(s => s.Rating.Value);
                    break;
                case "Rate_desc":
                    viewModel.Volunteers = viewModel.Volunteers.OrderByDescending(s => s.Rating.Value);
                    break;
                default:  // Name ascending 
                    viewModel.Volunteers = viewModel.Volunteers.OrderBy(s => s.Name);
                    break;
            }


            return View(viewModel);
        }

        // GET: Volunteers/Details/5
        public ActionResult Details(int? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var volonteer = db.Volunteers.Find(id);
            if (volonteer == null)
            {
                return HttpNotFound();
            }
            return View(volonteer);
        }

        // GET: Volunteers/Create
        public ActionResult Create()
        {
            var volonteer = new Volunteer();
            volonteer.What_helps = new List<What_helps>();
            PopulateAssignedHelpData(volonteer);
            PopulateLocationDropDownList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Volunteer volonteer, string[] selectedCourses)
        {
            if (selectedCourses != null)
            {
                volonteer.What_helps = new List<What_helps>();
                foreach (var help in selectedCourses)
                {
                    var helpToAdd = db.What_helps.Find(int.Parse(help));
                    volonteer.What_helps.Add(helpToAdd);
                }
            }
            if (ModelState.IsValid)
            {
                db.Volunteers.Add(volonteer);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            PopulateAssignedHelpData(volonteer);
            PopulateLocationDropDownList(volonteer.Id_Volunteer);

            return View(volonteer);
        }
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var volunteer = db.Volunteers
                .Include(d => d.Location)
                .Include(i => i.What_helps)
                .Where(i => i.Id_Volunteer == id)
                .Single();
            PopulateAssignedHelpData(volunteer);
            if (volunteer == null)
            {
                return HttpNotFound();
            }
            PopulateLocationDropDownList(volunteer.Id_Location);
            return View(volunteer);
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int? id, string[] selectedCourses)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var volonteerToUpdate = db.Volunteers
            .Include(d => d.Location)
            .Include(i => i.What_helps)
            .Where(i => i.Id_Volunteer == id)
            .Single();
            if (TryUpdateModel(volonteerToUpdate, "",
               new string[] { "Name", "Description", "Id_Location", "Phone", "Address", "Email" }))
            {
                try
                {

                    UpdateInstructorHelps(selectedCourses, volonteerToUpdate);
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    //Log the error (uncomment dex variable name and add a line here to write a log.
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }
            PopulateAssignedHelpData(volonteerToUpdate);
            PopulateLocationDropDownList(volonteerToUpdate.Id_Location);
            return View(volonteerToUpdate);
        }
        private void PopulateAssignedHelpData(Volunteer volonteer)
        {
            var allHelps = db.What_helps;
            var VolonteerHelps = new HashSet<int>(volonteer.What_helps.Select(c => c.Id_What_helps));
            var viewModel = new List<AssignedHelpData>();
            foreach (var help in allHelps)
            {
                viewModel.Add(new AssignedHelpData
                {
                    Id_What_helps = help.Id_What_helps,
                    Name = help.Name,
                    Type_of_help = help.Type_of_help,
                    Assigned = VolonteerHelps.Contains(help.Id_What_helps)
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
        

        private void UpdateInstructorHelps(string[] selectedCourses, Volunteer volonteerToUpdate)
        {
            if (selectedCourses == null)
            {
                volonteerToUpdate.What_helps = new List<What_helps>();
                return;
            }

            var selectedHelpHS = new HashSet<string>(selectedCourses);
            var VolonteerHelps = new HashSet<int>
                (volonteerToUpdate.What_helps.Select(c => c.Id_What_helps));
            foreach (var help in db.What_helps)
            {
                if (selectedHelpHS.Contains(help.Id_What_helps.ToString()))
                {
                    if (!VolonteerHelps.Contains(help.Id_What_helps))
                    {
                        volonteerToUpdate.What_helps.Add(help);
                    }
                }
                else
                {
                    if (VolonteerHelps.Contains(help.Id_What_helps))
                    {
                        volonteerToUpdate.What_helps.Remove(help);
                    }
                }
            }
        }

        // GET: Volunteers/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Volunteer volonteer = db.Volunteers.Find(id);
            if (volonteer == null)
            {
                return HttpNotFound();
            }
            return View(volonteer);
        }

        // POST: Course/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var volonteer = db.Volunteers
            .Include(d => d.Location)
            .Include(i => i.What_helps)
            .Where(i => i.Id_Volunteer == id)
            .Single();
            db.Volunteers.Remove(volonteer);
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

    //Алгоритм Дейкстри
    class DekstraAlgorim
    {

        public Point[] points { get; private set; }
        public Rebro[] rebra { get; private set; }
        public Point BeginPoint { get; private set; }

        public DekstraAlgorim(Point[] pointsOfgrath, Rebro[] rebraOfgrath)
        {
            points = pointsOfgrath;
            rebra = rebraOfgrath;
        }
        /// <summary>
        /// Початок розрахунку
        /// </summary>
        /// <param name="beginp"></param>
        public void AlgoritmRun(Point beginp)
        {
            if (this.points.Count() == 0 || this.rebra.Count() == 0)
            {
                throw new DekstraException("Массив вершин или ребер не задан!");
            }
            else
            {
                BeginPoint = beginp;
                OneStep(beginp);
                foreach (Point point in points)
                {
                    Point anotherP = GetAnotherUncheckedPoint();
                    if (anotherP != null)
                    {
                        OneStep(anotherP);
                    }
                    else
                    {
                        break;
                    }

                }
            }

        }
        /// <summary>
        /// Метод, який виконує один крок алгоритму. Приймає на вхід вершину
        /// </summary>
        /// <param name="beginpoint"></param>
        public void OneStep(Point beginpoint)
        {
            foreach (Point nextp in Pred(beginpoint))
            {
                if (nextp.IsChecked == false)//не підмічена
                {
                    float newmetka = beginpoint.ValueMetka + GetMyRebro(nextp, beginpoint).Weight;
                    if (nextp.ValueMetka > newmetka)
                    {
                        nextp.ValueMetka = newmetka;
                        nextp.predPoint = beginpoint;
                    }
                    else
                    {

                    }
                }
            }
            beginpoint.IsChecked = true;//викреслюємо
        }
        /// <summary>
        /// Пошук сусідів для вершини. Для неорієнтованого шукаються всі сусіди.
        /// </summary>
        /// <param name="currpoint"></param>
        /// <returns></returns>
        private IEnumerable<Point> Pred(Point currpoint)
        {
            IEnumerable<Point> firstpoints = from ff in rebra where ff.FirstPoint == currpoint select ff.SecondPoint;
            IEnumerable<Point> secondpoints = from sp in rebra where sp.SecondPoint == currpoint select sp.FirstPoint;
            IEnumerable<Point> totalpoints = firstpoints.Concat<Point>(secondpoints);
            return totalpoints;
        }
        /// <summary>
        /// Получили ребро, яке з'єднало дві сусідні точки
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private Rebro GetMyRebro(Point a, Point b)
        {//шукаємо ребро по двох точках
            IEnumerable<Rebro> myr = from reb in rebra where (reb.FirstPoint == a & reb.SecondPoint == b) || (reb.SecondPoint == a & reb.FirstPoint == b) select reb;
            if (myr.Count() > 1 || myr.Count() == 0)
            {
                throw new DekstraException("Не найдено ребро между соседями!");
            }
            else
            {
                return myr.First();
            }
        }
        /// <summary>
        /// получаємо найближчу, невідмічену вершину
        /// </summary>
        /// <returns></returns>
        private Point GetAnotherUncheckedPoint()
        {
            IEnumerable<Point> pointsuncheck = from p in points where p.IsChecked == false select p;
            if (pointsuncheck.Count() != 0)
            {
                float minVal = pointsuncheck.First().ValueMetka;
                Point minPoint = pointsuncheck.First();
                foreach (Point p in pointsuncheck)
                {
                    if (p.ValueMetka < minVal)
                    {
                        minVal = p.ValueMetka;
                        minPoint = p;
                    }
                }
                return minPoint;
            }
            else
            {
                return null;
            }
        }

        public List<Point> MinPath1(Point end)
        {
            List<Point> listOfpoints = new List<Point>();
            Point tempp = new Point();
            tempp = end;
            while (tempp != this.BeginPoint)
            {
                listOfpoints.Add(tempp);
                tempp = tempp.predPoint;
            }

            return listOfpoints;
        }
    }

    /// <summary>
    /// Клас, який реалізовує ребро
    /// </summary>
    class Rebro
    {
        public Point FirstPoint { get; private set; }
        public Point SecondPoint { get; private set; }
        public float Weight { get; private set; }

        public Rebro(Point first, Point second, float valueOfWeight)
        {
            FirstPoint = first;
            SecondPoint = second;
            Weight = valueOfWeight;
        }

    }
    /// <summary>
    /// Клас, який реалізовує вершину
    /// </summary>
    class Point
    {
        public float ValueMetka { get; set; }
        public string Name { get; private set; }
        public bool IsChecked { get; set; }
        public Point predPoint { get; set; }
        public object SomeObj { get; set; }
        public Point(int value, bool ischecked)
        {
            ValueMetka = value;
            IsChecked = ischecked;
            predPoint = new Point();
        }
        public Point(int value, bool ischecked, string name)
        {
            ValueMetka = value;
            IsChecked = ischecked;
            Name = name;
            predPoint = new Point();
        }
        public Point()
        {
        }
    }

    // <summary>
    /// для виводу графу
    /// </summary>
    static class PrintGrath
    {
        public static List<string> PrintAllPoints(DekstraAlgorim da)
        {
            List<string> retListOfPoints = new List<string>();
            foreach (Point p in da.points)
            {
                retListOfPoints.Add(string.Format("point name={0}, point value={1}, predok={2}", p.Name, p.ValueMetka, p.predPoint.Name ?? "нет предка"));
            }
            return retListOfPoints;
        }
        public static List<string> PrintAllMinPaths(DekstraAlgorim da)
        {
            List<string> retListOfPointsAndPaths = new List<string>();
            foreach (Point p in da.points)
            {

                if (p != da.BeginPoint)
                {
                    string s = string.Empty;
                    foreach (Point p1 in da.MinPath1(p))
                    {
                        s += string.Format("{0} ", p1.Name);
                    }
                    retListOfPointsAndPaths.Add(string.Format("Point ={0},MinPath from {1} = {2}", p.Name, da.BeginPoint.Name, s));
                }

            }
            return retListOfPointsAndPaths;
        }
    }
    class DekstraException : ApplicationException
    {
        public DekstraException(string message) : base(message)
        {
        }
    }


}
