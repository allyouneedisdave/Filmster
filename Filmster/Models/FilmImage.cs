using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;

namespace Filmster.Models
{
    [Table("FILM_IMAGE")]
    public class FilmImage
    {
        [Key]
        [Column("image_id")]
        public int ImageId { get; set; }

        [Column("image")]
        public byte[] ImageBytes { get; set; }


        public Image imageBitmap
        {
            get
            {
                return ConvertByteArrayToImage(ImageBytes);
            }
        }

        //Convert an image into a byte array.
        public byte[] ConvertImageToByteArray(System.Drawing.Image inputImage)
        {
            MemoryStream mStream = new MemoryStream();
            inputImage.Save(mStream, ImageFormat.Jpeg);
            return mStream.ToArray();
        }

        public Image ConvertByteArrayToImage(byte[] inputBytes)
        {
            MemoryStream mStream = new MemoryStream(inputBytes);
            Image outputImage = Image.FromStream(mStream);
            return outputImage;
        }
        

    }
}