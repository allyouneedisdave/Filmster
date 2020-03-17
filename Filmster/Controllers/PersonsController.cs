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
    public class PersonsController : Controller
    {
        private DBContext db = new DBContext();

        // GET: Persons
        public ActionResult Index()
        {
            List<PersonViewModel> personsList = new List<PersonViewModel>();

         

            List<Person> persons;

            persons = db.Persons.ToList();

            if (persons.Count > 0)
            {
                PersonViewModel personViewModel;

                foreach (Person p in persons)
                {
                   
                    PersonImage personImage = db.PersonImages.Where(x => x.ImageId == p.ImageId).Single();

                    personViewModel = new PersonViewModel();
                    personViewModel.ThisPerson = p;
                    personViewModel.ThisPersonImage = personImage;

                    personsList.Add(personViewModel);
                }
           
            }


            return View(personsList);
        }

        // GET: Persons/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Person person = db.Persons.Find(id);
            if (person == null)
            {
                return HttpNotFound();
            }

            PersonViewModel personViewModel = new PersonViewModel();

            PersonImage personImage = db.PersonImages.Where(x => x.ImageId == person.ImageId).Single();

            List<FilmPersonRoleViewModel> personRoleViewModelList = new List<FilmPersonRoleViewModel>();

            List<FilmPersonRole> rolesList = new List<FilmPersonRole>();
            rolesList = db.FilmPersonRoles.ToList();

            // For each role, do stuff if it matches the person id.
            if (rolesList.Count > 0)
            {
                foreach(FilmPersonRole role in rolesList)
                {
                    if (role.PersonId == person.PersonId)
                    {
                        FilmPersonRoleViewModel roleViewModel = new FilmPersonRoleViewModel();

                        Film film = db.Films.Where(x => x.FilmId == role.FilmId).Single();

                        roleViewModel.ThisFilm = film;
                        roleViewModel.ThisFilmPersonRole = role;

                        personRoleViewModelList.Add(roleViewModel);

                    }
                }
            }

            personViewModel.ThisFilmPersonRolesViewModel = personRoleViewModelList;
            personViewModel.ThisPerson = person;
            personViewModel.ThisPersonImage = personImage;

            return View(personViewModel);
        }

        // GET: Persons/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Persons/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "PersonId,ImageId,FirstName,LastName," +
            "IsActor,IsDirector,Biography")] Person person,
            HttpPostedFileBase upload)
        {
            //if we have valid data in the form
            if (ModelState.IsValid)
            {
                //check to see if a file has been uploaded
                if (upload != null && upload.ContentLength > 0)
                {
                    //check to see if a file has been uploaded
                    if (upload.ContentType == "image/jpeg" ||
                        upload.ContentType == "image/jpg" ||
                        upload.ContentType == "image/gif" ||
                        upload.ContentType == "image/png")
                    {
                        //DO SOMETHING WITH THE FILEPATH
                        //CONVERT IMAGE TO BLOB
                    }
                    else
                    {
                        //construct a message that can be displayed in the view
                        ViewBag.Message = "Not valid image format";
                    }
                }
                //add the person to the database and save
                db.Persons.Add(person);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(person);
        }

        // GET: Persons/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Person person = db.Persons.Find(id);
            if (person == null)
            {
                return HttpNotFound();
            }
            return View(person);
        }

        // POST: Persons/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "PersonId,ImageId,FirstName," +
            "LastName,IsActor,IsDirector,Biography")] Person person,
            HttpPostedFileBase upload)
        {
            if (ModelState.IsValid)
            {
                //check to see if a file has been uploaded
                if (upload != null && upload.ContentLength > 0)
                {
                    //check to see if a file has been uploaded
                    if (upload.ContentType == "image/jpeg" ||
                        upload.ContentType == "image/jpg" ||
                        upload.ContentType == "image/gif" ||
                        upload.ContentType == "image/png")
                    {
                        //DO SOMETHING WITH THE FILEPATH
                        //CONVERT IMAGE TO BLOB
                    }
                    else
                    {
                        //construct a message that can be displayed in the view
                        ViewBag.Message = "Not valid image format";
                    }
                }

                db.Entry(person).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(person);
        }

        // GET: Persons/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Person person = db.Persons.Find(id);
            if (person == null)
            {
                return HttpNotFound();
            }
            return View(person);
        }

        // POST: Persons/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Person person = db.Persons.Find(id);
            db.Persons.Remove(person);
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
