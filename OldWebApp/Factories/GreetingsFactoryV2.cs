using OldWebApp.Models;
using RestSharp;
using System;

namespace OldWebApp.Factories
{
    /// <summary>
    /// Uses deployed AWS APIs to get a welcome message and local weather
    /// </summary>
    public class GreetingsFactoryV2 : IGreetingsFactory
    {
        //welcome messages API deployed to Lambda
        private const string _welcomeMessageServiceURL = "<Lambda API URL>/api/"; //TODO: Use AWS Parameter store
        private const string _welcomeMessageResource = "welcomemessage";

        //local weather API deployed to ECS as a docker container
        private const string _localWeatherServiceURL = "<ECS Service API>/api/"; //TODO: Use AWS Parameter store
        private const string _localWeatherResource = "localWeather";

        public GreetingsModel GetGreeting(string userName)
        {
            GreetingsModel retval = null;

            string welcomeMessage = GetAPIData<string>(Method.POST, _welcomeMessageServiceURL, _welcomeMessageResource, userName);
            WeatherModel localWeather = GetAPIData<WeatherModel>(Method.GET, _localWeatherServiceURL, _localWeatherResource, null);
            GreetingsModel greetingsModel = new GreetingsModel(welcomeMessage, localWeather);

            retval = greetingsModel;

            return retval;
        }

        private T GetAPIData<T>(Method httpMethod, string apiURL, string apiResource, string jsonBody)
        {
            var client = new RestClient(apiURL);
            var request = new RestRequest(apiResource, httpMethod);
            IRestResponse<T> response;

            if (!String.IsNullOrEmpty(jsonBody))
            {
                request.AddJsonBody(jsonBody);
            }

            if (httpMethod == Method.POST)
            {
                response = client.Post<T>(request);
            }
            else
            {
                response = client.Get<T>(request);
            }


            return response.Data;
        }

    }
}
