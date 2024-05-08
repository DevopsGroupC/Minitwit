# Create cloud VM for Grafana server
resource "digitalocean_droplet" "grafana-server" {

  image   = "ubuntu-22-04-x64"  # Use Ubuntu 22.04 image
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

      # Function to wait for apt lock to be available
      "export DEBIAN_FRONTEND=noninteractive",
      "export NEEDRESTART_MODE=a",
      
      "sudo --preserve-env=DEBIAN_FRONTEND,NEEDRESTART_MODE apt-get update -y",

      "sudo apt-get install -y adduser libfontconfig1 musl",
      "wget https://dl.grafana.com/enterprise/release/grafana-enterprise_10.4.2_amd64.deb",
      "sudo dpkg -i grafana-enterprise_10.4.2_amd64.deb",


      # "sudo apt-get install -y apt-transport-https software-properties-common wget",
      # "sudo mkdir -p /etc/apt/keyrings/",
      # "wget -q -O - https://apt.grafana.com/gpg.key | gpg --dearmor | sudo tee /etc/apt/keyrings/grafana.gpg > /dev/null",
      # "echo 'deb [signed-by=/etc/apt/keyrings/grafana.gpg] https://apt.grafana.com stable main' | sudo tee -a /etc/apt/sources.list.d/grafana.list",
      # # "sudo apt-get update",
      # # Install Grafana and Loki together
      # "sudo apt-get update",

      # "wait_for_apt",
      # "sudo apt-get install -y grafana-enterprise loki",

      # Start Grafana service
      "sudo --preserve-env=DEBIAN_FRONTEND,NEEDRESTART_MODE apt-get update -y",

      "sudo --preserve-env=DEBIAN_FRONTEND,NEEDRESTART_MODE apt-get upgrade -y",

      "sudo systemctl daemon-reload",
      "sudo systemctl start grafana-server",
      "sudo systemctl enable grafana-server.service",

      # TODO: customise log path

      "ufw allow 3000",
      "'"
    ]
  }
}

# TODO: the above website shows that i can connect to grafana via terraform and add datasources
# Add sql datasource
# Add minitwit prometheus datasource
# Add loki datasource (before adding loki datasource, we must also configure loki)


#source: https://registry.terraform.io/providers/grafana/grafana/latest/docs/resources/data_source
resource "grafana_data_source" "database" {
  depends_on          = [digitalocean_droplet.grafana-server]
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

resource "grafana_data_source" "loki" {
  depends_on          = [digitalocean_droplet.grafana-server]
  type                = "loki"
  name                = "Loki"
  url                 = "http://${digitalocean_droplet.grafana-server.ipv4_address}:3100" 
  access_mode         = "proxy"
}

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