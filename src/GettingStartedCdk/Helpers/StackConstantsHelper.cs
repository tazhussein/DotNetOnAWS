using GettingStartedCdk.Helpers.Constants;

namespace GettingStartedCdk.Helpers
{
    public class StackConstantsHelper
    {
        private static string codeCommitRepoName = "DotNetOnAWS";
        private static string codeCommitBranch = "master";

        public static WelcomeMessageAPIConstants GetLambdaConstants()
        {

            WelcomeMessageAPIConstants retval = new WelcomeMessageAPIConstants()
            {
                StackPrefix = "WelcomeMessageApi",
                CodeCommitBranch = codeCommitBranch,
                CodeCommitRepoName = codeCommitRepoName,
                S3BuildOutputPath = "sam-welcomemessageapi-output/",
                StackName = "WelcomeMessageApi-Sam",
                ChangeSetName = "WelcomeMessageApi-ChangeSet"
            };

            return retval;
        }

        public static OldWebAppConstants GetBeanstalkConstants()
        {
            OldWebAppConstants retval = new OldWebAppConstants
            {
                StackPrefix = "OldWebApp",
                BuildSpecFileName = "buildspec-oldwebapp.yml",
                CodeCommitBranch = codeCommitBranch,
                CodeCommitRepoName = codeCommitRepoName,
                ApplicationName = "OldWebApp-App",
                EnvironmentName = "OldWebApp-Env",
                ProfileName = "OldWebApp-Instance-Profile"
            };

            return retval;
        }

        public static LocalWeatherAPIConstants GetECSConstants()
        {
            LocalWeatherAPIConstants retval = new LocalWeatherAPIConstants
            {
                StackPrefix = "LocalWeatherApi",
                CodeCommitRepoName = codeCommitRepoName,
                CodeCommitBranch = codeCommitBranch,
                ECRRegistryName = "localweather",
                ECSClusterName = "LocalWeatherApiCluster",
                ECSServiceName = "LocalWeatherApiService",
                EmptyImageUrl = "public.ecr.aws/lambda/dotnet:latest",
                APIResourcePath = "/localweather",
                BuildSpecFileName = "buildspec-localweatherapi.yml",
                ECRImageTag = "Latest",
                SSMParameterName = "/dotnetonaws/localweatherapi/url"
            };



            return retval;
        }
    }
}
