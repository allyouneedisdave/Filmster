using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;

namespace Filmster.Models
{
    [Table("PERSON_IMAGE")]
    public class PersonImage
    {
        [Key]
        [Column("image_id")]
        [Display(Name ="Image Id")]
        public int ImageId { get; set; }

        [Column("image")]
        [Display(Name ="Image")]
        public byte[] ImageBytes { get; set; }



        //Convert an image into a byte array.
        public byte[] ConvertImageToByteArray(System.Drawing.Image inputImage)
        {
            MemoryStream mStream = new MemoryStream();
            inputImage.Save(mStream, ImageFormat.Jpeg);
            return mStream.ToArray();
        }


    }
}