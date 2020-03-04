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
        public int genre_id { get; set; }

        [Column("genre")]
        public string genre { get; set; }
    }
}