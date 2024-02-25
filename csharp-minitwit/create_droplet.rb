#!/usr/bin/ruby

# To create a droplet, make sure to have a access token stored in the .env-file. Here reguired through the gem dotenv. 
 
require 'dotenv'
Dotenv.load

require 'droplet_kit'
token= ENV["PROVISION_TOKEN"]
client = DropletKit::Client.new(access_token: token)

droplet = DropletKit::Droplet.new(name: 'csharp-minitwit', region: 'fra1', size: 's-1vcpu-1gb', image: 'ubuntu-22-04-x64')
client.droplets.create(droplet) 
