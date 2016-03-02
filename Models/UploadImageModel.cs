using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace CutImage.Models
{
    public class UploadImageModel
    {
        public string HeadFileName { get; set; }
     
        public int X { get; set; }

       
        public int Y { get; set; }

       
        public int Width { get; set; }

       
        public int Height { get; set; }
    }
}