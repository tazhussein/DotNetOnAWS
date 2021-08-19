echo starting CDK deploy AzureDevOpsECSStack to AWS
cdk deploy AzureDevOpsECSStack --ci --require-approval never
echo finished CDK deploy