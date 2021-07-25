using Amazon.CDK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GettingStartedCdk
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            var app = new App();
            new LocalWeatherApiCdkStack(app, "LocalWeatherApiCdkStack");
            new OldWebAppCdkStack(app, "OldWebAppCdkStack");

            app.Synth();
        }
    }
}
