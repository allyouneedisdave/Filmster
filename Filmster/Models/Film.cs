using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Filmster.Models
{
    [Table("FILM")]
    public class Film
    {
        [Column("film_id")]
        public int FilmID { get; set; }
        [Column("genre_id")]
        public int GenreID { get; set; }
        [Column("certificate_id")]
        public int CertificateID { get; set; }
        [Column("image_Id")]
        public int ImageID { get; set; }
        [Column("title")]
        public string Title { get; set; }
        [Column("synopsis")]
        public string Synopsis { get; set; }
        [Column("runtime")]
        public int Runtime { get; set; }
        [Column("release_date")]
        public DateTime ReleaseDate { get; set; }


        public string FilmDescTrimmed;
    }
}