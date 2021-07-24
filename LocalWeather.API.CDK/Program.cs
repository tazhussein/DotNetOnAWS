using Amazon.CDK;

namespace LocalWeatherApiCdk
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            var app = new App();
            new LocalWeatherApiCdkStack(app, "LocalWeatherApiCdkStack");

            app.Synth();
        }

    }
}
