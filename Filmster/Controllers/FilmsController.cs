using Filmster.Models;
using Filmster.Models.ViewModels;
using PagedList;
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

        //to be accessed via AJAX - autocomplete jQuery UI plugin
        public ActionResult Search(string term)
        {
            //select all the films in the db
            //and get the id and title only
            //id and label used for autocomplete functionality
            var films = from f in db.Films
                        select new
                        {
                            id = f.FilmId,
                            label = f.Title
                        };
            //now check the searchString given for any matches in title
            films = films.Where(f => f.label.Contains(term));

            //convert to and return the JSON for the search UI
            return Json(films, JsonRequestBehavior.AllowGet);
        }


        // GET: Films
        public ActionResult Index(string sortOrder, string searchString,
                            string currentFilter, int? page)
        {
            //for the viewbag to keep a note of current sort order
            ViewBag.CurrentSort = sortOrder;

            //add a new value to the viewbag to retain current sort order
            //check if the sortOrder param is empty - if so we'll set the next choice
            //to title_desc (order by title descending) otherwise empty string
            //lets us construct a toggle link for the alternative
            ViewBag.TitleSortParam = String.IsNullOrEmpty(sortOrder) ? "title_desc" : "";

            //if there is a search string
            if(searchString != null)
            {
                //set page as 1
                page = 1;
            }
            else
            {
                //if no search string, set to the current filter
                searchString = currentFilter;
            }

            //the current filter is now the search string - note kept in view
            ViewBag.CurrentFilter = searchString;


            List<FilmViewModel> filmList = new List<FilmViewModel>();

            //List<Film> films;

            //Select all films in the db
            var films = from f in db.Films
                        select f;

            //check if the search string is not empty
            if (!String.IsNullOrEmpty(searchString))
            {
                //if we have a search term, then select where the title contains it
                //analogous to LIKE %term% in SQL
                films = films.Where(f => f.Title.Contains(searchString));
            }


            //check the sortOrder param
            switch (sortOrder)
            {
                case "title_desc":
                    //order by title descending
                    films = films.OrderByDescending(f => f.Title);
                    break;
                default:
                    //order by title ascending
                    films = films.OrderBy(f => f.Title);
                    break;

            }

            List<Film> theseFilms = new List<Film>();
            theseFilms = films.ToList();

    

            foreach (Film thisFilm in theseFilms)
            {
                var averageReview = db.Reviews.Where(x => x.FilmId == thisFilm.FilmId)
                                                .Average(x => (int?)x.Rating) ?? 0;

                Certificate certificate = db.Certificates.Where(x => x.CertificateId == thisFilm.CertificateId).Single();

                FilmImage filmImage = db.FilmImages.Where(x => x.ImageId == thisFilm.ImageId).Single();

                FilmViewModel toAdd = new FilmViewModel();

                toAdd.ThisFilm = thisFilm;
                toAdd.ThisFilmImage = filmImage;
                toAdd.ThisCertificate = certificate;

                toAdd.averageReview = (int)averageReview;
                
                filmList.Add(toAdd);

            }

            //how many records per page (could also be a param..)
            int pageSize = 5;
            //if page is null set to 1 otherwise keep page value
            int pageNumber = (page ?? 1);

            IPagedList pagedList = filmList.ToPagedList(pageNumber, pageSize );

            //send the updated films list to the view
            return View(pagedList);
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

            var averageReview = db.Reviews.Where(x => x.FilmId == id)
                                .Average(x => (int?)x.Rating) ?? 0;

        

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
            filmViewModel.averageReview = (int)averageReview;

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

            List<FilmPersonRole> filmPersonRoles = db.FilmPersonRoles.ToList();

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
            filmViewModel.ThisGenre = genre;
            filmViewModel.ThisCertificate = certificate;
            filmViewModel.ThisFilmImage = filmImage;
            filmViewModel.ThisFilmPersonRoleViewModel = rolesForThisFilm;


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

            List<Review> reviews = db.Reviews.Where(x => x.FilmId == film.FilmId).ToList();
            List<FilmPersonRole> roles = db.FilmPersonRoles.Where(x => x.FilmId == film.FilmId).ToList();
            FilmImage filmImage = db.FilmImages.Where(x => x.ImageId == film.ImageId).Single();

            //Delete reviews of the film to prevent orphan data
            if (reviews.Count > 0)
            {
                foreach(Review review in reviews)
                {
                    db.Reviews.Remove(review);
                }
            }

            //Delete actor/director roles to prevent orphan data
            if (roles.Count > 0)
            {
                foreach (FilmPersonRole role in roles)
                {
                    db.FilmPersonRoles.Remove(role);
                }
            }

            db.Films.Remove(film);
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
