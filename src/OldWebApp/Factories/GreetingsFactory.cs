using OldWebApp.Models;
using OldWebApp.Services;

namespace OldWebApp.Factories
{
    public class GreetingsFactory : IGreetingsFactory
    {

        public GreetingsModel GetGreeting(string userName)
        {
            GreetingsModel retval;

            WelcomeMessageSvc welcome = new WelcomeMessageSvc();
            LocalWeatherSvc weather = new LocalWeatherSvc();

            retval = new GreetingsModel(welcome.GetWelcomeMessage(userName), weather.GetWeather());

            return retval;
        }

    }
}
