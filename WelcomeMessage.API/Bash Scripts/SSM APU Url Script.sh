AWS_REGION=ap-southeast-2
API_GATEWAY_STAGE=Prod
API_GATEWAY_ID=$(aws apigateway get-rest-apis | jq '.items | .[] | select(.name == "<lambda application name>") | .id' --raw-output)
echo "https://$API_GATEWAY_ID.execute-api.$AWS_REGION.amazonaws.com/$API_GATEWAY_STAGE"
aws ssm put-parameter --name "/dotnetonaws/welcomemessageapi/url" --value "http://$CLUSTER_ALB_URL" --type "SecureString""