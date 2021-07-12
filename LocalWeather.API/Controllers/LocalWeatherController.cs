using LocalWeather.API.Helpers;
using LocalWeather.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LocalWeather.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LocalWeatherController : ControllerBase
    {
        private readonly ILogger<LocalWeatherController> _logger;

        public LocalWeatherController(ILogger<LocalWeatherController> logger)
        {
            _logger = logger;
        }

        // GET: api/<LocalWeatherController>
        [HttpGet]
        public IActionResult Get()
        {
            LocalWeatherSvc service = new LocalWeatherSvc();
            WeatherModel retval = service.GetWeather();

            if (retval != null)
            {
                return Ok(retval);
            }
            else
            {
                return NoContent();
            }

        }


    }
}
