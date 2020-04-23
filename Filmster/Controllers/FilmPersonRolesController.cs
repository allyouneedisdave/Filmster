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
    public class FilmPersonRolesController : Controller
    {
        private DBContext db = new DBContext();

        // GET: FilmPersonRoles
        public ActionResult Index()
        {
            //create a list for the view model to link Film and Person
            List<FilmPersonRoleViewModel> FilmPersonRoleList = new List<FilmPersonRoleViewModel>();

            //separate list for the FilmPersonRole to get the keys
            List<FilmPersonRole> filmPersonRoles;

            //populate the FilmPersonRoleList by selecting all record from the db context
            filmPersonRoles = db.FilmPersonRoles.ToList();

            //loop through each record to get the foreign keys
            //then populate the view model with the relevant
            //Film / Person
            foreach (FilmPersonRole role in filmPersonRoles)
            {
                //match the ID between FilmPersonRole and Film - store the single record in 'film'
                Film film = db.Films.Where(x => x.FilmId == role.FilmId).Single();

                //match the ID between FilmPersonRole and Film - store the single record in 'person'
                Person person = db.Persons.Where(x => x.PersonId == role.PersonId).Single();

                //NEED TO ADD BYTE DATA TO DATABASE FOR THIS TO WORK
                FilmImage filmImage = db.FilmImages.Where(x => x.ImageId == role.FilmId).Single();

                //new FilmPersonRolViewModel object to then add to the list
                FilmPersonRoleViewModel toAdd = new FilmPersonRoleViewModel();

                toAdd.ThisFilmPersonRole = role;    //get the FilmPersonRole record
                toAdd.ThisFilm = film;              //get the film record
                toAdd.ThisPerson = person;          //get the person record
                toAdd.ThisFilmImage = filmImage;

                //add to the FilmPersonRoleList (list of ViewModel objects)
                FilmPersonRoleList.Add(toAdd);
            }

            //send the FilmPersonRoleListViewModel List to the View for display
            return View(FilmPersonRoleList);
        }

        // GET: FilmPersonRoles/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FilmPersonRole filmPersonRole = db.FilmPersonRoles.Find(id);
            if (filmPersonRole == null)
            {
                return HttpNotFound();
            }
            return View(filmPersonRole);
        }

        // GET: FilmPersonRoles/Create
        public ActionResult Create(int? FilmId, int? PersonId, bool isActor)
        {


            FilmPersonRoleViewModel filmPersonRoleViewModel = new FilmPersonRoleViewModel();
            FilmPersonRole filmPersonRole = new FilmPersonRole();
            Film film = new Film();
            FilmImage filmImage = new FilmImage();
            Person person = new Person();
            PersonImage personImage = new PersonImage();

            if (isActor == true)
            {
                filmPersonRole.IsActor = true;
                filmPersonRole.IsDirector = false;
            }
            else
            {
                filmPersonRole.IsActor = false;
                filmPersonRole.IsDirector = true;
            }

            //db.FilmImages.Where(x => x.ImageId == role.FilmId).Single();

            if (FilmId == null && PersonId == null)
            {
                //Return to home page as user is manipulating the url
                return View("~/Views/Home/Index.cshtml");
            }
            else if (FilmId == null || FilmId == 0)
            {
                filmPersonRole.PersonId = PersonId;
                person = db.Persons.Where(x => x.PersonId == PersonId).Single();
                personImage = db.PersonImages.Where(x => x.ImageId == person.ImageId).Single();

                //If FilmId is null then link the existing person to a film choice

                //Get film collection for drop down box
                var filmQuery = from m in db.Films
                                 orderby m.Title
                                 select m;

                ViewBag.Films = new SelectList(filmQuery, "FilmId", "Title", null);

            }
            else if (PersonId == null || PersonId == 0)
            {
                filmPersonRole.FilmId = FilmId;
                film = db.Films.Where(x => x.FilmId == FilmId).Single();
                filmImage = db.FilmImages.Where(x => x.ImageId == film.ImageId).Single();

                //If PersonId is null then link the existing film to a person choice

                if (isActor)
                {
                    //Get Actors collection for drop down box
                    var personQuery = from p in db.Persons
                                      where p.IsActor == true
                                      orderby p.LastName
                                      select new
                                      {
                                          FullName = p.FirstName + " " + p.LastName,
                                          p.PersonId
                                      };

                    ViewBag.Persons = new SelectList(personQuery, "PersonId", "FullName", null);

                }
                else
                {
                    //Get Directors collection for drop down box
                    var personQuery = from p in db.Persons
                                      where p.IsDirector == true
                                      orderby p.LastName
                                      select new
                                      {
                                          FullName = p.FirstName + " " + p.LastName,
                                          p.PersonId
                                      };

                    ViewBag.Persons = new SelectList(personQuery, "PersonId", "FullName", null);

                }

            }

            filmPersonRoleViewModel.ThisFilm = film;
            filmPersonRoleViewModel.ThisFilmImage = filmImage;
            filmPersonRoleViewModel.ThisFilmPersonRole = filmPersonRole;
            filmPersonRoleViewModel.ThisPerson = person;
            filmPersonRoleViewModel.ThisPersonImage = personImage;

            //generate the view
            return View(filmPersonRoleViewModel);
        }

        // POST: FilmPersonRoles/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(FilmPersonRoleViewModel filmPersonRoleViewModel)
        {
            //flag if the action was called from the film edit view or the person edit view.
            bool isFromFilmView;

            if (filmPersonRoleViewModel.ThisFilm.FilmId == null || filmPersonRoleViewModel.ThisFilm.FilmId == 0)
            {
                filmPersonRoleViewModel.ThisFilmPersonRole.FilmId = Int32.Parse(Request["Films"]);

                filmPersonRoleViewModel.ThisFilmPersonRole.PersonId = filmPersonRoleViewModel.ThisPerson.PersonId;

                isFromFilmView = false;
            }
            else
            {
                filmPersonRoleViewModel.ThisFilmPersonRole.FilmId = filmPersonRoleViewModel.ThisFilm.FilmId;

                filmPersonRoleViewModel.ThisFilmPersonRole.PersonId = Int32.Parse(Request["Persons"]);

                isFromFilmView = true;
            }


                db.FilmPersonRoles.Add(filmPersonRoleViewModel.ThisFilmPersonRole);
                db.SaveChanges();

            if (isFromFilmView)
            {
                return RedirectToAction("Index", "Films");
            }
            else
            {
                return RedirectToAction("Index", "Persons");
            }          
        }

        // GET: FilmPersonRoles/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FilmPersonRole filmPersonRole = db.FilmPersonRoles.Find(id);
            if (filmPersonRole == null)
            {
                return HttpNotFound();
            }

            //code to generate dropdowns
            //FILMS ---------------------------------------------------------------------------------------------
            //from the Films model DbSet
            //select all columns from the database
            //orderby the film title
            var filmQuery = from m in db.Films
                            orderby m.Title
                            select m;
            //construct full films dropdown list preselected with the foreign key
            //do so from the query results and display the Film Title
            //store in FilmId in the ViewBag
            ViewBag.FilmId = new SelectList(filmQuery, "FilmId", "Title", filmPersonRole.FilmId);


            //PERSONS --------------------------------------------------------------------------------------------
            //from the Persons model DbSet
            //select the firstname and lastname as a new field called Name
            //and the person id - order by the lastname
            var personsQuery = from p in db.Persons
                               orderby p.LastName
                               select new
                               {
                                   Name = p.FirstName + " " + p.LastName,
                                   p.PersonId
                               };

            //construct full films dropdown list preselected with the foreign key
            //do so from the query results and display the Name (combined above)
            //store in FilmId in the ViewBag
            ViewBag.PersonId = new SelectList(personsQuery, "PersonId", "Name", filmPersonRole.PersonId);

            return View(filmPersonRole);
        }

        // POST: FilmPersonRoles/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "RoleId,PersonId,FilmId,IsActor,IsDirector")] FilmPersonRole filmPersonRole)
        {
            if (ModelState.IsValid)
            {
                db.Entry(filmPersonRole).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(filmPersonRole);
        }

        // GET: FilmPersonRoles/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FilmPersonRole filmPersonRole = db.FilmPersonRoles.Find(id);
            if (filmPersonRole == null)
            {
                return HttpNotFound();
            }
            return View(filmPersonRole);
        }

        // POST: FilmPersonRoles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            FilmPersonRole filmPersonRole = db.FilmPersonRoles.Find(id);
            db.FilmPersonRoles.Remove(filmPersonRole);
            db.SaveChanges();
            return RedirectToAction("Index", "Films");
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
