# api token
# here it is exported in the environment like
# export TF_VAR_do_token=xxx
variable "do_token" {}

# do region
variable "region" {}

# make sure to generate a pair of ssh keys
variable "pub_key" {}
variable "pvt_key" {}

# custom exports
variable "existing_ip" {}
variable "STAGE" {}

#grafana and loki related configs
variable "grafana_auth" {}
variable "database_name" {}
variable "database_user" {}
variable "database_pwd" {}
variable "database_url" {}
variable "CA_cert_path" {}

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

# source: https://registry.terraform.io/providers/grafana/grafana/2.17.0/docs?utm_content=documentLink&utm_medium=Visual+Studio+Code&utm_source=terraform-ls
provider "grafana" {
  url  = "http://${digitalocean_droplet.grafana-server.ipv4_address}:3000/"
  auth = var.grafana_auth
}