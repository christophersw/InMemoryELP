using HtmlAgilityPack;
using Microsoft.Security.Application;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace InMemoryELP.Models
{

    public class StoryContext : DbContext
    {
        public DbSet<Story> Stories { get; set; }
    }

    public class Story : TableEntity
    {
        public Story(StoryViewModel model)
        {
            if (model.Public)
            {
                this.PartitionKey = "Public";

            }
            else
            {
                this.PartitionKey = "Private";
            }

            if (model.Id.HasValue)
            {
                this.RowKey = model.Id.Value.ToString();
            }
            else
            {
                this.RowKey = Guid.NewGuid().ToString();
            }

            this.Title = Sanitizer.GetSafeHtmlFragment(model.Title);
            this.HTMLBodyText = Sanitizer.GetSafeHtmlFragment(model.HTMLBodyText);
            this.ImageUrls = Newtonsoft.Json.JsonConvert.SerializeObject(helpers.findImages(this.HTMLBodyText));
            this.Preview = helpers.generatePreview(this.HTMLBodyText);
            this.AuthorName = Sanitizer.GetSafeHtmlFragment(model.AuthorName);
            this.AuthorEmail = Sanitizer.GetSafeHtmlFragment(model.AuthorEmail);
            this.Approved = model.Approved;
        }

        public Story() { }

        public string Title { get; set; }

        public string HTMLBodyText {get;set;}

        public string ImageUrls { get; set; }

        public string AuthorName { get; set; }

        public string AuthorEmail { get; set; }

        public bool Approved { get; set; }

        public string Preview { get; set; }

        public static class helpers
        {
            public static string[] findImages(string html)
            {
                var model = new List<string>();

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);

                var imgs = doc.DocumentNode.SelectNodes("//img");

                if (imgs != null)
                {
                    foreach (var img in imgs)
                    {
                        model.Add(img.Attributes["src"].Value.Replace("\"", ""));
                    }
                }

                return model.ToArray<string>();
            }

            public static string generatePreview(string html, int maxChars = 140)
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);

                var root = doc.DocumentNode;
                var sb = new StringBuilder();
                foreach (var node in root.DescendantsAndSelf())
                {
                    if (!node.HasChildNodes)
                    {
                        string text = node.InnerText;
                        if (!string.IsNullOrEmpty(text))
                            sb.AppendLine(text.Trim());
                    }

                    if (sb.Length >= maxChars)
                    {
                        break;
                    }
                }

                if(sb.Length > maxChars)
                {
                    return sb.ToString().Substring(0, maxChars);
                }
                else
                {
                    return sb.ToString();
                }

            }
        }
    }

    public class StoryViewModel
    {

        public Guid? Id { get; set; }

        [AllowHtml]
        [Required]
        public string Title { get; set; }

        [AllowHtml]
        [Required(ErrorMessage="Please add some text to your story.")]
        public string HTMLBodyText { get; set; }

        [Required(ErrorMessage = "Please provide your name.")]
        public string AuthorName { get; set; }

        [Required(ErrorMessage = "Please provide a valid email.")]
        [EmailAddress(ErrorMessage = "The email provided appears to be invalid.")]
        public string AuthorEmail { get; set; }

        [Required(ErrorMessage = "Please indicate whether you want this publicly released.")]
        public bool Public { get; set; }

        public string[] ImageUrls { get; set; }

        public string Preview { get; set; }

        public bool Approved { get; set; }

    }

    
}