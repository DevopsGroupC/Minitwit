# Create cloud VM for Grafana server
resource "digitalocean_droplet" "grafana-server" {

  image   = "docker-20-04"
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

  provisioner "file" {
    source = "grafana_dashboards/docker-compose.yml"
    destination = "/root/docker-compose.yml"
  }

  provisioner "file" {
    source = "grafana_dashboards/loki-config.yml"
    destination = "/root/loki-config.yml"
  }

  provisioner "remote-exec" {
    inline = [
      "ufw allow 3000"
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

  # save the worker join token
  provisioner "remote-exec" {
    inline = [
      "mkdir -p /mnt/minitwit_data",
      "mount -o discard,defaults,noatime /dev/disk/by-id/scsi-0DO_Volume_minitwit-data /mnt/minitwit_data",
      "echo '/dev/disk/by-id/scsi-0DO_Volume_minitwit-data /mnt/minitwit_data ext4 defaults,nofail,discard 0 0' | sudo tee -a /etc/fstab",
      "mkdir -p /mnt/minitwit_data/grafana",
      "mkdir -p /mnt/minitwit_data/loki",
      "docker compose up -d",
    ]
  }
}


# TODO: the above website shows that i can connect to grafana via terraform and add datasources
# Add sql datasource
# Add minitwit prometheus datasource
# Add loki datasource (before adding loki datasource, we must also configure loki)


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
    tlsClientCert = "" # TODO: add ssl cert in secrets
  })

  json_data_encoded = jsonencode({
    sslmode          = "require"
    maxOpenConns = 4
    maxIdleConns = 4
  })
}

resource "grafana_data_source" "prometheus" {
  depends_on          = [digitalocean_droplet.grafana-server]
  type                = "prometheus"
  name                = "minitwit"
  uid                 = "cdhtgakl62xhce"
  url                 = "http://${digitalocean_droplet.minitwit-swarm-leader.ipv4_address}:9090" 
}

# resource "grafana_data_source" "loki" {
#   depends_on          = [digitalocean_droplet.grafana-server]
#   type                = "loki"
#   name                = "Loki"
#   url                 = "http://${digitalocean_droplet.grafana-server.ipv4_address}:3100" 
#   access_mode         = "proxy"
# }

resource "grafana_folder" "minitwit_folder" {
  depends_on          = [digitalocean_droplet.grafana-server]
  title               = "Minitwit"
  uid                 = "minitwit-uid"
}

resource "grafana_dashboard" "minitwit_dashboard" {
  depends_on          = [digitalocean_droplet.grafana-server, grafana_data_source.database, grafana_data_source.prometheus, grafana_folder.minitwit_folder]
  folder              = grafana_folder.minitwit_folder.uid
  config_json         = file("${path.module}/grafana_dashboards/dashboard.json")
}

resource "grafana_organization_preferences" "test" {
  depends_on          = [grafana_dashboard.minitwit_dashboard]
  home_dashboard_uid = grafana_dashboard.minitwit_dashboard.uid
}

output "grafana-server-ip-address" {
  value = digitalocean_droplet.grafana-server.ipv4_address
}