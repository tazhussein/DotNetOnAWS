namespace OldWebApp.Models
{
    public class GreetingsModel
    {
        public string WelcomeMessage { get; set; }
        public WeatherModel LocalWeather { get; set; }

        public GreetingsModel(string welcomeMessage, WeatherModel localWeather)
        {
            this.WelcomeMessage = welcomeMessage;
            this.LocalWeather = localWeather;
        }
    }
}
