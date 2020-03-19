using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Filmster.Models.ViewModels
{
    public class FilmViewModel
    {
        public Film ThisFilm { get; set; }

        public FilmImage ThisFilmImage { get; set; }

        public Genre ThisGenre { get; set; }

        public Certificate ThisCertificate { get; set; }

        public List<ReviewViewModel> ThisFilmReviews { get; set; }

        public List<FilmPersonRoleViewModel> ThisFilmPersonRoleViewModel { get; set; }
    }
}