using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EnglishStudySystem.Controllers
{
    public class CustomerController : Controller
    {
        // GET: Customer
        public ActionResult CustomerDashBoard()
        {
            return View();
        }
    }
}