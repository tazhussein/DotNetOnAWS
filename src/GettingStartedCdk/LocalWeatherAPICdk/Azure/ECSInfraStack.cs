using Amazon.CDK;
using Amazon.CDK.AWS.ECR;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.ECS.Patterns;
using Amazon.CDK.AWS.SSM;
using HealthCheck = Amazon.CDK.AWS.ElasticLoadBalancingV2.HealthCheck;

namespace GettingStartedCdk.LocalWeatherAPICdk.Azure
{
    public class ECSInfraStack : Stack
    {
        public ECSInfraStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            //Get reference for ECR created and image deployed by Azure DevOps Pipeline
            IRepository repo = Repository.FromRepositoryName(this, "localweather", "localweather");
            var image = ContainerImage.FromEcrRepository(repo, "latest");

            /********************************************************************************************************************************
            * Using Amazon.CDK.AWS.ECS.Patterns constructs to reduce lines of code needed to create ECS Cluster, Service, Tasks and ALB.
            * Unless an existing or pre-defined VPC is passed in this construct will create a new VPc and associated networking components.
            * If you don't want to use ECS Patterns here's a guide to using the underlying components independantly: https://docs.aws.amazon.com/cdk/latest/guide/ecs_example.html
            ********************************************************************************************************************************/
            var fargateECS = new ApplicationLoadBalancedFargateService(this, "localweathersvc",
                new ApplicationLoadBalancedFargateServiceProps
                {
                    TaskImageOptions = new ApplicationLoadBalancedTaskImageOptions
                    {
                        Image = image                        
                    },
                    PublicLoadBalancer = true, //loadbalancer will be provisioned
                    DesiredCount = 3, //I want 3 tasks running at any given time
                    ServiceName = "localweathersvc",
                    Cluster = new Cluster(this, "localweathercluster",
                        new ClusterProps { ClusterName = "localweathercluster" }),
                    LoadBalancerName = "localweatherelb"
                });

            //Add API controller name to ALB healthcheck
            fargateECS.TargetGroup.ConfigureHealthCheck(new HealthCheck()
            {
                Path = "/localweather"
            });

            /********************************************************************************************************************************
             * Create an SSM parameter to store the API URL. To be used by our MVC web page OldWebApp.
             * API URL is taken from the DNS of the newly provisioned ALB which is part of the ECS environment provisioned by this stack
             ********************************************************************************************************************************/
            var apiUrlParam = new StringParameter(this, "/dotnetonaws/localweatherapi/url", new StringParameterProps
            {
                StringValue = "http://" + fargateECS.LoadBalancer.LoadBalancerDnsName + "/",
                Description = "URL for the LocalWeather API. Used by the OldWebApp MVC project to be hosted on BeanStalk",
                ParameterName = "/dotnetonaws/localweatherapi/url"
            });
        }
    }
}
