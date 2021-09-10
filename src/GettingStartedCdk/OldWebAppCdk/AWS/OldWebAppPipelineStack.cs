using Amazon.CDK;
using Amazon.CDK.AWS.CodeBuild;
using Amazon.CDK.AWS.CodeCommit;
using Amazon.CDK.AWS.CodePipeline;
using Amazon.CDK.AWS.CodePipeline.Actions;
using Amazon.CDK.AWS.ElasticBeanstalk;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.S3;
using GettingStartedCdk.Helpers;
using GettingStartedCdk.Helpers.Constants;
using System.Collections.Generic;

namespace GettingStartedCdk.OldWebAppCdk.AWS
{
    public class OldWebAppPipelineStack : Stack
    {
        public OldWebAppPipelineStack(Construct parent, string id, IStackProps props = null) : base(parent, id, props)
        {
            OldWebAppConstants constants = StackConstantsHelper.GetBeanstalkConstants();

            Role pipelineRole = new Role(this, "PipelineRole", new RoleProps
            {
                AssumedBy = new ServicePrincipal("codepipeline.amazonaws.com")
            });

            pipelineRole.AddManagedPolicy(ManagedPolicy.FromAwsManagedPolicyName("PowerUserAccess"));

            IRole ebInstanceRole = new Role(this, "Beanstalk-Ec2-Role", new RoleProps
            { 
                AssumedBy = new ServicePrincipal("ec2.amazonaws.com")
            });

            ebInstanceRole.AddManagedPolicy(ManagedPolicy.FromAwsManagedPolicyName("AWSElasticBeanstalkWebTier"));
            ebInstanceRole.AddManagedPolicy(ManagedPolicy.FromAwsManagedPolicyName("AmazonSSMFullAccess"));

            CfnInstanceProfile instanceProfile = new CfnInstanceProfile(this, constants.StackPrefix + "-Instance-Profile", new CfnInstanceProfileProps
            {
                InstanceProfileName = constants.StackPrefix + "-Instance-Profile",
                Roles = new string[] { ebInstanceRole .RoleName }
            });

            //EBS Environment Options
            CfnEnvironment.OptionSettingProperty[] optionSettingProperties = new CfnEnvironment.OptionSettingProperty[] {
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
                    Value = constants.ProfileName
                },
                new CfnEnvironment.OptionSettingProperty()
                {
                    Namespace = "aws:elasticbeanstalk:application:environment",
                    OptionName = "ASPNETCORE_ENVIRONMENT",
                    Value = "Development "
                },
                new CfnEnvironment.OptionSettingProperty()
                {
                    Namespace =  "aws:elasticbeanstalk:environment:proxy:staticfiles",
                    OptionName = "/lib/bootstrap/dist/css",
                    Value= "/"
                },
                new CfnEnvironment.OptionSettingProperty()
                {
                    Namespace= "aws:elasticbeanstalk:environment:proxy:staticfiles",
                    OptionName= "/css",
                    Value= "/"
                },
                new CfnEnvironment.OptionSettingProperty()
                {
                    Namespace= "aws:elasticbeanstalk:environment:proxy:staticfiles",
                    OptionName= "/lib/jquery/dist",
                    Value= "/"
                },
                new CfnEnvironment.OptionSettingProperty()
                {
                    Namespace= "aws:elasticbeanstalk:environment:proxy:staticfiles",
                    OptionName= "/lib/bootstrap/dist/js",
                    Value= "/"
                },
                new CfnEnvironment.OptionSettingProperty()
                {
                    Namespace= "aws:elasticbeanstalk:environment:proxy:staticfiles",
                    OptionName= "/js",
                    Value= "/"
                },
                new CfnEnvironment.OptionSettingProperty()
                {
                    Namespace= "aws:elasticbeanstalk:environment:proxy:staticfiles",
                    OptionName= "/Images",
                    Value= "/"
                }
            };

            //EBS Application and Environment
            CfnApplication app = new CfnApplication(this, "Application", new CfnApplicationProps
            {
                ApplicationName = constants.ApplicationName
            });

            //Note: run this AWS CLI command if you don't know your SolutionStackName: aws elasticbeanstalk list-available-solution-stacks
            CfnEnvironment environment = new CfnEnvironment(this, "Environment", new CfnEnvironmentProps
            {
                EnvironmentName = constants.EnvironmentName,
                ApplicationName = app.ApplicationName,
                SolutionStackName = "64bit Amazon Linux 2 v2.2.5 running .NET Core",
                OptionSettings = optionSettingProperties
            });

            environment.AddDependsOn(app);

            //CodeBuild
            IRepository codeRepository = Repository.FromRepositoryName(this, constants.CodeCommitRepoName + "-repo", constants.CodeCommitRepoName);

            Artifact_ cloudAssemblyArtifact = new Artifact_("cloudAssemblyArtifact");
            Artifact_ buildActionOutputArtifact = new Artifact_("buildActionOutputArtifact");

            Bucket bucket = new Bucket(this, "oldwebappsbucket");

            Project project = new Project(this, "CodeBuild", new ProjectProps
            {
                ProjectName = constants.StackPrefix + "-Build",
                Source = Source.CodeCommit(new CodeCommitSourceProps { Repository = codeRepository }),
                Environment = new BuildEnvironment { BuildImage = LinuxBuildImage.STANDARD_5_0 },
                BuildSpec = BuildSpec.FromSourceFilename(constants.BuildSpecFileName),
                Artifacts = Artifacts.S3(new S3ArtifactsProps
                {
                    Bucket = bucket,
                    Path = "OldWebAppOutputPath"
                })
            });

            //CodePipeline step to get application code form our CodeCommit repo
            CodeCommitSourceAction sourceAction = new CodeCommitSourceAction(new CodeCommitSourceActionProps
            {
                ActionName = "Source",
                Branch = constants.CodeCommitBranch,
                Repository = codeRepository,
                Output = cloudAssemblyArtifact,
                Role = pipelineRole
            });

            //CodePipeline build step to compile our application and publish compiled artifacts to be deployed
            CodeBuildAction buildAction = new CodeBuildAction(new CodeBuildActionProps
            {
                ActionName = "Build",
                Project = project,
                Input = cloudAssemblyArtifact,
                Outputs = new Artifact_[] { buildActionOutputArtifact },
                Role = pipelineRole
            });

            //CodePipeline deploy step to push our compiled application artifacts onto Elastic Beanstalk.
            //Custom deploy action to push onto Beanstalk via CDK
            ElasticBeanstalkDeployAction deployAction = new ElasticBeanstalkDeployAction(new ElasticBeanstalkDeployActionProps
            {
                id = constants.StackPrefix + "-Env",
                ebsEnvironmentName = constants.StackPrefix + "-Env",
                ebsApplicationName = constants.StackPrefix + "-App",
                input = buildActionOutputArtifact,
                role = pipelineRole
            });

            //Create the CodePipeline to compile, publish and deploy our .NET Core application onto existing Elastic Beanstalk infrastructure
            Pipeline pipeline = new Pipeline(this, constants.StackPrefix + "-AppCode-Pipeline", new PipelineProps
            {
                PipelineName = constants.StackPrefix + "-Pipeline",
                Role = pipelineRole,
                Stages = new Amazon.CDK.AWS.CodePipeline.StageProps[]
                {
                    new Amazon.CDK.AWS.CodePipeline.StageProps
                    {
                        StageName = "Source",
                        Actions = new Amazon.CDK.AWS.CodePipeline.Actions.Action[]
                        {
                            sourceAction
                        }
                    },
                    new Amazon.CDK.AWS.CodePipeline.StageProps
                    {
                        StageName = "Build",
                        Actions = new Amazon.CDK.AWS.CodePipeline.Actions.Action[]
                        {
                            buildAction
                        }
                    },
                     new Amazon.CDK.AWS.CodePipeline.StageProps
                    {
                        StageName = "Deploy",
                        Actions = new Amazon.CDK.AWS.CodePipeline.Action[]
                        {
                            deployAction
                        }
                    },
                }
            });

        }
    }

    public class ElasticBeanstalkDeployActionProps
    {
        public string id { get; set; }
        public string ebsApplicationName { get; set; }
        public string ebsEnvironmentName { get; set; }
        public Artifact_ input { get; set; }
        public Role role { get; set; }

    }

    public class ElasticBeanstalkDeployAction : Amazon.CDK.AWS.CodePipeline.Action
    {
        private ActionProperties actionProperties { get; set; }
        private ElasticBeanstalkDeployActionProps props { get; set; }

        public ElasticBeanstalkDeployAction(ElasticBeanstalkDeployActionProps props)
        {
            this.actionProperties = new ActionProperties();

            this.actionProperties.Category = ActionCategory.DEPLOY;
            this.actionProperties.ActionName = "Deploy";
            this.actionProperties.Owner = "AWS";
            this.actionProperties.Provider = "ElasticBeanstalk";
            this.actionProperties.Inputs = new Artifact_[] { props.input };

            this.actionProperties.ArtifactBounds = new ActionArtifactBounds
            {
                MinInputs = 1,
                MaxInputs = 1,
                MinOutputs = 0,
                MaxOutputs = 0
            };

            this.props = props;
            this.actionProperties.Role = props.role;
        }

        protected override IActionProperties ProvidedActionProperties => actionProperties;

        protected override IActionConfig Bound(Construct scope, IStage stage, IActionBindOptions options)
        {
            options.Bucket.GrantRead(options.Role);

            IActionConfig retval = new ActionConfig
            {
                Configuration = new Dictionary<string, string>
                {
                    ["ApplicationName"] = this.props.ebsApplicationName,
                    ["EnvironmentName"] = this.props.ebsEnvironmentName
                }
            };

            return retval;
        }
    }

}
