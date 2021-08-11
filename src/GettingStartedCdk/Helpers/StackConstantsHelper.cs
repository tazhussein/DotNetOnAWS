using GettingStartedCdk.Helpers.Constants;
using System;
using System.Collections.Generic;
using System.Text;

namespace GettingStartedCdk.Helpers
{
    public class StackConstantsHelper
    {
        public static WelcomeMessageAPIConstants GetLambdaConstants()
        {
            WelcomeMessageAPIConstants retval = new WelcomeMessageAPIConstants()
            {
                StackPrefix = "WelcomeMessageApi",
                CodeCommitBranch = "master",
                CodeCommitRepoName = "cdktest",
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
                CodeCommitBranch = "master",
                CodeCommitRepoName = "cdktest",
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
                CodeCommitRepoName = "cdktest",
                CodeCommitBranch = "master",
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
