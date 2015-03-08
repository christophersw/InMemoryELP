using InMemoryELP.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace InMemoryELP.Controllers
{
    public class GuestbookController : Controller
    {
        private GuestbookTableContext context = new GuestbookTableContext(ConfigurationManager.AppSettings["StorageConnectionString"]);

        // GET: Guestbook
        [ChildActionOnly]
        public ActionResult Form()
        {
            var model = new GusetbookViewModel();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Submit(GusetbookViewModel model)
        {
            if (ModelState.IsValid)
            {
                context.AddEntry(model);
                return Content("<h3>Thanks!</h3>");
            }
            else
            {
                return View("Form", model);
            }
        }

        [ChildActionOnly]
        [Authorize(Roles="Family")]
        public ActionResult Entries()
        {
            var model = context.GetEntries();
            return View(model);
        }
    }
}