using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Filmster.Models.ViewModels
{
    public class FilmViewModel
    {
        public Film ThisFilm;

        public FilmImage ThisFilmImage;

        public Genre ThisGenre;

        public Certificate ThisCertificate;

        public List<Review> ThisFilmReviews;
    }
}