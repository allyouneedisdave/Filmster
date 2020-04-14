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

        // Autocomplete jQuery UI plugin, accessed via AJAX.
        // Gets all films from db and compares the title against
        // the search term. Matches are converted to JSON and returned to UI.
        // First and Last names will be searched.
        public ActionResult Search(string term)
        {
            var persons = from p in db.Persons
                        select new
                        {
                            id = p.PersonId,
                            label = p.FirstName + " " + p.LastName
                        };

            persons = persons.Where(p => p.label.Contains(term));

            return Json(persons, JsonRequestBehavior.AllowGet);
        }





        // Get persons for the index view and builds a list of person viewmodels to return.
        // Utilises arguments to return a sorting order, page of results
        // and searched results. The sort order is tracked by the viewbag.
        // A column clicked argument is also passed to sort by different columns.
        public ActionResult Index(string sortOrder, string searchString, string columnClicked,
                            string currentFilter, int? page)
        {
            // This section sets the search, ordering and pagination variables
            // from the values sent in the ActionResult arguments.
            ViewBag.CurrentSort = sortOrder;

            ViewBag.TitleSortParam = String.IsNullOrEmpty(sortOrder) ? "title_desc" : "";


            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }
  
            ViewBag.CurrentFilter = searchString;

            List<PersonViewModel> personsList = new List<PersonViewModel>();

            // Get all persons, then overwrite with returned results if a searchString
            // is specified.
            var persons = from p in db.Persons
                        select p;


            if (!String.IsNullOrEmpty(searchString))
            {
                persons = persons.Where(p => p.FirstName.Contains(searchString) || p.LastName.Contains(searchString));
            }

            // This switch allows for first name and last name columns to be sorted.
            switch (columnClicked)
            {
                case "First Name":
                    if (sortOrder == "title_desc")
                    {
                        // Order by First Name descending
                        persons = persons.OrderByDescending(p => p.FirstName);
                    }
                    else
                    {
                        // Order by First Name ascending
                        persons = persons.OrderBy(p => p.FirstName);
                    }
                    break;
                case "Last Name":
                    if (sortOrder == "title_desc")
                    {
                        // Order by Last Name descending
                        persons = persons.OrderByDescending(p => p.LastName);
                    }
                    else
                    {
                        // Order by Last Name ascending
                        persons = persons.OrderBy(p => p.LastName);
                    }
                    break;
                default:
                    // Order by first name ascending as default.
                    persons = persons.OrderBy(p => p.FirstName);
                    break;
            }


            List<Person> thesePersons = new List<Person>();
            thesePersons = persons.ToList();

            // Loop through persons results and get viewmodel information from other db entities.
            // Assign the entities to the new viewmodel and add the viewmodel to the viewmodels list.
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

            // Set the amount of records shown on a page and convert the viewmodel
            // to an IpagedList object to return to the view.
            int pageSize = 5;
            //if page is null set to 1 otherwise keep page value
            int pageNumber = (page ?? 1);

            IPagedList pagedList = personsList.ToPagedList(pageNumber, pageSize);

            return View(pagedList);

        }

        // Get persons details by person id, builds a viewmodel for return.
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

            // IF a role exists for this person, add it to the roles list.
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

        // POST: Creates a new person and image record from viewmodel data.
        // Validates input values and abandons db insert if data is missing or incorrect.

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "PersonId,ImageId,FirstName,LastName," +
            "IsActor,IsDirector,Biography")] Person person,
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
                        if (Request.Files.Count > 0)
                        {
                            // If an image is being uploaded then save the image to a temporary folder, 
                            // convert the image to bytes and assign it to the model for inserting into the db.

                            var file = Request.Files[0];
                            {
                                // Save image to temp folder.
                                var fileName = Path.GetFileName(file.FileName);
                                var path = Path.Combine(Server.MapPath("~/ImagesTemp/"), fileName);
                                file.SaveAs(path);

                                // Convert image to bytes.
                                Image newImage = Image.FromFile(path);
                                PersonImage personImage = new PersonImage();
                                personImage.ImageBytes = personImage.ConvertImageToByteArray(newImage);

                                // Insert image into db and return the new image id.
                                db.PersonImages.Add(personImage);
                                db.SaveChanges();
                                int imageId = personImage.ImageId;
                                person.ImageId = imageId;

                                // Attempt to delete temporary image.
                                if (System.IO.File.Exists(path))
                                {
                                    try
                                    {
                                        System.IO.File.Delete(path);
                                    }
                                    catch (Exception)
                                    {
                                        // Do nothing, temporary folder will be 
                                        // cleared when application is re-launched.

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
                // Add the person to the database and save
                db.Persons.Add(person);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(person);
        }

        // Get viewmodel data for the edit view.
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


            // IF a role exists for this person, add it to the roles list.
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

        // POST: Update person record with new person data from the viewmodel.
        // Validates input data and aborts the update if data is invalid.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PersonViewModel personViewModel, HttpPostedFileBase upload)
        {
            if (ModelState.IsValid)
            {
                personViewModel.ThisPerson.ImageId = personViewModel.ThisPersonImage.ImageId;

                if (upload != null && upload.ContentLength > 0)
                {
                  
                    if (upload.ContentType == "image/jpeg" ||
                        upload.ContentType == "image/jpg" ||
                        upload.ContentType == "image/gif" ||
                        upload.ContentType == "image/png")
                    {
                        // If an image is being uploaded then save the image to a
                        // temporary folder, convert the image to bytes, save the image
                        // as a new record and assign its new id to the person model for
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
                                PersonImage personImage = new PersonImage();
                                personImage.ImageBytes = personImage.ConvertImageToByteArray(newImage);

                                // Insert image into db and return the new image id.
                                db.PersonImages.Add(personImage);
                                db.SaveChanges();
                                int imageId = personImage.ImageId;

                                //Get the old person image and delete it.
                                PersonImage oldPersonImage = db.PersonImages.Where(x => x.ImageId == personViewModel.ThisPersonImage.ImageId).Single();
                                db.PersonImages.Remove(oldPersonImage);

                                personViewModel.ThisPerson.ImageId = imageId;

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
                db.Entry(personViewModel.ThisPerson).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(personViewModel);
        }

        // GET: Gets the person data in preparation for deletion.
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

        // POST: Attempt to delete the person and image.
        // Any film roles for this persons will also be deleted.

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Person person = db.Persons.Find(id);

            List<FilmPersonRole> roles = db.FilmPersonRoles.Where(x => x.PersonId == person.PersonId).ToList();
            PersonImage personImage = db.PersonImages.Where(x => x.ImageId == person.ImageId).Single();

            // Delete the actor/director roles to prevent orphan data.
            if (roles.Count > 0)
            {
                foreach (FilmPersonRole role in roles)
                {
                    db.FilmPersonRoles.Remove(role);
                }
            }

            db.PersonImages.Remove(personImage);
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
