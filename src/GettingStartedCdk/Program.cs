using Amazon.CDK;
using GettingStartedCdk.LocalWeatherAPICdk.AWS;
using GettingStartedCdk.OldWebAppCdk.AWS;
using GettingStartedCdk.WelcomeMessageAPICdk.AWS;

namespace GettingStartedCdk
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            var app = new App();

            new ECSECRPipelineStack(app, "ECSECRPipelineStack");
            new OldWebAppPipelineStack(app, "BealstalkPipelineStack");
            new LambdaPipelineStack(app, "LambdaPipelineStack");

            app.Synth();
        }
    }
}
