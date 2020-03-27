using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Filmster.Models.ViewModels
{
    public class ReviewViewModel
    {
        public Review thisReview { get; set; }

        public User thisUser { get; set; }

        public Film thisFilm { get; set; }
    }
}