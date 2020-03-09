using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Filmster.Models;

namespace Filmster.Controllers
{
    public class FilmImagesController : Controller
    {
        private DBContext db = new DBContext();

        // GET: FilmImages
        public ActionResult Index()
        {
            return View(db.FilmImages.ToList());
        }

        // GET: FilmImages/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FilmImage filmImage = db.FilmImages.Find(id);
            if (filmImage == null)
            {
                return HttpNotFound();
            }
            return View(filmImage);
        }

        // GET: FilmImages/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: FilmImages/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ImageId,ImageBytes")] FilmImage filmImage,
            HttpPostedFileBase upload)
        {
            if (ModelState.IsValid)
            {
                if (upload != null && upload.ContentLength > 0)
                {
                    if (upload.ContentType == "image/jpeg" ||
                        upload.ContentType == "image/jpg" ||
                        upload.ContentType == "image/gif" ||
                        upload.ContentType == "image/png")
                    {
                        if (Request.Files.Count > 0)
                        {
                            var file = Request.Files[0];
                            {
                                var fileName = Path.GetFileName(file.FileName);
                                var path = Path.Combine(Server.MapPath("~/Images/"), fileName);
                                file.SaveAs(path);

                                Image newImage = Image.FromFile(path);

                                filmImage.ImageBytes = filmImage.ConvertImageToByteArray(newImage);

                            }
                        }
         
                        
                    }
                }


                db.FilmImages.Add(filmImage);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(filmImage);
        }

        // GET: FilmImages/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FilmImage filmImage = db.FilmImages.Find(id);
            if (filmImage == null)
            {
                return HttpNotFound();
            }
            return View(filmImage);
        }

        // POST: FilmImages/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ImageId,ImageBytes")] FilmImage filmImage)
        {
            if (ModelState.IsValid)
            {
                db.Entry(filmImage).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(filmImage);
        }

        // GET: FilmImages/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FilmImage filmImage = db.FilmImages.Find(id);
            if (filmImage == null)
            {
                return HttpNotFound();
            }
            return View(filmImage);
        }

        // POST: FilmImages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            FilmImage filmImage = db.FilmImages.Find(id);
            db.FilmImages.Remove(filmImage);
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
