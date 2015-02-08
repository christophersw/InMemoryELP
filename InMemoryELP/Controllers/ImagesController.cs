using InMemoryELP.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace InMemoryELP.Controllers
{
    public class ImagesController : Controller
    {
        private ImageBlobContext context = new ImageBlobContext(ConfigurationManager.AppSettings["StorageConnectionString"]);

        public JsonResult Upload()
        {
            List<string> fileIds = new List<string>();

            try
            {
                for (int i = 0; i < Request.Files.Count; i++)
                {
                    var fileId = Guid.NewGuid();
                    string folder = DateTime.Now.ToString("dd-MMM-yyyy");

                    if (!Directory.Exists(Server.MapPath("~/Content/Images/" + folder)))
                    {
                        Directory.CreateDirectory(Server.MapPath("~/Content/Images/" + folder));
                    }

                    HttpPostedFileBase file = Request.Files[i]; //Uploaded file
                    
                    //Use the following properties to get file's name, size and MIMEType
                    int fileSize = file.ContentLength;
                    string fileName = file.FileName;
                    string mimeType = file.ContentType;

                    if (!ValidMimeTypes().Contains(mimeType))
                    {
                        throw new Exception("Cannot upload image - file does not appear to be a valid .jpg, .bmp, or .png");
                    }
                                   

                    //To save file, use SaveAs method
                    var uriString = context.SaveTempImage(file);

                    //Return the location of the image...
                    fileIds.Add(uriString);
                }

                var model = new UploadAPI()
                {
                    message = null,
                    success = true,
                    result = fileIds
                };

                return Json(model);
            }
            catch (Exception e)
            {
                var model = new UploadAPI()
                {
                    message = e.Message,
                    success = false,
                    result = new List<string>()
                };
                return Json(model);
            }

        }

        public static List<string> ValidMimeTypes()
        {
            var model = new List<string>();
            model.Add("image/jpeg");
            model.Add("image/bmp");
            model.Add("Image/png");
            return model;
        }
    }

    [Serializable]
    public class UploadAPI
    {
        public bool success {get;set;}
        public string message {get;set;}
        public List<string> result {get;set;}
    }


}