using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Filmster.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            // Attemp to clear the temp images folder on application launch.
            try
            {
               
                if (Directory.Exists(Server.MapPath("~/ImagesTemp/")))
                {
                    DirectoryInfo dir = new DirectoryInfo(Server.MapPath("~/ImagesTemp/"));
                    foreach (FileInfo file in dir.GetFiles())
                    {
                        file.Delete();
                    }
                }
            }
            catch (Exception)
            {
                // Do nothing. Attempt on next launch.
            }
          


            return View();
        }


    }
}