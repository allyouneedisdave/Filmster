using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Filmster.Models;
using Filmster.Models.ViewModels;

namespace Filmster.Controllers
{
    public class FilmsController : Controller
    {
        private DBContext db = new DBContext();

        // GET: Films
        public ActionResult Index()
        {
            List<FilmViewModel> FilmList = new List<FilmViewModel>();

            List<Film> films;

            films = db.Films.ToList();

            foreach (Film thisFilm in films)
            {
                // Genre genre = db.Genres.Where(x => x.genre_id == thisFilm.GenreId).Single();

                Certificate certificate = db.Certificates.Where(x => x.CertificateId == thisFilm.CertificateId).Single();

                FilmImage filmImage = db.FilmImages.Where(x => x.ImageId == thisFilm.ImageId).Single();

                FilmViewModel toAdd = new FilmViewModel();

                toAdd.ThisFilm = thisFilm;
                toAdd.ThisFilmImage = filmImage;
                toAdd.ThisCertificate = certificate;

                FilmList.Add(toAdd);

            }
            return View(FilmList);
        }

        // GET: Films/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Film film = db.Films.Find(id);
            if (film == null)
            {
                return HttpNotFound();
            }

            Genre genre = db.Genres.Where(x => x.genre_id == film.GenreId).Single();

            Certificate certificate = db.Certificates.Where(x => x.CertificateId == film.CertificateId).Single();

            FilmImage filmImage = db.FilmImages.Where(x => x.ImageId == film.ImageId).Single();

            FilmViewModel filmViewModel = new FilmViewModel();

            filmViewModel.ThisFilm = film;
            filmViewModel.ThisFilmImage = filmImage;
            filmViewModel.ThisCertificate = certificate;

          

            return View(filmViewModel);
        }

        // GET: Films/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Films/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            [Bind(Include = "FilmID,GenreID,CertificateID,ImageID," +
            "Title,Synopsis,Runtime,ReleaseDate")] Film film,
            HttpPostedFileBase upload)
        {
            //if we have valid data in the form
            if (ModelState.IsValid)
            {
                //check to see if a file has been uploaded
                if (upload != null && upload.ContentLength > 0)
                {
                    //check to see if valid MIME type (JPG / PNG or GIF images)
                    if (upload.ContentType == "image/jpeg" ||
                        upload.ContentType == "image/jpg" ||
                        upload.ContentType == "image/gif" ||
                        upload.ContentType == "image/png")
                    {
                        //DO SOMETHING WITH THE FILE
                        //CREATE A METHOD TO CONVERT TO BLOB
                    }
                    else
                    {
                        //construct a message that can be displayed in the view
                        ViewBag.Message = "Not valid image format";
                    }
                }
                //add the film to the database and save

                db.Films.Add(film);
                db.SaveChanges();
                //redirect to index
                return RedirectToAction("Index");
            }

            return View(film);
        }

        // GET: Films/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Film film = db.Films.Find(id);
            if (film == null)
            {
                return HttpNotFound();
            }
            return View(film);
        }

        // POST: Films/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "FilmID,GenreID,CertificateID,ImageID,Title,Synopsis,Runtime,ReleaseDate")] Film film)
        {
            if (ModelState.IsValid)
            {
                db.Entry(film).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(film);
        }

        // GET: Films/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Film film = db.Films.Find(id);
            if (film == null)
            {
                return HttpNotFound();
            }
            return View(film);
        }

        // POST: Films/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Film film = db.Films.Find(id);
            db.Films.Remove(film);
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
