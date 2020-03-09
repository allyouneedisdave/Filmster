using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Filmster.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Interested in films? Look no further! Filmster has all you need to satisfy your filming needs.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Feel free to drop us a line!";

            return View();
        }
    }
}