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
      # source: https://grafana.com/docs/grafana/latest/setup-grafana/installation/debian/
      # Add Grafana repository and install Grafana
      "sudo apt-get install -y apt-transport-https software-properties-common wget",
      "sudo mkdir -p /etc/apt/keyrings/",
      "wget -q -O - https://apt.grafana.com/gpg.key | gpg --dearmor | sudo tee /etc/apt/keyrings/grafana.gpg > /dev/null",
      "echo 'deb [signed-by=/etc/apt/keyrings/grafana.gpg] https://apt.grafana.com stable main' | sudo tee -a /etc/apt/sources.list.d/grafana.list",
      "sudo apt-get update",
      # Installs the latest Enterprise release:
      "sudo apt-get install grafana-enterprise",

      # Start Grafana service
      "sudo systemctl daemon-reload",
      "sudo systemctl start grafana-server",
      "sudo systemctl enable grafana-server.service"
    ]
  }
}

output "grafana-server-ip-address" {
  value = digitalocean_droplet.grafana-server.ipv4_address
}