using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using MvcControlsToolkit.Core.ITests.Options;

namespace MvcControlsToolkit.Core.ITests.Controllers
{
    public class HomeController : Controller
    {
        private WelcomeMessage welcome = null;
        public HomeController(WelcomeMessage welcome)
        {
            this.welcome = welcome;
        }
        public IActionResult Index()
        {

            ViewData["Welcome"] = welcome.Message + (welcome.AddDate ? ", "+DateTime.Today.ToString("D") : "");
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
