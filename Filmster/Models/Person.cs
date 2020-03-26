using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Filmster.Models
{
    [Table("PERSON")]
    public class Person
    {
        [Key]
        [Column("person_id")]
        public int? PersonId { get; set; }

        [Column("image_id")]
        public int? ImageId { get; set; }



        [Required]
        [Column("first_name")]
        [Display(Name ="First Name")]
        public string FirstName { get; set; }

        [Required]
        [Column("last_name")]
        [Display(Name ="Last Name")]
        public string LastName { get; set; }

        [Column("is_actor")]
        [Display(Name ="Actor?")]
        public bool IsActor { get; set; }

        [Column("is_director")]
        [Display(Name = "Director?")]
        public bool IsDirector { get; set; }

        [Required]
        [Column("biography")]
        [Display(Name ="Biography")]
        [DataType(DataType.MultilineText)]
        public string Biography { get; set; }

        public string PersonShortBio
        {
            //get only
            get
            {
                //if the length of the desc is greater than 100 characters
                if ((Biography.Length) > 100)
                {
                    //get a substring of the first 100 characters followed by ellipses
                    return $"{Biography.Substring(0, 100)} ...";
                }
                else
                {
                    //otherwise return the full description
                    return Biography;
                }
            }
        }


        public string PersonFullName
        {
            get
            {
                return $"{FirstName} {LastName}";
            }
        }
    }
}