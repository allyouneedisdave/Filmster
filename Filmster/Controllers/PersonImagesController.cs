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
    public class PersonImagesController : Controller
    {
        private DBContext db = new DBContext();

        // GET: PersonImages
        public ActionResult Index()
        {
            return View(db.PersonImages.ToList());
        }

        // GET: PersonImages/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PersonImage personImage = db.PersonImages.Find(id);
            if (personImage == null)
            {
                return HttpNotFound();
            }
            return View(personImage);
        }

        // GET: PersonImages/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: PersonImages/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ImageId,ImageBytes")] PersonImage personImage,
            HttpPostedFileBase upload)
        {
            if (ModelState.IsValid)
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

                            personImage.ImageBytes = personImage.ConvertImageToByteArray(newImage);

                            if (System.IO.File.Exists(path))
                            {
                                System.IO.File.Delete(path);
                            }
                        }
                    }
                }

                db.PersonImages.Add(personImage);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(personImage);
        }

        // GET: PersonImages/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PersonImage personImage = db.PersonImages.Find(id);
            if (personImage == null)
            {
                return HttpNotFound();
            }
            return View(personImage);
        }

        // POST: PersonImages/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ImageId,ImageBytes")] PersonImage personImage)
        {
            if (ModelState.IsValid)
            {
                db.Entry(personImage).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(personImage);
        }

        // GET: PersonImages/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PersonImage personImage = db.PersonImages.Find(id);
            if (personImage == null)
            {
                return HttpNotFound();
            }
            return View(personImage);
        }

        // POST: PersonImages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            PersonImage personImage = db.PersonImages.Find(id);
            db.PersonImages.Remove(personImage);
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
