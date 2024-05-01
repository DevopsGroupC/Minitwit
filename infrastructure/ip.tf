# this creates a new reserved ip
# resource "digitalocean_reserved_ip" "public-ip" {
#   region = var.region
# }

#this assigns reserved ip to the leader droplet
resource "digitalocean_reserved_ip_assignment" "public-ip" {
  # the commented out line is related to the new reserved ip creation
  # ip_address = digitalocean_reserved_ip.public-ip.ip_address
  ip_address = var.existing_ip
  droplet_id = digitalocean_droplet.minitwit-swarm-leader.id
}

output "public_ip" {
  # the commented out line is related to the new reserved ip creation
  # value = digitalocean_reserved_ip.public-ip.ip_address
  value = var.existing_ip
}
