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
    }
}