using HtmlAgilityPack;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace InMemoryELP.Models
{
    public class ImageBlobContext
    {
        const string TempContainerName = "temp";

        public ImageBlobContext(string StorageConnectionString)
        {
            this.storageAccount = CloudStorageAccount.Parse(StorageConnectionString);
        }

        public ImageBlobContext(CloudStorageAccount cloudStorageAccount)
        {
            this.storageAccount = cloudStorageAccount;
        }

        private CloudStorageAccount storageAccount;

        private CloudBlobClient getBlobClient(){
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            return blobClient;
        }

        private CloudBlobContainer getContainer(string name, bool openToPublic = false)
        {
            var blobClient = getBlobClient();

            // Retrieve a reference to a container. 
            CloudBlobContainer container = blobClient.GetContainerReference(name);

            // Create the container if it doesn't already exist.
            container.CreateIfNotExists();


            //Make it public?
            var permisssions = new BlobContainerPermissions();
            if (openToPublic)
            {
                permisssions.PublicAccess = BlobContainerPublicAccessType.Blob;

            }
            else
            {
                permisssions.PublicAccess = BlobContainerPublicAccessType.Off;
            }

            container.SetPermissions(permisssions);

            return container;
        }

        
        public string SaveTempImage(HttpPostedFileBase file)
        {
            if (!ValidMimeTypes().Contains(file.ContentType))
            {
                throw new Exception("Cannot upload image - file does not appear to be a valid .jpg, .bmp, or .png");
            }

            var blobName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);


            var container = getContainer(TempContainerName);

            CloudBlockBlob blob = container.GetBlockBlobReference(blobName);
            blob.Properties.ContentType = file.ContentType;

            blob.UploadFromStream(file.InputStream);
                        
            //Set the expiry time and permissions for the blob.
            //In this case the start time is specified as a few minutes in the past, to mitigate clock skew.
            //The shared access signature will be valid immediately.
            SharedAccessBlobPolicy sasConstraints = new SharedAccessBlobPolicy();
            sasConstraints.SharedAccessStartTime = DateTime.UtcNow.AddMinutes(-5);
            sasConstraints.SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(30);
            sasConstraints.Permissions = SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.List;

            //Generate the shared access signature on the blob, setting the constraints directly on the signature.
            string sasBlobToken = blob.GetSharedAccessSignature(sasConstraints);

            //Return the URI string for the container, including the SAS token.
            return blob.Uri + sasBlobToken;
        }

        /// <summary>
        /// This is used to move all the images in a story from temp to perm storage containers.
        /// </summary>
        /// <param name="story">the story whose images need to be committed</param>
        /// <returns>a modified story</returns>
        public Story CommittStoryImages(Story story)
        {
            var tempContainer = getContainer(TempContainerName);
            var container = getContainer(story.RowKey, true);


            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(story.HTMLBodyText);

            var imgs = doc.DocumentNode.SelectNodes("//img");

            //Find the srcs in the HTML, move the blobs, update the HTML
            if (imgs != null)
            {
                foreach (var img in imgs)
                {
                    var src = img.Attributes["src"].Value.Replace("\"", "");

                    var uri = new Uri(src);
                    var pathWithoutSAS = uri.GetLeftPart(UriPartial.Path);
                    CloudBlockBlob TempBlockBlob = new CloudBlockBlob(new Uri(pathWithoutSAS));

                    CloudBlockBlob blockBlob = container.GetBlockBlobReference(TempBlockBlob.Name);

                    blockBlob.StartCopyFromBlob(TempBlockBlob);

                    story.HTMLBodyText = story.HTMLBodyText.Replace(src, blockBlob.Uri.AbsoluteUri.ToString());
                }
            }


            //Reset the images array
            var imgSrcs = new List<string>();
            doc = new HtmlDocument();
            doc.LoadHtml(story.HTMLBodyText);

            imgs = doc.DocumentNode.SelectNodes("//img");

            if (imgs != null)
            {
                foreach (var img in imgs)
                {
                    imgSrcs.Add(img.Attributes["src"].Value.Replace("\"", ""));
                }
            }

            story.ImageUrls = Newtonsoft.Json.JsonConvert.SerializeObject(imgSrcs);

            return story;
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
}