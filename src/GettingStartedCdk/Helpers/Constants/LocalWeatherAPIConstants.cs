
namespace GettingStartedCdk.Helpers.Constants
{
    public class LocalWeatherAPIConstants : StackConstants
    {
        public string ECRRegistryName { get; init; }
        public string ECSClusterName { get; init; }
        public string ECSServiceName { get; init; }
        public string EmptyImageUrl { get; init; }
        public string APIResourcePath { get; init; }
        public string ECRImageTag { get; init; }
    }
}
