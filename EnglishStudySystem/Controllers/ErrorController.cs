using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EnglishStudySystem.Controllers
{
    public class ErrorController : Controller
    {
        // GET: Error
        public ActionResult AccessDenied(string message)
        {
            ViewBag.ErrorMessage = message;
            return View();
        }
    }
}