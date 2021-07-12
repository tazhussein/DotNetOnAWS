using OldWebApp.Models;
using System;

namespace OldWebApp.Services
{
    public class LocalWeatherSvc
    {
        private const string _weatherMessageTemplate = "The weather in your region is {0}";

        private enum Weather
        {
            Sunny,
            Overcast,
            Raining,
            Thundering
        }

        public WeatherModel GetWeather()
        {
            WeatherModel retval;
            string weather = GetRandomWeatherValue();
            string weatherMessage = string.Format(_weatherMessageTemplate, weather);

            retval = new WeatherModel(weather, weatherMessage);

            return retval;
        }

        private string GetRandomWeatherValue()
        {
            string retval = string.Empty;

            Array weatherValues = Enum.GetValues(typeof(Weather));
            Random random = new Random();
            retval = ((Weather)weatherValues.GetValue(random.Next(weatherValues.Length))).ToString();

            return retval;
        }
    }
}
