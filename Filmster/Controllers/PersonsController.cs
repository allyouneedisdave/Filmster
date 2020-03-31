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
using Filmster.Models;
using Filmster.Models.ViewModels;
using PagedList;

namespace Filmster.Controllers
{
    public class PersonsController : Controller
    {
        private DBContext db = new DBContext();

        //to be accessed via AJAX - autocomplete jQuery UI plugin
        public ActionResult Search(string term)
        {
            //select all the persons in the db
            //and get the id and title only
            //id and label used for autocomplete functionality
            var persons = from p in db.Persons
                        select new
                        {
                            id = p.PersonId,
                            label = p.FirstName + " " + p.LastName
                        };
            //now check the searchString given for any matches in title
            persons = persons.Where(p => p.label.Contains(term));

            //convert to and return the JSON for the search UI
            return Json(persons, JsonRequestBehavior.AllowGet);
        }





        // GET: Persons
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
            if (searchString != null)
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


            List<PersonViewModel> personsList = new List<PersonViewModel>();

            //List<Film> films;

            //Select all films in the db
            var persons = from p in db.Persons
                        select p;

            //check if the search string is not empty
            if (!String.IsNullOrEmpty(searchString))
            {
                //if we have a search term, then select where the title contains it
                //analogous to LIKE %term% in SQL
                persons = persons.Where(p => p.FirstName.Contains(searchString) || p.LastName.Contains(searchString));
            }


            //check the sortOrder param
            switch (sortOrder)
            {
                case "title_desc":
                    //order by title descending
                    persons = persons.OrderByDescending(p => p.FirstName);
                    break;
                default:
                    //order by title ascending
                    persons = persons.OrderBy(p => p.FirstName);
                    break;

            }

            List<Person> thesePersons = new List<Person>();
            thesePersons = persons.ToList();




            if (thesePersons.Count > 0)
            {
                PersonViewModel personViewModel;

                foreach (Person p in thesePersons)
                {
                   
                    PersonImage personImage = db.PersonImages.Where(x => x.ImageId == p.ImageId).Single();

                    personViewModel = new PersonViewModel();
                    personViewModel.ThisPerson = p;
                    personViewModel.ThisPersonImage = personImage;

                    personsList.Add(personViewModel);
                }
           
            }

            //how many records per page (could also be a param..)
            int pageSize = 5;
            //if page is null set to 1 otherwise keep page value
            int pageNumber = (page ?? 1);

            IPagedList pagedList = personsList.ToPagedList(pageNumber, pageSize);

            //send the updated films list to the view
            return View(pagedList);

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
                        if (Request.Files.Count > 0)
                        {
                            var file = Request.Files[0];
                            {
                                var fileName = Path.GetFileName(file.FileName);
                                var path = Path.Combine(Server.MapPath("~/ImagesTemp/"), fileName);
                                file.SaveAs(path);

                                Image newImage = Image.FromFile(path);
                                PersonImage personImage = new PersonImage();
                                personImage.ImageBytes = personImage.ConvertImageToByteArray(newImage);


                                db.PersonImages.Add(personImage);
                                db.SaveChanges();
                                int imageId = personImage.ImageId;
                                person.ImageId = imageId;

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

            PersonViewModel personViewModel = new PersonViewModel();

            PersonImage personImage = db.PersonImages.Where(x => x.ImageId == person.ImageId).Single();

            List<FilmPersonRoleViewModel> personRoleViewModelList = new List<FilmPersonRoleViewModel>();

            List<FilmPersonRole> rolesList = new List<FilmPersonRole>();
            rolesList = db.FilmPersonRoles.ToList();

            // For each role, do stuff if it matches the person id.
            if (rolesList.Count > 0)
            {
                foreach (FilmPersonRole role in rolesList)
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

        // POST: Persons/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PersonViewModel personViewModel, HttpPostedFileBase upload)
        {
            if (ModelState.IsValid)
            {
                personViewModel.ThisPerson.ImageId = personViewModel.ThisPersonImage.ImageId;

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
                        if (Request.Files.Count > 0)
                        {
                            var file = Request.Files[0];
                            {
                                var fileName = Path.GetFileName(file.FileName);
                                var path = Path.Combine(Server.MapPath("~/ImagesTemp/"), fileName);
                                file.SaveAs(path);

                                Image newImage = Image.FromFile(path);
                                PersonImage personImage = new PersonImage();
                                personImage.ImageBytes = personImage.ConvertImageToByteArray(newImage);


                                db.PersonImages.Add(personImage);
                                db.SaveChanges();
                                int imageId = personImage.ImageId;
                                personViewModel.ThisPerson.ImageId = imageId;


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

                db.Entry(personViewModel.ThisPerson).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(personViewModel);
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
