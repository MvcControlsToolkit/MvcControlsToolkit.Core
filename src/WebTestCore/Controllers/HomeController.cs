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
using MvcControlsToolkit.Core.DataAnnotations;

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
            var options = new QueryAttribute().AllowedForProperty(typeof(string));
            var select = QueryAttribute.QueryOptionsToEnum(options);
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
                AMonth = new Month(2016, 4),
                ADatetimeOffset=new DateTimeOffset(2016, 4, 1, 0, 0, 0, new TimeSpan(4, 0, 0))
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
            return View(new Employee()
            {
                Name="John",
                Surname = "Smith",
                Matr = "aaaaa"
            });
        }

        [HttpPost]
        public IActionResult StoreTest(Person model)
        {
            if (ModelState.IsValid)
            {
            }
            return View(model);
        }
        [HttpGet]
        public IActionResult StoreTypesTest()
        {
            return View(new SerializationViewModel()
            {
                MonthTest = new Month(2000, 4),
                WeekTest = new Week(2000, 10),
                SimpleTypeTest = new DateTimeOffset(2017, 1, 30, 18, 46, 10, new TimeSpan(4, 0, 0))
            });
            
        }

        [HttpPost]
        public IActionResult StoreTypesTest(SerializationViewModel model)
        {
            if (ModelState.IsValid)
            {
            }
            return View(model);
        }
        [HttpGet]
        public IActionResult SubClassTest()
        {
            return View(new SubClassViewModel()
            {
                Test=new Person
                {
                    Name="Francesco",
                    Surname="Abbruzzese",
                    
                },
                Test1 = new Customer
                {
                    Name = "John",
                    Surname = "Smith",
                    RegisterNumber = "12345678"
                },
                Test2 = new Employee
                {
                    Name = "Peter",
                    Surname = "Black",
                    Matr = "pg34"
                }
            });
        }
        [HttpPost]
        public IActionResult SubClassTest(SubClassViewModel model)
        {
            if (ModelState.IsValid)
            {
            }
            return View(model);
        }
        public IActionResult InterfaceTest()
        {
            return View();
        }
        [HttpPost]
        public IActionResult InterfaceTest(ITestInterface model)
        {
            return View(model);
        }
        public IActionResult RowCollection()
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
