using Filmster.Models;
using Filmster.Models.ViewModels;
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

            Genre genre = db.Genres.Where(x => x.GenreId == film.GenreId).Single();

            Certificate certificate = db.Certificates.Where(x => x.CertificateId == film.CertificateId).Single();

            FilmImage filmImage = db.FilmImages.Where(x => x.ImageId == film.ImageId).Single();

            List<FilmPersonRole> filmPersonRoles = db.FilmPersonRoles.ToList();

            List<Review> reviews = db.Reviews.ToList();

            List<ReviewViewModel> reviewsForThisFilm = new List<ReviewViewModel>();

            if (reviews.Count > 0)
            {
                foreach (Review r in reviews)
                {
                    if (r.FilmId == id)
                    {
                        ReviewViewModel reviewViewModel = new ReviewViewModel();
                        reviewViewModel.thisReview = r;

                        User user = db.Users.Where(x => x.UserId == r.UserId).Single();
                        reviewViewModel.thisUser = user;

                        reviewViewModel.thisFilm = film;

                        reviewsForThisFilm.Add(reviewViewModel);

                    }
                }
            }

            List<FilmPersonRoleViewModel> rolesForThisFilm = new List<FilmPersonRoleViewModel>();

            foreach (FilmPersonRole role in filmPersonRoles)
            {
                if (role.FilmId == id)
                {
                    Person person = db.Persons.Where(x => x.PersonId == role.PersonId).Single();

                    PersonImage personImage = new PersonImage();

                    if (person != null)
                    {
                       personImage = db.PersonImages.Where(x => x.ImageId == person.ImageId).Single();                       
                    }


                    FilmPersonRoleViewModel filmPersonRoleViewModel = new FilmPersonRoleViewModel();

                    filmPersonRoleViewModel.ThisFilm = film;
                    filmPersonRoleViewModel.ThisPerson = person;
                    filmPersonRoleViewModel.ThisPersonImage = personImage;
                    filmPersonRoleViewModel.ThisFilmPersonRole = role;
                          
                    rolesForThisFilm.Add(filmPersonRoleViewModel);

                }
            }
          
            FilmViewModel filmViewModel = new FilmViewModel();

            filmViewModel.ThisFilm = film;
            filmViewModel.ThisFilmImage = filmImage;
            filmViewModel.ThisGenre = genre;
            filmViewModel.ThisCertificate = certificate;
            filmViewModel.ThisFilmReviews = reviewsForThisFilm;
            filmViewModel.ThisFilmPersonRoleViewModel = rolesForThisFilm;
       
            return View(filmViewModel);
        }

        // GET: Films/Create
        public ActionResult Create()
        {
            var genreQuery = from m in db.Genres
                             orderby m.GenreName
                             select m;
            ViewBag.Genres = new SelectList(genreQuery, "GenreId", "GenreName", null);

            var certificateQuery = from m in db.Certificates
                                   orderby m.CertificateName
                                   select m;
            ViewBag.Certificates = new SelectList(certificateQuery, "CertificateId", "CertificateName", null);

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
                        if (Request.Files.Count > 0)
                        {
                            var file = Request.Files[0];
                            {
                                var fileName = Path.GetFileName(file.FileName);
                                var path = Path.Combine(Server.MapPath("~/ImagesTemp/"), fileName);
                                file.SaveAs(path);

                                Image newImage = Image.FromFile(path);
                                FilmImage filmImage = new FilmImage();
                                filmImage.ImageBytes = filmImage.ConvertImageToByteArray(newImage);


                                db.FilmImages.Add(filmImage);
                                db.SaveChanges();
                                int imageId = filmImage.ImageId;
                                film.ImageId = imageId;

                                if (System.IO.File.Exists(path))
                                {
                                    try
                                    {
                                        System.IO.File.Delete(path);
                                    }
                                    catch (Exception)
                                    {

                                    }

                                }
                            }
                        }


                    }
                    else
                    {
                        //construct a message that can be displayed in the view
                        ViewBag.Message = "Not valid image format";
                    }
                }
                //add the film to the database and save

                film.GenreId = Int32.Parse(Request["Genres"]);
                film.CertificateId = Int32.Parse(Request["Certificates"]);

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




            //Get genre collection for drop down box
            var genreQuery = from m in db.Genres
                             orderby m.GenreName
                             select m;

            if (film.GenreId == 0)
            {
                ViewBag.Genres = new SelectList(genreQuery, "GenreId", "GenreName", null);
            }
            else
            {
                ViewBag.Genres = new SelectList(genreQuery, "GenreId", "GenreName", film.GenreId);
            }

          

            //Get certificate selection for drop down box
            var certificateQuery = from m in db.Certificates
                                   orderby m.CertificateName
                                   select m;
            if (film.CertificateId == 0)
            {
                ViewBag.Certificates = new SelectList(certificateQuery, "CertificateId", "CertificateName", null);
            }
            else
            {
                ViewBag.Certificates = new SelectList(certificateQuery, "CertificateId", "CertificateName", film.CertificateId);
            }
            



            Genre genre = db.Genres.Where(x => x.GenreId == film.GenreId).Single();

            Certificate certificate = db.Certificates.Where(x => x.CertificateId == film.CertificateId).Single();

            FilmImage filmImage = db.FilmImages.Where(x => x.ImageId == film.ImageId).Single();

            FilmViewModel filmViewModel = new FilmViewModel();
            filmViewModel.ThisFilm = film;
            filmViewModel.ThisGenre = genre;
            filmViewModel.ThisCertificate = certificate;
            filmViewModel.ThisFilmImage = filmImage;


            return View(filmViewModel);
        }

        // POST: Films/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(FilmViewModel filmViewModel, HttpPostedFileBase upload)
        {
            if (ModelState.IsValid)
            {

                filmViewModel.ThisFilm.ImageId = filmViewModel.ThisFilmImage.ImageId;
                filmViewModel.ThisFilm.GenreId = Int32.Parse(Request["Genres"]);
                filmViewModel.ThisFilm.CertificateId = Int32.Parse(Request["Certificates"]);

                //

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
                        if (Request.Files.Count > 0)
                        {
                            var file = Request.Files[0];
                            {
                                var fileName = Path.GetFileName(file.FileName);
                                var path = Path.Combine(Server.MapPath("~/ImagesTemp/"), fileName);
                                file.SaveAs(path);

                                Image newImage = Image.FromFile(path);
                                FilmImage filmImage = new FilmImage();
                                filmImage.ImageBytes = filmImage.ConvertImageToByteArray(newImage);


                                db.FilmImages.Add(filmImage);
                                db.SaveChanges();
                                int imageId = filmImage.ImageId;
                                filmViewModel.ThisFilm.ImageId = imageId;
                 

                                if (System.IO.File.Exists(path))
                                {
                                    try
                                    {
                                        System.IO.File.Delete(path);
                                    }
                                    catch (Exception)
                                    {

                                    }

                                }
                            }
                        }


                    }
                    else
                    {
                        //construct a message that can be displayed in the view
                        ViewBag.Message = "Not valid image format";
                    }
                }

                //

                //Film film = db.Films.SingleOrDefault(c => c.FilmId == filmViewModel.ThisFilm.FilmId);

            

                db.Entry(filmViewModel.ThisFilm).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            return View(filmViewModel);
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
