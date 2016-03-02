using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace CutImage.Controllers
{
    public class ImageController : Controller
    {
        //
        // GET: /Image/
        public ActionResult Index()
        {
            return View();
        }


        // 上传头像，保存到临时文件夹
        [HttpPost]
        public ActionResult UploadHead(HttpPostedFileBase head)
        {
            try
            {
                if ((head == null))
                {
                    return Json(new { msg = 0 });
                }
                else
                {
                    var supportedTypes = new[] { "jpg", "jpeg", "png", "gif", "bmp" };
                    var fileExt = System.IO.Path.GetExtension(head.FileName).Substring(1);
                    if (!supportedTypes.Contains(fileExt))
                    {
                        return Content(Newtonsoft.Json.JsonConvert.SerializeObject(new { msg = -1 }));
                    }
                    if (head.ContentLength > 1024 * 1000 * 3) // 图片大小3M
                    {
                        return Content(Newtonsoft.Json.JsonConvert.SerializeObject(new { msg = -2 }));
                    }
                    Random r = new Random();
                    var filename = DateTime.Now.ToString("yyyyMMddHHmmss") + r.Next(10000) + "." + fileExt;
                    var customPath = @"\temp";
                    var savePath = System.Configuration.ConfigurationManager.AppSettings["SysDataPath"] + customPath;
                    ViewBag.Size = head.ContentLength;
                    #region 验证并创建文件上传路径
                    if (System.Configuration.ConfigurationManager.AppSettings["VirtualPath"] == "true")
                        CheckDirect(Server.MapPath(savePath));
                    else
                        CheckDirect(savePath);
                    #endregion
                    // 保存文件
                    if (System.Configuration.ConfigurationManager.AppSettings["VirtualPath"].ToLower() == "true")
                        head.SaveAs(Path.Combine(Server.MapPath(savePath), filename));
                    else
                        head.SaveAs(Path.Combine(savePath, head.FileName));
                    return Content(Newtonsoft.Json.JsonConvert.SerializeObject(new { msg = filename }));
                }
            }
            catch (Exception)
            {
                return Content(Newtonsoft.Json.JsonConvert.SerializeObject(new { msg = -3 }));
            }
        }


        #region Private
        /// <summary>
        /// 附件保存路径是否是虚拟路径
        /// </summary>
        //public string Config_IsVirtualPath
        //{
        //    get
        //    {
        //        string path = "~/xml/customer/config/" + Session["SiteID"].ToString() + ".xml";
        //        XmlDocument xmldom = new XmlDocument();
        //        xmldom.Load(System.Web.HttpContext.Current.Server.MapPath(path));//加载xml文件

        //        XmlNode xmlNode = xmldom.SelectSingleNode("config");//读取第一个节点
        //        #region 加载文件配置信息
        //        XmlNode fileConfig = xmlNode.SelectSingleNode("file");

        //        return fileConfig.SelectNodes("IsVirtualPath")[0].InnerText;
        //        #endregion
        //    }
        //}
        ///// <summary>
        ///// 上传文件路径
        ///// </summary>
        //public string Config_SysDataPath
        //{
        //    get
        //    {
        //        string path = "~/xml/customer/config/" + ".xml";
        //        XmlDocument xmldom = new XmlDocument();
        //        xmldom.Load(System.Web.HttpContext.Current.Server.MapPath(path));//加载xml文件

        //        XmlNode xmlNode = xmldom.SelectSingleNode("config");//读取第一个节点
        //        #region 加载文件配置信息
        //        XmlNode fileConfig = xmlNode.SelectSingleNode("file");

        //        return fileConfig.SelectNodes("SysDataPath")[0].InnerText;
        //        #endregion
        //    }
        //}
        #endregion


        #region  剪裁头像相关
        /// <summary>
        /// 验证并创建保存路径
        /// </summary>
        /// <param name="path"></param>
        private void CheckDirect(string path)
        {
            DirectoryInfo di = new DirectoryInfo(path);
            if (!di.Exists)
            {
                di.Create();
            }
        }
        /// <summary>
        /// 创建缩略图
        /// </summary>
        public string CutAvatar(string imgSrc, int x, int y, int width, int height, long Quality, string SavePath, int t)
        {
            Image original = Image.FromFile(imgSrc);

            Bitmap img = new Bitmap(t, t, PixelFormat.Format24bppRgb);

            img.MakeTransparent(img.GetPixel(0, 0));
            img.SetResolution(72, 72);
            using (Graphics gr = Graphics.FromImage(img))
            {
                if (original.RawFormat.Equals(ImageFormat.Jpeg) || original.RawFormat.Equals(ImageFormat.Png) || original.RawFormat.Equals(ImageFormat.Bmp))
                {
                    gr.Clear(Color.Transparent);
                }
                if (original.RawFormat.Equals(ImageFormat.Gif))
                {
                    gr.Clear(Color.White);
                }

                gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gr.SmoothingMode = SmoothingMode.AntiAlias;
                gr.CompositingQuality = CompositingQuality.HighQuality;
                gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                gr.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                using (var attribute = new System.Drawing.Imaging.ImageAttributes())
                {
                    attribute.SetWrapMode(WrapMode.TileFlipXY);
                    gr.DrawImage(original, new Rectangle(0, 0, t, t), x, y, width, height, GraphicsUnit.Pixel, attribute);
                }
            }
            ImageCodecInfo myImageCodecInfo = GetEncoderInfo("image/jpeg");
            if (original.RawFormat.Equals(ImageFormat.Jpeg))
            {
                myImageCodecInfo = GetEncoderInfo("image/jpeg");
            }
            else if (original.RawFormat.Equals(ImageFormat.Png))
            {
                myImageCodecInfo = GetEncoderInfo("image/png");
            }
            else if (original.RawFormat.Equals(ImageFormat.Gif))
            {
                myImageCodecInfo = GetEncoderInfo("image/gif");
            }
            else if (original.RawFormat.Equals(ImageFormat.Bmp))
            {
                myImageCodecInfo = GetEncoderInfo("image/bmp");
            }

            System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
            EncoderParameters myEncoderParameters = new EncoderParameters(1);
            EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, Quality);
            myEncoderParameters.Param[0] = myEncoderParameter;
            img.Save(SavePath, myImageCodecInfo, myEncoderParameters);
            img.Dispose();
            string imgLength = "1";
            FileInfo fi = new FileInfo(SavePath);

            if (fi.Exists)
            {
                imgLength = fi.Length.ToString();
            }
            fi.Refresh();
            return imgLength;
        }
        //根据长宽自适应 按原图比例缩放 
        private static Size GetThumbnailSize(System.Drawing.Image original, int desiredWidth, int desiredHeight)
        {
            var widthScale = (double)desiredWidth / original.Width;
            var heightScale = (double)desiredHeight / original.Height;
            var scale = widthScale < heightScale ? widthScale : heightScale;
            return new Size
            {
                Width = (int)(scale * original.Width),
                Height = (int)(scale * original.Height)
            };
        }
        private static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }
        /// <summary>
        /// 获取文件扩展名
        /// </summary>
        /// <returns></returns>
        private string GetExtension(HttpPostedFileBase file)
        {
            string extension = "";
            //获得文件扩展名 
            extension = System.IO.Path.GetExtension(file.FileName);//若为null,表明文件无后缀名; 
            if (!string.IsNullOrEmpty(extension))
            {
                extension = extension.ToLower();
            }
            return extension;
        }
        /// <summary>
        /// 上传文件重命名
        /// </summary>
        /// <returns>新文件名</returns>
        private string FileRename(string extName)
        {
            //保证重命名后的文件名唯一
            Random ra = new Random();
            string newName = DateTime.Now.ToString("yyyyMMddHHmmss") + DateTime.Now.Millisecond.ToString("000") + ra.Next(100);

            newName = newName + extName;//重命名后的新文件名
            return newName;

        }
        #endregion      
    }
}

