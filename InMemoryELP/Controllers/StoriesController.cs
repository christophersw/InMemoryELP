using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using InMemoryELP.Models;
using System.Text;
using HtmlAgilityPack;
using System.Configuration;
using InMemoryELP.Helpers;

namespace InMemoryELP.Controllers
{
    public class StoriesController : Controller
    {
        private StoryTableContext context = new StoryTableContext(ConfigurationManager.AppSettings["StorageConnectionString"]);

        // GET: Stories
        public ActionResult Index()
        {
            return View(context.GetPublicStories());
        }

        public ActionResult Story(Guid id)
        {
            var model = context.GetPublicStory(id);
            if (model != null)
            {
                return View(model);
            }
            else
            {
                //Check for private stories if Family...
                if (User.IsInRole("Family"))
                {
                    model = context.GetPrivateStory(id);

                    if (model != null)
                    {
                        return View(model);
                    }
                }

                Response.StatusCode = 404;
                return Content("Not Found");
            }
        }

        public ActionResult Name(string id)
        {
            var model = context.GetPublicStory(id);
            if (model != null)
            {
                return View("Story", model);
            }
            else
            {
                //Check for private stories if Family...
                if (User.IsInRole("Family"))
                {
                    model = context.GetPrivateStory(id);

                    if (model != null)
                    {
                        return View("Story", model);
                    }
                }

                Response.StatusCode = 404;
                return Content("Not Found");
            }
        }
               
        [ChildActionOnly]
        [Authorize(Roles="Family")]
        public ActionResult PendingApproval()
        {
            var model = context.GetUnapprovedStories();
            return View(model);
        }

        [ChildActionOnly]
        [Authorize(Roles = "Family")]
        public ActionResult PrivateStories()
        {
            var model = context.GetPrivateStories();
            return View(model);
        }

        [Authorize (Roles="Family")]
        public ActionResult RevokeApproval(Guid id)
        {

            StoryViewModel storyToUpdate;

            var publicStory = context.GetPublicStory(id);

            if (publicStory != null)
            {
                storyToUpdate = publicStory;
            } 
            else
            {
                storyToUpdate = context.GetPrivateStory(id);
                if (storyToUpdate == null)
                {
                    Response.StatusCode = 404;
                    return Content("Not Found");
                }
            }

            storyToUpdate.Approved = false;

            if (context.UpdateStory(storyToUpdate))
            {
                return RedirectToAction("Story", new { id = id });
            }
            else
            {
                Response.StatusCode = 500;
                return Content("Server Error");
            }  
        }

        [Authorize(Roles="Family")]
        public ActionResult Approve(Guid id)
        {

            StoryViewModel storyToUpdate;

            var publicStory = context.GetPublicStory(id);

            if (publicStory != null)
            {
                storyToUpdate = publicStory;
            }
            else
            {
                storyToUpdate = context.GetPrivateStory(id);
                if (storyToUpdate == null)
                {
                    Response.StatusCode = 404;
                    return Content("Not Found");
                }
            }

            storyToUpdate.Approved = true;

            if (context.UpdateStory(storyToUpdate))
            {
                return RedirectToAction("Story", new { id = id });
            }
            else
            {
                Response.StatusCode = 500;
                return Content("Server Error");
            }
        }

        [Authorize(Roles="Family")]
        public ActionResult Remove(Guid id){
            StoryViewModel storyToUpdate;

            var publicStory = context.GetPublicStory(id);

            if (publicStory != null)
            {
                storyToUpdate = publicStory;
            }
            else
            {
                storyToUpdate = context.GetPrivateStory(id);
                if (storyToUpdate == null)
                {
                    Response.StatusCode = 404;
                    return Content("Not Found");
                }
            }

            if (context.RemoveStory(storyToUpdate))
            {
                return RedirectToAction("Index", "Family", null);
            }
            else
            {
                Response.StatusCode = 500;
                return Content("Server Error");
            }
        }

        // GET: Stories/Create
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Upload(StoryViewModel model)
        {

            if (ModelState.IsValid)
            {
                model.Approved = false;
                model.Title = HtmlRemoval.StripTagsRegex(model.Title);
                context.AddStory(model);

                //Store the results by Mapping to a DB Object;
                return Json("");
            }
            else
            {
                StringBuilder result = new StringBuilder();

                result.Append("<p><ul>");

                foreach (var item in ModelState)
                {
                    string key = item.Key;
                    var errors = item.Value.Errors;

                    foreach (var error in errors)
                    {
                        result.Append("<li>" + error.ErrorMessage + "</li>");
                    }
                }

                result.Append("</p></ul>");

                Response.StatusCode = 400;


                return Json(new {message = result.ToString()});
            }
        }     
    }
}
