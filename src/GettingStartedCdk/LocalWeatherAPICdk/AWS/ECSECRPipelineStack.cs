using Amazon.CDK;
using Amazon.CDK.AWS.CodeBuild;
using Amazon.CDK.AWS.CodePipeline;
using Amazon.CDK.AWS.CodePipeline.Actions;
using Amazon.CDK.AWS.CodeCommit;

using System.Collections.Generic;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.ECS.Patterns;
using Amazon.CDK.AWS.ApplicationAutoScaling;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.SSM;
using GettingStartedCdk.Helpers;
using GettingStartedCdk.Helpers.Constants;

namespace GettingStartedCdk.LocalWeatherAPICdk.AWS
{
    public class ECSECRPipelineStack : Stack
    {
        internal ECSECRPipelineStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            LocalWeatherAPIConstants constants = StackConstantsHelper.GetECSConstants();

            #region Create CodeCommit and ECR repositories

            IRepository codeRepository = Repository.FromRepositoryName(this, constants.CodeCommitRepoName + "-repo", constants.CodeCommitRepoName);

            Amazon.CDK.AWS.ECR.Repository containerRegistry = new Amazon.CDK.AWS.ECR.Repository(this, constants.ECRRegistryName + "-registry", new Amazon.CDK.AWS.ECR.RepositoryProps
            {
                RepositoryName = constants.ECRRegistryName,
                RemovalPolicy = RemovalPolicy.DESTROY
            });

            #endregion

            #region Create ECS Fargate Cluster, Service and Tasks

            //Using Amazon.CDK.AWS.ECS.Patterns we can get CDK to create all components of a new VPC automatically without the need to define it ourselves.
            //You can also use an existing VPC if needed and pass it into the cluster construct below 

            Cluster cluster = new Cluster(this, constants.ECSClusterName, new ClusterProps
            {
                ClusterName = constants.ECSClusterName
            });

            // Create AWS Fargate service
            ApplicationLoadBalancedFargateService fargateService = new ApplicationLoadBalancedFargateService(this, constants.ECSServiceName, new ApplicationLoadBalancedFargateServiceProps
            {
                ServiceName = constants.ECSServiceName,
                Cluster = cluster,
                TaskImageOptions = new ApplicationLoadBalancedTaskImageOptions
                {
                    Image = ContainerImage.FromRegistry(constants.EmptyImageUrl), //use generic image to build the ECS environment. The pipeline will deploy the weather api image onto this service
                    EnableLogging = true
                },
                PublicLoadBalancer = true,
                DesiredCount = 1
            });

            // Allow ECS task to access ECR
            fargateService.TaskDefinition.AddToExecutionRolePolicy(new PolicyStatement(new PolicyStatementProps
            {
                Effect = Effect.ALLOW,
                Resources = new string[] { "*" },
                Actions = new string[]
                {
                    "ecr:GetAuthorizationToken",
                    "ecr:BatchCheckLayerAvailability",
                    "ecr:GetDownloadUrlForLayer",
                    "ecr:BatchGetImage",
                    "logs:CreateLogStream",
                    "logs:PutLogEvents"
                }
            }));

            //Add API controller name to ALB healthcheck
            fargateService.TargetGroup.ConfigureHealthCheck(new Amazon.CDK.AWS.ElasticLoadBalancingV2.HealthCheck
            {
                Path = constants.APIResourcePath
            });

            // Setup AutoScaling policy
            ScalableTaskCount scaling = fargateService.Service.AutoScaleTaskCount(new EnableScalingProps { MaxCapacity = 6 });

            scaling.ScaleOnCpuUtilization("CpuScaling", new CpuUtilizationScalingProps
            {
                TargetUtilizationPercent = 50,
                ScaleInCooldown = Duration.Seconds(60),
                ScaleOutCooldown = Duration.Seconds(60)
            });

            #endregion

            #region Create pipeline build project

            //Define a build project for the pipeline which will read the buildspec-localweatherapi.yml file, compile our localweather api .NET Core application and load it on to the image
            //The image will then be uploaded into our repository.
            //The build spec file will refer to the Dockerfile commands to build our .NET Core project
            PipelineProject buildProject = new PipelineProject(this, "BuildProject", new PipelineProjectProps
            {
                BuildSpec = BuildSpec.FromSourceFilename(constants.BuildSpecFileName),
                Description = "Build project for the localweatherapi application",
                Environment = new BuildEnvironment
                {
                    BuildImage = LinuxBuildImage.STANDARD_5_0,
                    Privileged = true
                },
                EnvironmentVariables = new Dictionary<string, IBuildEnvironmentVariable>()
                {
                    { "AWS_DEFAULT_REGION", new BuildEnvironmentVariable { Type = BuildEnvironmentVariableType.PLAINTEXT, Value = this.Region} },
                    { "AWS_ACCOUNT_ID", new BuildEnvironmentVariable { Type = BuildEnvironmentVariableType.PLAINTEXT, Value = this.Account} },
                    { "IMAGE_TAG", new BuildEnvironmentVariable { Type = BuildEnvironmentVariableType.PLAINTEXT, Value = constants.ECRImageTag} },
                    { "IMAGE_REPO_NAME", new BuildEnvironmentVariable { Type = BuildEnvironmentVariableType.PLAINTEXT, Value = containerRegistry.RepositoryName} }
                }
            });

            containerRegistry.GrantPullPush(buildProject);

            #endregion

            #region Create CodePipeline actions
            
            //CI CD pipeline actions which will be used to orchestrate build and dpeloy steps for our application
            
            
            Role pipelineRole = new Role(this, "PipelineRole", new RoleProps
            {
                AssumedBy = new CompositePrincipal
                (
                    new ServicePrincipal("codepipeline.amazonaws.com"),
                    new ServicePrincipal("codebuild.amazonaws.com"),
                    new AccountRootPrincipal()
                )
            });

            Artifact_ sourceOutput = new Artifact_("Source");
            Artifact_ buildOutput = new Artifact_("Build");

            //Get source code from CodeCommit and place it into an S3 bucket
            CodeCommitSourceAction sourceAction = new CodeCommitSourceAction(new CodeCommitSourceActionProps
            {
                ActionName = "Source",
                Branch = constants.CodeCommitBranch,
                Repository = codeRepository,
                Output = sourceOutput,
                Role = pipelineRole
            });

            //Get code from S3, build project and place outputs into S3. The buildspec file will also upload an image into ECR
            CodeBuildAction buildAction = new CodeBuildAction(new CodeBuildActionProps
            {
                ActionName = "Build",
                Project = buildProject,
                Input = sourceOutput,
                Outputs = new Artifact_[] { buildOutput },
                Role = pipelineRole
            });

            //Pickup imagedefinitions from S3 referring to the ECR registry for our container image, deploy image onto ECS
            EcsDeployAction deployAction = new EcsDeployAction(new EcsDeployActionProps
            {
                ActionName = "Deploy",
                Service = fargateService.Service,
                ImageFile = new ArtifactPath_(buildOutput, "imagedefinitions.json"),
                Role = pipelineRole
            });

            #endregion

            #region Create CI/CD pipeline

            Pipeline pipeline = new Pipeline(this, "Pipeline", new PipelineProps
            {
                PipelineName = constants.StackPrefix + "-Pipeline",
                Stages = new Amazon.CDK.AWS.CodePipeline.StageProps[]
                {
                    new  Amazon.CDK.AWS.CodePipeline.StageProps
                    {
                        StageName = "Source",
                        Actions = new IAction[]
                        {
                            sourceAction
                        }
                    },
                    new Amazon.CDK.AWS.CodePipeline.StageProps
                    {
                        StageName = "Build",
                        Actions = new IAction[]
                        {
                            buildAction
                        }
                    },
                    new Amazon.CDK.AWS.CodePipeline.StageProps
                    {
                        StageName = "Deploy",
                        Actions = new IAction[]
                        {
                            deployAction
                        }
                    }
                }
            });

            #endregion

            #region Add api url to SSM Parameterstore

            //The OldWebApp MVC application will need to invoke our LocalWeather API application deployed onto ECS. Add the API path to Parameter store as a lookup for the OldWebApp
            StringParameter apiUrlParam = new StringParameter(this, constants.SSMParameterName, new StringParameterProps
            {
                StringValue = "http://" + fargateService.LoadBalancer.LoadBalancerDnsName + "/",
                Description = "URL for the LocalWeather API. Used by the OldWebApp MVC project to be hosted on BeanStalk",
                ParameterName = constants.SSMParameterName
            });

            #endregion

        }
    }
}
