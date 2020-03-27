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

        // GET: Reviews
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

        // GET: Reviews/Details/5
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

        // GET: Reviews/Create
        public ActionResult Create(int? filmId)
        {
            ReviewViewModel reviewViewModel = new ReviewViewModel();
            reviewViewModel.thisFilm = new Film();
            reviewViewModel.thisReview = new Review();
            reviewViewModel.thisUser = new User();

            
            if (filmId == null || filmId == 0)
            {
                //Return to home
                return View("~/Views/Home/index.cshtml");
            }
            else
            {
                reviewViewModel.thisFilm = db.Films.Where(x => x.FilmId == filmId).Single();
            }

            //Get a list of user names for drop down selection
            var userQuery = from u in db.Users
                            orderby u.Username
                            select u;
            ViewBag.Users = new SelectList(userQuery, "UserId", "Username", null);

            return View(reviewViewModel);
        }

        // POST: Reviews/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ReviewViewModel reviewViewModel)
        {
       
            reviewViewModel.thisReview.UserId = Int32.Parse(Request["Users"]);
            reviewViewModel.thisReview.FilmId = (int)reviewViewModel.thisFilm.FilmId;

            reviewViewModel.thisReview.CreatedDate = DateTime.Now;

            reviewViewModel.thisReview.Rating = Int32.Parse(Request["Rating"]);
  
                db.Reviews.Add(reviewViewModel.thisReview);
                db.SaveChanges();

            return RedirectToAction("Index", "Films");

        }

        // GET: Reviews/Edit/5
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

        // POST: Reviews/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ReviewId,FilmId,UserId,ReviewTitle,ReviewDetail,CreatedDate,Rating")] Review review)
        {
            if (ModelState.IsValid)
            {
                db.Entry(review).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(review);
        }

        // GET: Reviews/Delete/5
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
            return View(review);
        }

        // POST: Reviews/Delete/5
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
