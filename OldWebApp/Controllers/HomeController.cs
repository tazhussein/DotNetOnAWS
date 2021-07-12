using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OldWebApp.Factories;
using OldWebApp.Models;
using System.Diagnostics;

namespace OldWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public IActionResult GetGreeting(string UserName)
        {
            IGreetingsFactory factory = new GreetingsFactoryV2();
            GreetingsModel data = factory.GetGreeting(UserName);

            ViewData["WelcomeMessage"] = data.WelcomeMessage;
            ViewData["WeatherMessage"] = data.LocalWeather.WeatherMessage;
            ViewData["WeatherStatus"] = data.LocalWeather.Weather;

            return View();
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
