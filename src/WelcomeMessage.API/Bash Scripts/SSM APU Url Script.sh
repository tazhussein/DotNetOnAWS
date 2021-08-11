API_GATEWAY_STAGE=Prod
API_GATEWAY_ID=$(aws apigateway get-rest-apis | jq '.items | .[] | select(.name == "WelcomeMessageAPI") | .id' --raw-output)
APU_GATEWAY_URL="https://$API_GATEWAY_ID.execute-api.$AWS_DEFAULT_REGION.amazonaws.com/$API_GATEWAY_STAGE"
aws ssm put-parameter --overwrite --name "/dotnetonaws/welcomemessageapi/url" --value "$APU_GATEWAY_URL/api" --type "String"