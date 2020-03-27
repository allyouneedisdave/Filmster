using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Filmster.Models
{
    [Table("REVIEW")]
    public class Review
    {
        [Key]
        [Column("review_id")]
        public int ReviewId { get; set; }

        [Column("film_id")]
        public int FilmId { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Display(Name ="Review Title")]
        [Column("title")]
        public string ReviewTitle { get; set; }

        [Display(Name = "Review Details")]
        [Column("detail")]
        public string ReviewDetail { get; set; }

        [Display(Name = "Created Date")]
        [Column("created_date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}",
            ApplyFormatInEditMode = true)]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Rating")]
        [Column("rating")]
        public int Rating { get; set; }


        public string ReviewShortDetail
        {
            //get only
            get
            {
                //if the length of the desc is greater than 100 characters
                if ((ReviewDetail.Length) > 100)
                {
                    //get a substring of the first 100 characters followed by ellipses
                    return $"{ReviewDetail.Substring(0, 100)} ...";
                }
                else
                {
                    //otherwise return the full description
                    return ReviewDetail;
                }
            }
        }
    }
}