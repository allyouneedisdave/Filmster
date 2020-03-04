using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Filmster.Models
{
    [Table("FILM_PERSON_ROLE")]
    public class FilmPersonRole
    {
        [Key]
        [Column("role_id")]
        public int? RoleId { get; set; }

        [Column("person_id")]
        public int? PersonId { get; set; }

        [Column("film_id")]
        public int? FilmId { get; set; }

        [Column("is_actor")]
        public bool IsActor { get; set; }

        [Column("is_director")]
        public bool IsDirector { get; set; }


    }
}