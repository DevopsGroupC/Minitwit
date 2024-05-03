#!/bin/bash

set -e

echo -e "\n--> Bootstrapping Minitwit\n"

echo -e "\n--> Loading environment variables from secrets file\n"
source secrets

echo -e "\n--> Checking that environment variables are set\n"
# check that all variables are set
[ -z "$TF_VAR_do_token" ] && echo "TF_VAR_do_token is not set" && exit
[ -z "$SPACE_NAME" ] && echo "SPACE_NAME is not set" && exit
[ -z "$STATE_FILE" ] && echo "STATE_FILE is not set" && exit
[ -z "$AWS_ACCESS_KEY_ID" ] && echo "AWS_ACCESS_KEY_ID is not set" && exit
[ -z "$AWS_SECRET_ACCESS_KEY" ] && echo "AWS_SECRET_ACCESS_KEY is not set" && exit

echo -e "\n--> Initializing terraform\n"
# initialize terraform
terraform init \
    -backend-config "bucket=$SPACE_NAME" \
    -backend-config "key=$STATE_FILE" \
    -backend-config "access_key=$AWS_ACCESS_KEY_ID" \
    -backend-config "secret_key=$AWS_SECRET_ACCESS_KEY"

# check that everything looks good
echo -e "\n--> Validating terraform configuration\n"
terraform validate

# create infrastructure
echo -e "\n--> Creating Infrastructure\n"
terraform apply -auto-approve

# TODO:
# # generate loadbalancer configuration
# echo -e "\n--> Generating loadbalancer configuration\n"
# bash scripts/gen_load_balancer_config.sh

# # scp loadbalancer config to all nodes
# echo -e "\n--> Copying loadbalancer configuration to nodes\n"
# bash scripts/scp_load_balancer_config.sh

# deploy the stack to the cluster
echo -e "\n--> Deploying the Csharp-Minitwit stack to the cluster\n"

command="export STAGE=$STAGE DOCKER_USERNAME=$DOCKER_USERNAME ConnectionStrings__DefaultConnection='$ConnectionStrings__DefaultConnection' && docker stack deploy minitwit -c minitwit_stack.yml"
echo "$command"

ssh \
    -o 'StrictHostKeyChecking no' \
    root@$(terraform output -raw minitwit-swarm-leader-ip-address) \
    -i ssh_key/terraform \
    "$command"

echo -e "\n--> Done bootstrapping Minitwit"
echo -e "--> Site will be available @ http://$(terraform output -raw public_ip)"
# echo -e "--> You can check the status of swarm cluster @ http://$(terraform output -raw minitwit-swarm-leader-ip-address):8888"
echo -e "--> ssh to swarm leader with 'ssh root@\$(terraform output -raw minitwit-swarm-leader-ip-address) -i ssh_key/terraform'"
echo -e "--> To remove the infrastructure run: terraform destroy -auto-approve"
