using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TSPPproject.Models;
using TSPPproject.Models.ViewModels;

namespace TSPPproject.Controllers
{
    public class ServicesController : Controller
    {
        private VolunteerHelpEntities db = new VolunteerHelpEntities();

        public interface IDatabase
        {
            bool RegisterProductVote(int userId, int productid, int vote);
        }

        bool RegisterProductVote(int userId, int productid, int vote)
        {
            return true;
        }
        public ActionResult RateProduct(int id, int rate)
        {
            int userId = 142; // WebSecurity.CurrentUserId;
            bool success = false;
            string error = "";

            try
            {
                success = RegisterProductVote(userId, id, rate);
            }
            catch (System.Exception ex)
            {
                // get last error
                if (ex.InnerException != null)
                    while (ex.InnerException != null)
                        ex = ex.InnerException;

                error = ex.Message;
            }

            return Json(new { error = error, success = success, pid = id }, JsonRequestBehavior.AllowGet);
        }
    }
}