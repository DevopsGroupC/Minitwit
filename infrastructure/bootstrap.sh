#!/bin/bash

set -e

echo -e "\n--> Bootstrapping Minitwit\n"

# Check if the variable is empty
if [ -z "$1" ]; then
    echo "Environment variable is empty. Please set it to either 'production' or 'staging'."
    exit 1
fi

# Check if the variable is either "production" or "staging"
if [ "$1" != "production" ] && [ "$1" != "staging" ]; then
    echo "Invalid environment. Please set the variable to either 'production' or 'staging'."
    exit 1
fi

echo -e "\n--> Loading environment variables from secrets file\n"
if [ "$1" = "production" ]; then
    source secrets
elif [ "$1" = "staging" ]; then
    source secrets.staging
fi

# Proceed with the script
export TF_VAR_STAGE=$1
echo "Environment is set to $1"

export TF_VAR_pub_key=ssh_key/terraform-$1.pub
export TF_VAR_pvt_key=ssh_key/terraform-$1
echo "ssh_key paths are set to $TF_VAR_pub_key && $TF_VAR_pvt_key"

echo -e "\n--> Checking that environment variables are set\n"
# check that all variables are set
[ -z "$TF_VAR_do_token" ] && echo "TF_VAR_do_token is not set" && exit
[ -z "$SPACE_NAME" ] && echo "SPACE_NAME is not set" && exit
[ -z "$STATE_FILE" ] && echo "STATE_FILE is not set" && exit
[ -z "$AWS_ACCESS_KEY_ID" ] && echo "AWS_ACCESS_KEY_ID is not set" && exit
[ -z "$AWS_SECRET_ACCESS_KEY" ] && echo "AWS_SECRET_ACCESS_KEY is not set" && exit
[ -z "$DOCKER_USERNAME" ] && echo "DOCKER_USERNAME is not set" && exit
[ -z "$ConnectionStrings__DefaultConnection" ] && echo "ConnectionStrings__DefaultConnection is not set" && exit

echo -e "\n--> Checking that environment variables for monitoring server are set\n"
[ -z "$TF_VAR_grafana_auth" ] && echo "TF_VAR_grafana_auth is not set" && exit
[ -z "$TF_VAR_database_name" ] && echo "TF_VAR_database_name is not set" && exit
[ -z "$TF_VAR_database_user" ] && echo "TF_VAR_database_user is not set" && exit
[ -z "$TF_VAR_database_pwd" ] && echo "TF_VAR_database_pwd is not set" && exit
[ -z "$TF_VAR_database_url" ] && echo "TF_VAR_database_pwd is not set" && exit
[ -z "$TF_VAR_CA_cert_path" ] && echo "TF_VAR_database_pwd is not set" && exit

mkdir -p temp
mkdir -p ssh_key

# Check if the required SSH key files exist
if [ ! -f "$TF_VAR_pvt_key" ] && [ ! -f "$TF_VAR_pub_key" ]; then
    # Generate SSH key pair
    ssh-keygen -t rsa -b 4096 -q -N '' -f $TF_VAR_pvt_key

    # Set permissions for the private key
    chmod 600 $TF_VAR_pvt_key

    echo "SSH key pair generated successfully."
else
    echo "SSH key pair already exists."
fi


echo -e "\n--> Initializing terraform\n"
# initialize terraform
terraform init \
    -backend-config "bucket=$SPACE_NAME" \
    -backend-config "key=$STATE_FILE" \
    -backend-config "access_key=$AWS_ACCESS_KEY_ID" \
    -backend-config "secret_key=$AWS_SECRET_ACCESS_KEY"

echo -e "\n--> Creating/selecting terraform workspace\n"
terraform workspace select $1 || terraform workspace new $1

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

command="export STAGE=$TF_VAR_STAGE DOCKER_USERNAME=$DOCKER_USERNAME ConnectionStrings__DefaultConnection='$ConnectionStrings__DefaultConnection' && docker stack deploy minitwit -c minitwit_stack.yml"
echo "$command"

ssh \
    -o 'StrictHostKeyChecking no' \
    root@$(terraform output -raw minitwit-swarm-leader-ip-address) \
    -i $TF_VAR_pvt_key \
    "$command"

echo -e "\n--> Done bootstrapping Minitwit"
echo -e "--> Site will be available @ http://$(terraform output -raw public_ip)"
echo -e "--> Monitoring site will be available @ http://$(terraform output -raw grafana-server-ip-address)"
echo -e "--> ssh to swarm leader with 'ssh root@$(terraform output -raw minitwit-swarm-leader-ip-address) -i ssh_key/terraform'"
echo -e "--> To remove the infrastructure run: terraform destroy -auto-approve"
