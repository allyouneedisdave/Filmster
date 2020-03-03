﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Filmster.Models
{
    [Table("FILM")]
    public class Film
    {
        [Column("film_id")]
        public int? FilmID { get; set; }
        [Column("genre_id")]
        public int? GenreID { get; set; }
        [Column("certificate_id")]
        public int? CertificateID { get; set; }
        [Column("image_Id")]
        public int? ImageID { get; set; }



        [Required]
        [Display(Name = "Film Title")]
        [Column("title")]
        public string Title { get; set; }

        [Required]
        [Display(Name = "Synopsis")]
        [DataType(DataType.MultilineText)]
        [Column("synopsis")]
        public string Synopsis { get; set; }

        [Required]
        [Display(Name = "Runtime")]
        [Column("runtime")]
        public int? Runtime { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}",
            ApplyFormatInEditMode = true)]
        [Display(Name ="Release Date")]
        [Column("release_date")]
        public DateTime? ReleaseDate { get; set; }



    }
}