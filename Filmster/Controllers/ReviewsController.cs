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
    public class ReviewsController : Controller
    {
        private DBContext db = new DBContext();


        // No search functionality required for reviews
        // as they films can be searched for in the films index
        // and reviews can be accessed from there.


        // GET: Reviews and records to creat a review viewmodel list for return.
        public ActionResult Index()
        {
            List<ReviewViewModel> reviewViewModelList = new List<ReviewViewModel>();

            List<Review> reviewsList = new List<Review>();

            reviewsList = db.Reviews.ToList();

            ReviewViewModel reviewViewModel;


            if (reviewsList.Count > 0)
            {
                foreach(Review r in reviewsList)
                {
                    reviewViewModel = new ReviewViewModel();
                    reviewViewModel.thisReview = r;

                    reviewViewModel.thisFilm = db.Films.Where(x => x.FilmId == r.FilmId).Single();
                    reviewViewModel.thisUser = db.Users.Where(x => x.UserId == r.UserId).Single();

                    reviewViewModelList.Add(reviewViewModel);
                }
            }

            return View(reviewViewModelList);
        }

        // GET: Review details to create a review view model for return.
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Review review = db.Reviews.Find(id);
           
            if (review == null)
            {
                return HttpNotFound();
            }

            User user = db.Users.Where(x => x.UserId == review.UserId).Single();

            Film film = db.Films.Where(x => x.FilmId == review.FilmId).Single();

            ReviewViewModel reviewViewModel = new ReviewViewModel();

            reviewViewModel.thisReview = review;
            reviewViewModel.thisUser = user;
            reviewViewModel.thisFilm = film;

            return View(reviewViewModel);
       
        }

        // GET: Get film data to populate the view for creating a review.
        public ActionResult Create(int? filmId)
        {
            ReviewViewModel reviewViewModel = new ReviewViewModel();
            reviewViewModel.thisFilm = new Film();
            reviewViewModel.thisReview = new Review();
            reviewViewModel.thisUser = new User();

            
            if (filmId == null || filmId == 0)
            {
                // Return to films index
                return View("~/Views/Films/index.cshtml");
            }
            else
            {
                reviewViewModel.thisFilm = db.Films.Where(x => x.FilmId == filmId).Single();
            }

            // Get a list of user names for drop down selection
            var userQuery = from u in db.Users
                            orderby u.Username
                            select u;
            ViewBag.Users = new SelectList(userQuery, "UserId", "Username", null);

            return View(reviewViewModel);
        }

        // POST: Creates a new review from viewmodel data.
        // Validates input values and abandons db insert if data is missing or incorrect.

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ReviewViewModel reviewViewModel)
        {
       
            reviewViewModel.thisReview.UserId = Int32.Parse(Request["Users"]);
            reviewViewModel.thisReview.FilmId = (int)reviewViewModel.thisFilm.FilmId;

            // Sets the created date automatically.
            reviewViewModel.thisReview.CreatedDate = DateTime.Now;

            reviewViewModel.thisReview.Rating = Int32.Parse(Request["Rating"]);
  
                db.Reviews.Add(reviewViewModel.thisReview);
                db.SaveChanges();

            return RedirectToAction("Index", "Films");

        }

        // Get viewmodel data for the edit view.
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Review review = db.Reviews.Find(id);
            if (review == null)
            {
                return HttpNotFound();
            }

            ReviewViewModel reviewViewModel = new ReviewViewModel();

            reviewViewModel.thisReview = review;

            Film film = db.Films.Where(x => x.FilmId == review.FilmId).Single();

            reviewViewModel.thisFilm = film;

            User user = db.Users.Where(x => x.UserId == review.UserId).Single();

            reviewViewModel.thisUser = user;

            return View(reviewViewModel);
        }

        // POST: Update review record with new review data from the viewmodel.
        // Validates input data and aborts the update if data is invalid.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ReviewViewModel reviewViewModel)
        {
            reviewViewModel.thisReview.Rating = Int32.Parse(Request["Rating"]);

            if (ModelState.IsValid)
            {
                db.Entry(reviewViewModel.thisReview).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(reviewViewModel);
        }

        // Gets the review data as a view model in preparation for deletion.
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Review review = db.Reviews.Find(id);
            if (review == null)
            {
                return HttpNotFound();
            }

            ReviewViewModel reviewViewModel = new ReviewViewModel();

            reviewViewModel.thisReview = review;

            Film film = db.Films.Where(x => x.FilmId == review.FilmId).Single();

            reviewViewModel.thisFilm = film;

            User user = db.Users.Where(x => x.UserId == review.UserId).Single();

            reviewViewModel.thisUser = user;

            return View(reviewViewModel);
        }

        // POST: Attempt to delete the review.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Review review = db.Reviews.Find(id);
            db.Reviews.Remove(review);
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
