using OldWebApp.Models;

namespace OldWebApp.Factories
{
    public interface IGreetingsFactory
    {
        GreetingsModel GetGreeting(string userName);
    }
}
