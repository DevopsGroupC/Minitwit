#  _                    _ _           _                           
# | |    ___   __ _  __| | |__   __ _| | __ _ _ __   ___ ___ _ __ 
# | |   / _ \ / _` |/ _` | '_ \ / _` | |/ _` | '_ \ / __/ _ \ '__|
# | |__| (_) | (_| | (_| | |_) | (_| | | (_| | | | | (_|  __/ |   
# |_____\___/ \__,_|\__,_|_.__/ \__,_|_|\__,_|_| |_|\___\___|_|

resource "digitalocean_loadbalancer" "public" {
  name   = "loadbalancer-1"
  region = "fra1"

  forwarding_rule {
    entry_port     = 443
    entry_protocol = "https"
    target_port    = 5000
    target_protocol = "http"
    certificate_name = digitalocean_certificate.kvidder_cert.name
  }
  
  redirect_http_to_https = true

  healthcheck {
    port                  = 5000
    protocol              = "http"
    path                  = "/public"
    check_interval_seconds = 60
    response_timeout_seconds = 10
    healthy_threshold      = 5
    unhealthy_threshold    = 3
  }

  sticky_sessions {
    type = "cookies"
    cookie_name = "DO_LB_SESSION"
    cookie_ttl_seconds = 34650 // 9,6 hours (max)
  }

  droplet_ids = flatten([
    [digitalocean_droplet.minitwit-swarm-leader.id],
    digitalocean_droplet.minitwit-swarm-manager.*.id,
    digitalocean_droplet.minitwit-swarm-worker.*.id
  ])
}

resource "digitalocean_certificate" "kvidder_cert" {
  name     = "KvidderCertificate"
  type     = "lets_encrypt"
  domains  = ["kvidder.dk", "www.kvidder.dk"]
}