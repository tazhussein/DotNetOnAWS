namespace OldWebApp.Models
{
    public class WeatherModel
    {
        public string Weather { get; set; }
        public string WeatherMessage { get; set; }

        public WeatherModel() { }

        public WeatherModel(string weather, string weatherMessage)
        {
            this.Weather = weather;
            this.WeatherMessage = weatherMessage;
        }
    }
}
