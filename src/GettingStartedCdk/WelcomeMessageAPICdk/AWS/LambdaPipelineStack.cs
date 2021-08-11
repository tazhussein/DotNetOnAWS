using Amazon.CDK;
using Amazon.CDK.AWS.CodeBuild;
using Amazon.CDK.AWS.CodeCommit;
using Amazon.CDK.AWS.CodePipeline;
using Amazon.CDK.AWS.CodePipeline.Actions;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.S3;
using GettingStartedCdk.Helpers;
using GettingStartedCdk.Helpers.Constants;
using System.Collections.Generic;

namespace GettingStartedCdk.WelcomeMessageAPICdk.AWS
{
    public class LambdaPipelineStack : Stack
    {
        internal LambdaPipelineStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            WelcomeMessageAPIConstants constants = StackConstantsHelper.GetLambdaConstants();

            IRepository codeRepository = Repository.FromRepositoryName(this, constants.CodeCommitRepoName + "-repo", constants.CodeCommitRepoName);
            Bucket bucket = new Bucket(this, "welcomemessageapibucket");

            Artifact_ sourceOutput = new Artifact_("Source");
            Artifact_ buildOutput = new Artifact_("Build");

            Role pipelineRole = new Role(this, "PipelineRole", new RoleProps
            {
                AssumedBy = new CompositePrincipal(new PrincipalBase[] 
                { 
                    new ServicePrincipal("codepipeline.amazonaws.com"),
                    new ServicePrincipal("codebuild.amazonaws.com")
                })
            });

            pipelineRole.AddManagedPolicy(ManagedPolicy.FromAwsManagedPolicyName("PowerUserAccess"));
            bucket.GrantWrite(pipelineRole);

            PipelineProject buildProject = new PipelineProject(this, "BuildProject", new PipelineProjectProps
            {
                ProjectName = constants.StackPrefix + "-Build",
                Environment = new BuildEnvironment
                {
                    BuildImage = LinuxBuildImage.STANDARD_5_0,
                    Privileged = true
                },
                BuildSpec = BuildSpec.FromObject(new Dictionary<string, object> 
                {
                    ["version"] = "0.2",
                    ["phases"] = new Dictionary<string, object>
                    {
                        ["install"] = new Dictionary<string, object>
                        {
                            ["commands"] = "dotnet tool install -g Amazon.Lambda.Tools"
                        },
                        ["build"] = new Dictionary<string, string>
                        {
                            ["commands"] = "dotnet tool install -g Amazon.Lambda.Tools",
                            ["commands"] = "dotnet lambda package-ci --template ./src/WelcomeMessage.API/serverless.template --output-template packaged-template.yaml --s3-bucket " + bucket.BucketName + " --s3-prefix " + constants.S3BuildOutputPath
                        }
                    },
                    ["artifacts"] = new Dictionary<string, string>
                    {
                        ["files"] = "packaged-template.yaml"
                    }
                }),
                Role = pipelineRole,
                Description = "Build project for the welcomemessage api application",
            });

            //Get source code from CodeCommit and place it into an S3 bucket
            CodeCommitSourceAction sourceAction = new CodeCommitSourceAction(new CodeCommitSourceActionProps
            {
                ActionName = "Source",
                Branch = constants.CodeCommitBranch,
                Repository = codeRepository,
                Output = sourceOutput,
                Role = pipelineRole
            });

            CodeBuildAction buildAction = new CodeBuildAction(new CodeBuildActionProps
            {
                ActionName = "Build",
                Project = buildProject,
                Input = sourceOutput,
                Outputs = new Artifact_[] { buildOutput },
                Role = pipelineRole
            });

            CloudFormationCreateReplaceChangeSetAction deployActioncreateChangeSet = new CloudFormationCreateReplaceChangeSetAction(new CloudFormationCreateReplaceChangeSetActionProps
            {
                ActionName = "CreateChangeSet",
                StackName = constants.StackName,
                ChangeSetName = constants.ChangeSetName,
                AdminPermissions = true,
                TemplatePath = buildOutput.AtPath("packaged-template.yaml"),
                RunOrder = 1
            });

            CloudFormationExecuteChangeSetAction deployActionExecuteChangeSet = new CloudFormationExecuteChangeSetAction(new CloudFormationExecuteChangeSetActionProps
            { 
                ActionName = "ExecuteChangeSet",
                StackName = constants.StackName,
                ChangeSetName = constants.ChangeSetName,
                RunOrder = 2
            });

            Pipeline pipeline = new Pipeline(this, "Pipeline", new PipelineProps
            {
                PipelineName = constants.StackName + "-Pipeline",
                Role = pipelineRole,
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
                            deployActioncreateChangeSet,
                            deployActionExecuteChangeSet
                        }
                    }
                }
            });
        }
    }
}
