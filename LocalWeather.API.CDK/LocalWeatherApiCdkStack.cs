using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ECR;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.ECS.Patterns;
using HealthCheck = Amazon.CDK.AWS.ElasticLoadBalancingV2.HealthCheck;
using Amazon.CDK.AWS.SSM;

namespace LocalWeatherApiCdk
{
    public class LocalWeatherApiCdkStack : Stack
    {
        internal LocalWeatherApiCdkStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            string ssmAPIParameterName = "/dotnetonaws/localweatherapi/url";
            string apiResourcePath = "/localweather";
            string ecsServiceName = "localweathersvc";
            string ecsClusterName = "localweathercluster";
            string ecrRepoNameId = "localweather";
            string elbName = "localweatherelb";

            //Get reference for ECR created and image deployed by Azure DevOps Pipeline
            IRepository repo = Repository.FromRepositoryName(this, ecrRepoNameId, ecrRepoNameId);
            var image = ContainerImage.FromEcrRepository(repo, "latest");


            /********************************************************************************************************************************
            * Using Amazon.CDK.AWS.ECS.Patterns constructs to reduce lines of code needed to create ECS Cluster, Service, Tasks and ALB.
            * Unless an existing or pre-defined VPC is passed in this construct will create a new VPc and associated networking components.
            ********************************************************************************************************************************/
            var fargateECS = new ApplicationLoadBalancedFargateService(this, ecsServiceName,
                new ApplicationLoadBalancedFargateServiceProps
                {
                    TaskImageOptions = new ApplicationLoadBalancedTaskImageOptions
                    {
                        Image = image
                    },
                    PublicLoadBalancer = true, //loadbalancer will be provisioned
                    DesiredCount = 3, //I want 3 tasks running at any given time
                    ServiceName = ecsServiceName,
                    Cluster = new Cluster(this, ecsClusterName,
                        new ClusterProps { ClusterName = ecsClusterName }),
                    LoadBalancerName = elbName
                }); ;

            //Add API controller name to ALB healthcheck
            fargateECS.TargetGroup.ConfigureHealthCheck(new HealthCheck() 
            { 
                Path = apiResourcePath
            });


            //Create an SSM parameter to store the API URL. To be used by our MVC web page OldWebApp
            var apiUrlParam = new StringParameter(this, ssmAPIParameterName, new StringParameterProps
            {
                StringValue = "http://" + fargateECS.LoadBalancer.LoadBalancerDnsName + "/",
                Description = "URL for the LocalWeather API. Used by the OldWebApp MVC project to be hosted on BeanStalk",
                ParameterName = ssmAPIParameterName
            });

        }

    }
}