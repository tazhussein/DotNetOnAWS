# .NET on AWS CDK Project

This project shows how to use .NET with CDK to create infrastructure to deploy workloads onto AWS from Azure DevOps Pipelines and AWS CodePipeline.

If you have any questions, feel free to reach out to me on [Twitter](http://twitter.com/husseintaz) or [LinkedIn](https://www.linkedin.com/in/tasleem-taz-hussein-b300a577/)

## How to install the AWS CLI and the AWS CDK
Install the AWS CLI onto windows. Please follow the steps at the following location [link to the AWS CLI user guide](https://docs.aws.amazon.com/cli/latest/userguide/install-cliv2-windows.html)

To use CDK from your local windows machine please follow the steps below to install the AWS CDK:
- Install NPM onto windows [link to npm Docs](https://docs.npmjs.com/downloading-and-installing-node-js-and-npm)
- Open the windows command prompt and run the following commands to install the AWS CDK:

	```
	npm install -g aws-cdk
	cdk --version
	```
	
	This should show you the version of CDK if it has installed correctly. If you see a version number then bootstrap your AWS environment:
	```
	cdk bootstrap
	```

### How to deploy the CDK Stacks
Copy this application from Github into a source repository in AWS CodeCommit. Use the same structure as this Github repository.

To deploy the applications onto AWS from your local machine run the following commands from the windows CLI once the AWS CLI and CDK is installed.

- Open the windows command prompt 
- Navigate to the root directory of the project where the cdk.json file is located
- Run the following commands

  ```
	cdk list
  ```
  
This should show you the stacks available for deployment as below

  ```
  BeanstalkPipelineStack
  ECSECRPipelineStack
  LambdaPipelineStack	
  ```

Each item in the list is a stack available for deployment. Running the commands will create a CI/CD pipeline in AWS CodePipeline.

Run the following commands to deploy the CDK stacks onto AWS, create the CI/CD pipeline and deploy the apis and MVC webapp onto AWS:
  
  ```
  cdk deploy LambdaPipelineStack
  cdk deploy ECSECRPipelineStack
  cdk deploy BealstalkPipelineStack
  ```

Once deployed, log into your AWS console to investigate the AWS CloudFormation stacks being created along with the relevant AWS CodePipeline actions.
***Do not forget to dispose of the deployments once you are done to avoid unexpected charges!***

### Solution
This solution consists of 4 projects: <br>

```
(Solution) GettingStarted
|  buildspec-localweatherapi.yml
|  buildspec-oldwebapp.yml
|
|--(Project) GettingStartedCdk
|
|--(Project) LocalWeather.API
|
|--(Project) OldWebApp
|
|--(Project) WelcomeMessage.API
```

**GettingStartedCDK** is a CDK project containing code (using .NET Core) which will create infrastructure to
host the above mentioned workloads.


**LocalWeather.API** is a .NET core restful API which will be deployed onto Amazon Elastic Container Service as a containerized workload.


**OldWebApp** is an ASP.NET Core MVC project which will be dpeloyed onto Elastic Beanstalk.


**WelcomeMessage.API** is a .NET Core restful API which will be deployed onto AWS Lambda as a serverless application.


### GettingStartedCDK for Azure DevOps deployments
The follow CDK code will be triggered from Azure DevOps Pipelines:
- GettingStartedCDK.LocalWeatherAPICdk.Azure.ECSInfraStack.cs: Creates an ECS enviroment to deploy the LocalWeather.API project onto
- GettingStartedCDK.OldWebAppCdk.OldWebAppInfraStack.cs: Creates an Elastic Beanstalk enviroment to deploy the OldWebApp project onto

### GettingStartedCDK for AWS deployments
The following CDK core will be run from the AWS CLI to create CI/CD pipelines:


- **GettingStartedCDK.LocalWeatherAPICdk.AWS.ECSECRPipelineStack.cs:**
	- Creates an ECR repository to host a Docker Image
	- Creates an ECS environment to host our LocalWeather.API web service. This will create a new VPC and associated networking components
	- Creates a CI/CD pipeline (and associated build & deployment steps) to compile our application, create an image in our ECR repository and deploy the image onto an ECS task
	- Uses the buildspec-localweatherapi.yml buildspec to create the docker image (using the Dockerfile)
	- Uses the Dockerfile to compile the .NET Core application before it is deployed onto the Docker image


- **GettingStartedCDK.WelcomeMessageAPICdk.AWS.LambdaPipelineStack.cs:**
	- Creates a CI/CD pipeline (and associated build & deployment steps) to compile our application and deploy it as a serverless application onto AWS Lambda
	- Creates an Amazon API Gateway endpoint to front our API
	- The buildspec.yml file is built incode to demonstrate another option for creating the buildspec.yml file (as opposed to using a pre-created file as per the ECSECRPipelineStack)


- **GettingStartedCDK.OldWebAppCdk.OldWebAppPipelineStack.cs:**
	- Creates an Elastic Beanstalk environment to host our web application
	- Creates a CI/CD pipeline (and associated build & deployment steps) to compile our application and deploy it onto Elastic Beanstalk
	- Uses the buildspec-oldwebapp.yml buildspec to compile the .NET Core application and publish it to S3 for dpeloyment onto Elastic Beanstalk
