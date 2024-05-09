# Create cloud VM for Grafana server
resource "digitalocean_droplet" "grafana-server" {

  image   = "docker-20-04"
  name    = "grafana-server-${var.STAGE}"
  region  = var.region
  monitoring = true
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

  provisioner "file" {
    source = "grafana/docker-compose.yml"
    destination = "/root/docker-compose.yml"
  }

  provisioner "file" {
    source = "grafana/loki-config.yml"
    destination = "/root/loki-config.yml"
  }

  provisioner "file" {
    source = "grafana/prometheus.yml"
    destination = "/root/prometheus.yml"
  }

  provisioner "remote-exec" {
    inline = [
      "ufw allow 3000",
      "ufw allow 3100",
      "ufw allow 9090"
    ]
  }
}

resource "null_resource" "run_docker_compose" {
  depends_on = [digitalocean_volume_attachment.minitwit-data]

  connection {
      user        = "root"
      host        = digitalocean_droplet.grafana-server.ipv4_address
      type        = "ssh"
      private_key = file(var.pvt_key)
      timeout     = "2m"
    }

  # mount the volume 
  provisioner "remote-exec" {
    inline = [
      "mkdir -p /mnt/minitwit_data",
      "mount -o discard,defaults,noatime /dev/disk/by-id/scsi-0DO_Volume_minitwit-data /mnt/minitwit_data",
      "echo '/dev/disk/by-id/scsi-0DO_Volume_minitwit-data /mnt/minitwit_data ext4 defaults,nofail,discard 0 0' | sudo tee -a /etc/fstab",
      "mkdir -p /mnt/minitwit_data/grafana",
      "mkdir -p /mnt/minitwit_data/loki",
      "mkdir -p /mnt/minitwit_data/prometheus",
      "sed -i 's/__TARGET_IP__/${digitalocean_droplet.minitwit-swarm-leader.ipv4_address}/g' /root/prometheus.yml",
      "docker compose up -d",
      "until curl -sf http://${digitalocean_droplet.grafana-server.ipv4_address}:3000; do echo 'Waiting for server to respond...'; sleep 5; done"
    ]
  }
}

#source: https://registry.terraform.io/providers/grafana/grafana/latest/docs/resources/data_source
resource "grafana_data_source" "database" {
  depends_on          = [null_resource.run_docker_compose]
  type                = "postgres"
  name                = "minitwit-database"
  url                 = var.database_url 
  uid                 = "adhtrrc5eetq8a"
  database_name       = var.database_name 
  username            = var.database_user
  secure_json_data_encoded = jsonencode({
    password = var.database_pwd
    tlsCACert = file(var.CA_cert_path)
  })

  json_data_encoded           = jsonencode({
    sslmode                   = "verify-ca"
    "tlsConfigurationMethod"  = "file-content"
    maxOpenConns = 4
    maxIdleConns = 4
  })
}

resource "grafana_data_source" "prometheus" {
  depends_on          = [null_resource.run_docker_compose]
  type                = "prometheus"
  name                = "minitwit"
  uid                 = "cdhtgakl62xhce"
  url                 = "http://${digitalocean_droplet.grafana-server.ipv4_address}:9090" 
}

resource "grafana_data_source" "loki" {
  depends_on          = [null_resource.run_docker_compose]
  type                = "loki"
  name                = "Loki"
  url                 = "http://${digitalocean_droplet.grafana-server.ipv4_address}:3100" 
  access_mode         = "proxy"
}

resource "grafana_dashboard" "minitwit_dashboard" {
  depends_on          = [null_resource.run_docker_compose]
  config_json         = file("${path.module}/grafana/dashboard.json")
  overwrite           = true
}

resource "grafana_organization_preferences" "test" {
  depends_on          = [grafana_dashboard.minitwit_dashboard]
  home_dashboard_uid = grafana_dashboard.minitwit_dashboard.uid
}

output "grafana-server-ip-address" {
  value = digitalocean_droplet.grafana-server.ipv4_address
}