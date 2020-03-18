using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Filmster.Models
{
    [Table("GENRE")]
    public class Genre
    {
        [Key]
        [Column("genre_id")]
        public int GenreId { get; set; }

        [Column("genre")]
        public string GenreName { get; set; }
    }
}