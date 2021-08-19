echo starting CDK deploy AzureDevOpsBeanstalkStack to AWS
cdk deploy AzureDevOpsBeanstalkStack --ci --require-approval never
echo finished CDK deploy
