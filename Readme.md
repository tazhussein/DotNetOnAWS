# .NET on AWS CDK Project

This poject shows how to use .NET with CDK to create infrastructure to deploy workloads onto AWS from Azure DevOps and AWS CodePipeline.

## How to install the AWS CDK
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

### Solution
This solution consists of 4 projects: 
**OldWebApp** is an ASP.NET Core MVC project which will be dpeloyed onto Elastic Beanstalk.
**WelcomeMessage.API** is a .NET Core restful API which will be deployed onto AWS Lambda as a serverless application.
**LocalWeather.API** is a .NET core restful API which will be deployed onto Amazon Elastic Container Service as a containerized workload.
**GettingStartedCDK** is a CDK project containing code (using .NET Core) which will create infrastructure to
host the above mentioned workloads.

### GettingStartedCDK for Azure DevOps deployments
The follow CDK code will be triggered from Azure DevOps Pipelines:
- GettingStartedCDK.LocalWeatherAPICdk.Azure.ECSInfraStack.cs: Creates an ECS enviroment to deploy the LocalWeather.API project onto
- GettingStartedCDK.OldWebAppCdk.OldWebAppInfraStack.cs: Creates an Elastic Beanstalk enviroment to deploy the OldWebApp project onto

### GettingStartedCDK for AWS deployments
The following CDK core will be run from the AWS CLI to create CI/CD pipelines:

- **GettingStartedCDK.LocalWeatherAPICdk.AWS.ECSECRPipelineStack.cs:**
	- Creates an ECR repository to host a Docker Image
	- Creates an ECS environment to host our LocalWeather.API web service. This will create a new VPC and associated networking components.
	- Creates a CI/CD pipeline (and associated build & deployment steps) to compile our application, create an image in our ECR repository and deploy the image onto an ECS task

- **GettingStartedCDK.WelcomeMessageAPICdk.AWS.LambdaPipelineStack.cs:**
	- Creates a CI/CD pipeline (and associated build & deployment steps) to compile our application and deploy it as a serverless application onto AWS Lambda. 
	- Creates an Amazon API Gateway endpoint to front our API

- **GettingStartedCDK.OldWebAppCdk.OldWebAppPipelineStack.cs:**
	- Creates an Elastic Beanstalk environment to host our web application
	- Creates a CI/CD pipeline (and associated build & deployment steps) to compile our application and deploy it onto Elastic Beanstalk. 
