# api token
# here it is exported in the environment like
# export TF_VAR_do_token=xxx
variable "do_token" {}

# do region
variable "region" {}

# make sure to generate a pair of ssh keys
variable "pub_key" {}
variable "pvt_key" {}

# setup the provider
terraform {
        required_providers {
                digitalocean = {
                        source = "digitalocean/digitalocean"
                        version = "~> 2.37.1"
                }
                null = {
                        source = "hashicorp/null"
                        version = "3.1.0"
                }
        }
}

provider "digitalocean" {
  token = var.do_token
}