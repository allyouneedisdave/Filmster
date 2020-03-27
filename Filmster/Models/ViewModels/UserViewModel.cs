using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Filmster.Models.ViewModels
{
    public class UserViewModel
    {
        public User thisUser { get; set; }

        public List<Review> thisReviews { get; set; }
    }
}