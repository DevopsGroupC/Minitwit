# Create cloud VM for Grafana server
resource "digitalocean_droplet" "grafana-server" {
  image   = "ubuntu-22-04"  # Use Ubuntu 22.04 image
  name    = "grafana-server-${var.STAGE}"
  region  = var.region
  size    = "s-1vcpu-2gb"
  ssh_keys = [digitalocean_ssh_key.minitwit.fingerprint]

  # SSH connection configuration
  connection {
    user        = "root"
    host        = self.ipv4_address
    type        = "ssh"
    private_key = file(var.pvt_key)
    timeout     = "2m"
  }

  # Install Grafana using provisioner
  provisioner "remote-exec" {
    inline = [
      # Add Grafana repository and install Grafana
      "wget -q -O - https://packages.grafana.com/gpg.key | sudo apt-key add -",
      "echo 'deb https://packages.grafana.com/oss/deb stable main' | sudo tee -a /etc/apt/sources.list.d/grafana.list",
      "sudo apt-get update",
      "sudo apt-get install -y grafana",

      # Start Grafana service
      "sudo systemctl daemon-reload",
      "sudo systemctl start grafana-server",
      "sudo systemctl enable grafana-server"
    ]
  }
}

output "grafana-server-ip-address" {
  value = digitalocean_droplet.grafana-server.ipv4_address
}