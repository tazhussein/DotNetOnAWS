CLUSTER_ARN=$(aws ecs list-clusters | jq '.clusterArns | map(select(. | contains("/<ecs cluster name>")))[0]' --raw-output)
echo $CLUSTER_ARN
CLUSTER_SERVICE_ARN=$(aws ecs list-services --cluster $CLUSTER_ARN | jq '.serviceArns[0]' --raw-output)
echo $CLUSTER_SERVICE_ARN
CLUSTER_ALB_TG_ARN=$(aws ecs describe-services --cluster $CLUSTER_ARN --services $CLUSTER_SERVICE_ARN | jq '.services | .[] | .loadBalancers[0].targetGroupArn' --raw-output)
echo $CLUSTER_ALB_TG_ARN
CLUSTER_ALB_ARN=$(aws elbv2 describe-target-groups --target-group-arns $CLUSTER_ALB_TG_ARN | jq '.TargetGroups[0].LoadBalancerArns[0]' --raw-output)
echo $CLUSTER_ALB_ARN
CLUSTER_ALB_URL=$(aws elbv2 describe-load-balancers --load-balancer-arns $CLUSTER_ALB_ARN | jq '.LoadBalancers | .[] | .DNSName' --raw-output)
echo $CLUSTER_ALB_URL
aws ssm put-parameter --name "/dotnetonaws/localweatherapi/url" --value "http://$CLUSTER_ALB_URL" --type "SecureString"