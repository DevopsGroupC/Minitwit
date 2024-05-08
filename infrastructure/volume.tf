resource "digitalocean_volume" "minitwit-data" {
  region                  = "fra1"
  name                    = "minitwit-data"
  size                    = 100
  initial_filesystem_type = "ext4"
  description             = "Monitoring and logging data for minitwit"
}

resource "digitalocean_volume_attachment" "minitwit-data" {
  droplet_id = digitalocean_droplet.minitwit-swarm-leader.id
  volume_id  = digitalocean_volume.minitwit-data.id
}
