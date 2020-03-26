using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Filmster.Models.ViewModels
{
    public class FilmPersonRoleViewModel
    {
        public FilmPersonRole ThisFilmPersonRole { get; set; }

        public Person ThisPerson { get; set; }

        public PersonImage ThisPersonImage { get; set; }

        public Film ThisFilm { get; set; }

        public FilmImage ThisFilmImage { get; set; }

        public Genre ThisGenre { get; set; }

        public Certificate ThisCertificate { get; set; }
    }
}