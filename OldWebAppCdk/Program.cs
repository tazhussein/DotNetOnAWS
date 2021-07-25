using Amazon.CDK;

namespace OldWebAppCdk
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            var app = new App();
            new OldWebAppCdkStack(app, "OldWebAppCdkStack");

            app.Synth();
        }
    }
}
