# api token
# here it is exported in the environment like
# export TF_VAR_do_token=xxx
variable "do_token" {}

# do region
variable "region" {}

# make sure to generate a pair of ssh keys
variable "pub_key" {}
variable "pvt_key" {}

# export TF_VAR_existing_ip=xxx
variable "existing_ip" {}
variable "STAGE" {}

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
                grafana = {
                        source = "grafana/grafana"
                        version = "2.18.0"
                }
        }
}

provider "digitalocean" {
  token = var.do_token
}

provider "grafana" {
  # Configuration options
}