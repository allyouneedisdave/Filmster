using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Filmster.Models
{
    [Table("REVEIW")]
    public class Review
    {
        [Key]
        [Column("review_id")]
        public int ReviewId { get; set; }

        [Column("film_id")]
        public int FilmId { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("title")]
        public string ReviewTitle { get; set; }

        [Column("detail")]
        public string ReviewDetail { get; set; }

        [Column("created_date")]
        public DateTime CreatedDate { get; set; }

        [Column("rating")]
        public int Rating { get; set; }
    }
}