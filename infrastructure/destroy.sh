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

mkdir -p temp
mkdir -p ssh_key

# Check if the required SSH key files exist
if [ ! -f "$TF_VAR_pvt_key" ] && [ ! -f "$TF_VAR_pub_key" ]; then
    echo "SSH key pair not present" && exit
fi

# Remove infrastructure
echo -e "\n--> Removing Infrastructure\n"
terraform destroy -auto-approve