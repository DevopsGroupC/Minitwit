# TODO: look into this 
resource "digitalocean_reserved_ip" "public-ip" {
  region = var.region
}

resource "digitalocean_reserved_ip_assignment" "public-ip" {
  ip_address = digitalocean_reserved_ip.public-ip.ip_address
  droplet_id = digitalocean_droplet.minitwit-swarm-leader.id
}

output "public_ip" {
  value = digitalocean_reserved_ip.public-ip.ip_address
}
