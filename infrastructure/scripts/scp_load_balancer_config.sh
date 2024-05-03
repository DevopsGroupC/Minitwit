#!/bin/bash

# load balancer config file
config_file='temp/load_balancer.conf'

# ssh key
key_file='ssh_key/terraform'

# ugly list concatenating of ips from terraform output
rows=$(terraform output -raw minitwit-swarm-leader-ip-address)
rows+=' '
rows+=$(terraform output -json minitwit-swarm-manager-ip-address | jq -r .[])
rows+=' '
rows+=$(terraform output -json minitwit-swarm-worker-ip-address | jq -r .[])

# scp the file
for ip in $rows; do
    ssh -o 'StrictHostKeyChecking no' root@$ip -i $key_file "mkdir /loadbalancer"
    scp -i $key_file $config_file root@$ip:/loadbalancer/default.conf
done
