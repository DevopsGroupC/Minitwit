#!/usr/bin/ruby
# Not able to load the enviroment files up to the server.
# To create a droplet, make sure to have a access token stored in the .env-file. Here reguired through the gem dotenv. 
require 'dotenv'
Dotenv.load

require 'droplet_kit'
token= ENV["PROVISION_TOKEN"]
docker_user= ENV["DOCKER_USERNAME"]
docker_pw= ENV["DOCKER_PASSWORD"]
client = DropletKit::Client.new(access_token: token)

user_data_script= <<-SCRIPT
    #!/bin/bash\n
    sudo apt-get update\n
    sudo killall apt apt-get\n
    sudo rm /var/lib/dpkg/lock-frontend\n
    sudo apt-get install -y docker.io docker-compose-v2\n
    sudo systemctl status docker\n
    docker run --rm hello-world\n
    ufw allow 5000\n
    ufw allow 22/tcp\n
    echo 'Droplet setup done'\n
    echo 'csharp_minitwit will later be accessible at http://$(hostname -I | awk '{print $1}'):5000'\n
    SCRIPT


droplet = DropletKit::Droplet.new(name: 'csharp-minitwit', region: 'fra1', size: 's-1vcpu-1gb', image: 'ubuntu-22-04-x64', ssh_keys:ENV["SSH_KEY_ID"],timeout: 120, user_data: user_data_script)

client.droplets.create(droplet) 

