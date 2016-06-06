using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using MvcControlsToolkit.Core.Types;
using MvcControlsToolkit.Core.ITests.Options;
using MvcControlsToolkit.Core.ITests.ViewModels.Home;

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

        [HttpGet]
        public IActionResult GlobalizationTest()
        {
            return View(new GlobalizationTestVieModel()
            {
                ADatetime = new DateTime(2016, 4, 10),
                ADate = new DateTime(2016, 4, 10),
                AFloat = 12f,
                ATime = new TimeSpan(12, 10, 0),
                AWeek = new Week(2016, 25),
                AMonth = new Month(2016, 4)
            });
        }

        [HttpPost]
        public IActionResult GlobalizationTest(GlobalizationTestVieModel model)
        {
            if (ModelState.IsValid)
            {
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult DynamicRangeTest()
        {
            return View(new DynamicRangeTestViewModel()
            {
                ADatetime = new DateTime(2016, 4, 10),
                ADate = new DateTime(2016, 4, 10),
                AFloat = 1.5f,
                ATime = new TimeSpan(12, 10, 0),
                AWeek = new Week(2016, 25),
                AMonth = new Month(2016, 4)
            });
        }
        [HttpPost]
        public IActionResult DynamicRangeTest(DynamicRangeTestViewModel model)
        {
            if (ModelState.IsValid)
            {
            }
            return View(model);
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
