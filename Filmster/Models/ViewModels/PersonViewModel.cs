using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Filmster.Models.ViewModels
{
    public class PersonViewModel
    {
        public Person ThisPerson { get; set; }

        public PersonImage ThisPersonImage { get; set; }

        public List<FilmPersonRoleViewModel> ThisFilmPersonRolesViewModel { get; set; }

           
    }
}