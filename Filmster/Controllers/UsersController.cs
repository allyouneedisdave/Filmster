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

        // GET: Users
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

        // GET: Users/Details/5
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
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
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

        // GET: Users/Edit/5
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

        // POST: Users/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
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

        // GET: Users/Delete/5
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

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            User user = db.Users.Find(id);

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
