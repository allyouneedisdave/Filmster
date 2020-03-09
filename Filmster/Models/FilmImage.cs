using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Filmster.Models
{
    [Table("FILM_IMAGE")]
    public class FilmImage
    {
        [Key]
        [Column("image_id")]
        [Display(Name ="Image Id")]
        public int ImageId { get; set; }

        [Column("image")]
        [Display(Name ="Image")]
        public byte[] ImageBytes { get; set; }
    }
}