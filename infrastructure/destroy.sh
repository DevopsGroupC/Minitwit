#!/bin/bash

set -e

echo -e "\n--> Destroying Minitwit\n"

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
    source secrets-production
elif [ "$1" = "staging" ]; then
    source secrets-staging
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

mkdir -p temp
mkdir -p ssh_key

# Check if the required SSH key files exist
if [ ! -f "$TF_VAR_pvt_key" ] && [ ! -f "$TF_VAR_pub_key" ]; then
    echo "SSH key pair not present" && exit
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
    
# Remove infrastructure
echo -e "\n--> Removing Infrastructure\n"
terraform destroy -auto-approve

# Delete terraform workspace
terraform workspace select default
terraform workspace delete $1

## OPTIONAL: Destroys a target resource:
## Comment out the resource above when using this one:
# terraform destroy --target digitalocean_droplet.grafana-server