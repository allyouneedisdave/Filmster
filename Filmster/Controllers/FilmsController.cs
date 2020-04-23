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
    
        // Autocomplete jQuery UI plugin, accessed via AJAX.
        // Gets all films from db and compares the title against
        // the search term. Matches are converted to JSON and returned to UI.
        public ActionResult Search(string term)
        {
            var films = from f in db.Films
                        select new
                        {
                            id = f.FilmId,
                            label = f.Title
                        };

            films = films.Where(f => f.label.Contains(term));

            return Json(films, JsonRequestBehavior.AllowGet);
        }


        // Get films for the index view and builds a list of film viewmodels to return.
        // Utilises arguments to return a sorting order, page of results
        // and searched results. The sort order is tracked by the viewbag.
        public ActionResult Index(string errorMessage, string sortOrder, string searchString,
                            string currentFilter, int? page)
        {

            // This section sets the search, ordering and pagination variables
            // from the values sent in the ActionResult arguments.
            ViewBag.CurrentSort = sortOrder;

            if(errorMessage != null)
            {
                ViewBag.ErrorMessage = errorMessage;
            }

            ViewBag.TitleSortParam = String.IsNullOrEmpty(sortOrder) ? "title_desc" : "";

            if(searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

        

            List<FilmViewModel> filmList = new List<FilmViewModel>();      



            // Get all films, then overwrite with returned results if a searchString
            // is specified.
            var films = from f in db.Films
                        select f;

            if (!String.IsNullOrEmpty(searchString))
            {
                films = films.Where(f => f.Title.Contains(searchString));
            }


            // Apply sorting based on the specified argument from the view.
            switch (sortOrder)
            {
                case "title_desc":
                    films = films.OrderByDescending(f => f.Title);
                    break;
                default:
                    films = films.OrderBy(f => f.Title);
                    break;
            }


            List<Film> theseFilms = new List<Film>();
            theseFilms = films.ToList();
 
            // Loop through film results and get viewmodel information from other db entities.
            // Assign the entities to the new viewmodel and add the viewmodel to the viewmodels list.
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


            // Set the amount of records shown on a page and convert the viewmodel
            // to an IpagedList object to return to the view.
            int pageSize = 5;
            // If page is null set to 1 otherwise keep page value.
            int pageNumber = (page ?? 1);

            IPagedList pagedList = filmList.ToPagedList(pageNumber, pageSize );

            return View(pagedList);
        }


        // Get film details by film id, builds a viewmodel for return.
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

            // Get's an average review score from all Reviews for this film.
            var averageReview = db.Reviews.Where(x => x.FilmId == id)
                                .Average(x => (int?)x.Rating) ?? 0;

        
            // Get records for the view model.
            Genre genre = db.Genres.Where(x => x.GenreId == film.GenreId).Single();

            Certificate certificate = db.Certificates.Where(x => x.CertificateId == film.CertificateId).Single();

            FilmImage filmImage = db.FilmImages.Where(x => x.ImageId == film.ImageId).Single();

            List<FilmPersonRole> filmPersonRoles = db.FilmPersonRoles.ToList();

            List<Review> reviews = db.Reviews.ToList();


            // If reviews exist for this film, return a list of reviews for the viewmodel.
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

            // If roles exist for this film, return a list of roles for the viewmodel.
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

        // Gets genre and certificate options for the creation of a new film
        // and adds the collections to the viewbag to be used in a dropdownlist.
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

        // POST: Creates a new film and image record from viewmodel data.
        // Validates input values and abandons db insert if data is missing or incorrect.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            [Bind(Include = "FilmID,GenreID,CertificateID,ImageID," +
            "Title,Synopsis,Runtime,ReleaseDate")] Film film,
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
                        // If an image is being uploaded then save the image to a temporary folder, 
                        // convert the image to bytes and assign it to the model for inserting into the db.
                        if (Request.Files.Count > 0)
                        {
                            var file = Request.Files[0];

                            // Save image to temp folder.      
                            var fileName = Path.GetFileName(file.FileName);
                            var path = Path.Combine(Server.MapPath("~/ImagesTemp/"), fileName);
                            file.SaveAs(path);

                            // Convert image to bytes.      
                            Image newImage = Image.FromFile(path);
                            FilmImage filmImage = new FilmImage();
                            filmImage.ImageBytes = filmImage.ConvertImageToByteArray(newImage);

                            // Insert image into db and return the new image id.      
                            db.FilmImages.Add(filmImage);
                            db.SaveChanges();
                            int imageId = filmImage.ImageId;
                            film.ImageId = imageId;

                            // Attempt to delete temporary image.      
                            if (System.IO.File.Exists(path))
                            {
                                try
                                {
                                    System.IO.File.Delete(path);
                                }
                                catch (Exception thisException)
                                {
                                    // Do nothing, temporary folder will be       
                                    // cleared when application is re-launched.      
                                }
                            }
                            // Add the film to the database and save.
                            film.GenreId = Int32.Parse(Request["Genres"]);
                            film.CertificateId = Int32.Parse(Request["Certificates"]);

                            db.Films.Add(film);
                            db.SaveChanges();

                        }
                    }
                    else
                    {
                        // Constructs error messages for the view.
                        ViewBag.ErrorMessage = "A valid image image format was not uploaded.";
                    }
                }
                else
                {
                    // Constructs error messages for the view.
                    ViewBag.ErrorMessage = "An image must be uploaded.";
                }
  
                return RedirectToAction("Index", "Films", new { errorMessage = ViewBag.ErrorMessage });

            }

            return View(film);
        }

        // Get viewmodel data for the edit view.
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

            // Get genre collection for drop down box.
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

          

            // Get certificate selection for drop down box.
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
           
            // Get record for the viewmodel data.
            Genre genre = db.Genres.Where(x => x.GenreId == film.GenreId).Single();

            Certificate certificate = db.Certificates.Where(x => x.CertificateId == film.CertificateId).Single();

            FilmImage filmImage = db.FilmImages.Where(x => x.ImageId == film.ImageId).Single();

            List<FilmPersonRole> filmPersonRoles = db.FilmPersonRoles.ToList();

            List<FilmPersonRoleViewModel> rolesForThisFilm = new List<FilmPersonRoleViewModel>();

            // Loop through film roles for roles related to this film.
            // Add matches to the role viewmodel list.
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

        // POST: Update film record with new film data from the viewmodel.
        // Validates input data and aborts the update if data is invalid.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(FilmViewModel filmViewModel, HttpPostedFileBase upload)
        {
            if (ModelState.IsValid)
            {

                filmViewModel.ThisFilm.ImageId = filmViewModel.ThisFilmImage.ImageId;

                // Gets the dropdownlist selection and adds the id to the viewmodel.
                filmViewModel.ThisFilm.GenreId = Int32.Parse(Request["Genres"]);
                filmViewModel.ThisFilm.CertificateId = Int32.Parse(Request["Certificates"]);

                
                if (upload != null && upload.ContentLength > 0)
                {
                    if (upload.ContentType == "image/jpeg" ||
                        upload.ContentType == "image/jpg" ||
                        upload.ContentType == "image/gif" ||
                        upload.ContentType == "image/png")
                    {
                        // If an image is being uploaded then save the image to a
                        // temporary folder, convert the image to bytes, save the image
                        // as a new record and assign its new id to the film model for
                        // updating the db.
                        if (Request.Files.Count > 0)
                        {
                            var file = Request.Files[0];
                            {
                                // Save image to temp folder.
                                var fileName = Path.GetFileName(file.FileName);
                                var path = Path.Combine(Server.MapPath("~/ImagesTemp/"), fileName);
                                file.SaveAs(path);

                                // Convert image to bytes.
                                Image newImage = Image.FromFile(path);
                                FilmImage filmImage = new FilmImage();
                                filmImage.ImageBytes = filmImage.ConvertImageToByteArray(newImage);

                                // Insert image into db and return the new image id.
                                db.FilmImages.Add(filmImage);
                                db.SaveChanges();
                                int imageId = filmImage.ImageId;

                                //Get the old film image and delete it.
                                FilmImage oldFilmImage = db.FilmImages.Where(x => x.ImageId == filmViewModel.ThisFilmImage.ImageId).Single();
                                db.FilmImages.Remove(oldFilmImage);

                                filmViewModel.ThisFilm.ImageId = imageId;
                 
                                // Attempt to delete temporary image.
                                if (System.IO.File.Exists(path))
                                {
                                    try
                                    {
                                        System.IO.File.Delete(path);
                                    }
                                    catch (Exception)
                                    {
                                        // Do nothing, temporary folder will be cleared
                                        // when application is re-launched.
                                    }

                                }
                            }
                        }


                    }
                    else
                    {
                        // Constructs error messages for the view.
                        ViewBag.Message = "Not valid image format";
                    }
                }


                //Update the db and save.
                db.Entry(filmViewModel.ThisFilm).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            return View(filmViewModel);
        }

        // GET: Gets the film data in preparation for deletion.
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

        // POST: Attempt to delete the film and image.
        // Any film roles and reviews for this film will also be deleted to prevent orphan data.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
           
            Film film = db.Films.Find(id);

            List<Review> reviews = db.Reviews.Where(x => x.FilmId == film.FilmId).ToList();
            List<FilmPersonRole> roles = db.FilmPersonRoles.Where(x => x.FilmId == film.FilmId).ToList();
            FilmImage filmImage = db.FilmImages.Where(x => x.ImageId == film.ImageId).Single();

            // Delete reviews of the film to prevent orphan data.
            if (reviews.Count > 0)
            {
                foreach(Review review in reviews)
                {
                    db.Reviews.Remove(review);
                }
            }

            // Delete actor/director roles to prevent orphan data.
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
