using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebTestCore.Options;
using MvcControlsToolkit.Core.Types;
using WebTestCore.ViewModels;
using WebTestCore.Data;
using System.Security.Claims;
using WebTestCore.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace WebTestCore.Controllers
{
    public class HomeController : Controller
    {
        private WelcomeMessage welcome = null;
        public HomeController(WelcomeMessage welcome, IHttpContextAccessor httpContextAccessor)
        {
            this.welcome = welcome;
            if (httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                var id = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            }
        }
        public IActionResult Index()
        {
            ViewData["Welcome"] = welcome.Message + (welcome.AddDate ? ", " + DateTime.Today.ToString("D") : "");
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
        [HttpGet]
        public IActionResult StoreTest()
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
        public IActionResult StoreTest(GlobalizationTestVieModel model)
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
