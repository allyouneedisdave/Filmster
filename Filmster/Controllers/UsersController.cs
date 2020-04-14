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
    public class UsersController : Controller
    {
        private DBContext db = new DBContext();

        // Gets a list of all users and creates a users view model list for return.
        public ActionResult Index()
        {
            List<UserViewModel> userViewModelList = new List<UserViewModel>();

            UserViewModel userViewModel;

            List<User> users = db.Users.ToList();

            if (users.Count > 0)
            {
                foreach(User user in users)
                {
                    userViewModel = new UserViewModel();
                    userViewModel.thisUser = user;
                    userViewModelList.Add(userViewModel);
                }
            }

            return View(userViewModelList);
        }

        // Gets the user details and presents them as a view model, 
        // which includes a list of all reviews for the specified user.
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            UserViewModel userViewModel = new UserViewModel();
            userViewModel.thisUser = user;
            userViewModel.thisReviews = db.Reviews.Where(x => x.UserId == user.UserId).ToList();

            return View(userViewModel);
        }

        // GET: Users/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "UserId,username")] User user)
        {
            if (ModelState.IsValid)
            {
                db.Users.Add(user);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(user);
        }

        // GET: Returns a view model for the user for the edit view.
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            UserViewModel userViewModel = new UserViewModel();
            userViewModel.thisUser = user;
            userViewModel.thisReviews = db.Reviews.Where(x => x.UserId == user.UserId).ToList();

            return View(userViewModel);
        }

        // Attempts to edit the user record from the user view model data.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(UserViewModel userViewModel)
        {
            if (ModelState.IsValid)
            {
                db.Entry(userViewModel.thisUser).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(userViewModel);
        }

        // Gets the user record data in preperation for deletion.
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

          

            return View(user);
        }

        // POST: Attempt to delete the user
        // Any reviews for this user will be deleted.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            User user = db.Users.Find(id);

            // Delete reviews from this user to prevent orphan data.
            List<Review> reviews = db.Reviews.Where(x => x.UserId == user.UserId).ToList();

            if (reviews.Count > 0)
            {
                foreach (Review review in reviews)
                {
                    db.Reviews.Remove(review);
                }
            }

            db.Users.Remove(user);
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
