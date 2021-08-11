echo starting CDK deploy AzureDevOpsECSStack
cdk deploy AzureDevOpsECSStack --ci --require-approval never
echo finished CDK deploy