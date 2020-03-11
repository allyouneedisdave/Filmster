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

        public DbSet<Genre> Genres { get; set; }

        public DbSet<PersonImage> PersonImages { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<Review> Reviews { get; set; }
    }
}