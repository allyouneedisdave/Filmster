using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Filmster.Models
{
    public class DBContext : DbContext
    {
        public DbSet<Film> Films { get; set; }

        public DbSet<Person> Persons { get; set; }

        public DbSet<FilmPersonRole> FilmPersonRoles { get; set; }

        public DbSet<FilmImage> FilmImages { get; set; }
    }
}