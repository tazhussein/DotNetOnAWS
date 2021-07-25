using Amazon.CDK;
using Amazon.CDK.AWS.ElasticBeanstalk;
using Amazon.CDK.AWS.IAM;

namespace OldWebAppCdk
{
    public class OldWebAppCdkStack : Stack
    {
        internal OldWebAppCdkStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            string prefix = "OldWebApp";
            string appName = prefix + "-App";
            string environmentName = prefix + "-Env";
            string profileName = prefix + "-Instance-Profile";

            var ebsInstanceRole = new Role(this, "Beanstalk-Ec2-Role", new RoleProps
            {
                AssumedBy = new ServicePrincipal("ec2.amazonaws.com")
            });

            var instanceProfile = new CfnInstanceProfile(this, profileName, new CfnInstanceProfileProps
            {
                InstanceProfileName = profileName,
                Roles = new string[] { ebsInstanceRole.RoleName }
            });

            var optionSettingProperties = new CfnEnvironment.OptionSettingProperty[] {
                new CfnEnvironment.OptionSettingProperty()
                {
                    Namespace = "aws:autoscaling:launchconfiguration",
                    OptionName = "InstanceType",
                    Value = "t3.small"
                },
                new CfnEnvironment.OptionSettingProperty()
                {
                    Namespace = "aws:autoscaling:launchconfiguration",
                    OptionName = "IamInstanceProfile",
                    Value = profileName
                },
                new CfnEnvironment.OptionSettingProperty()
                {
                    Namespace = "aws:elasticbeanstalk:application:environment",
                    OptionName = "ASPNETCORE_ENVIRONMENT",
                    Value = ""
                }
            };


            var app = new CfnApplication(this, "Application", new CfnApplicationProps
            {
                ApplicationName = appName
            });

            //Note: run this AWS CLI command if you don't know your SolutionStackName: aws elasticbeanstalk list-available-solution-stacks
            var environment = new CfnEnvironment(this, "Environment", new CfnEnvironmentProps
            {
                EnvironmentName = environmentName,
                ApplicationName = app.ApplicationName,
                SolutionStackName = "64bit Amazon Linux 2 v2.2.3 running .NET Core",
                OptionSettings = optionSettingProperties
            });
        }
    }
}
