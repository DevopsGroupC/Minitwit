terraform {
  backend "s3" {
    region = "fra1"
    skip_credentials_validation = true
    skip_metadata_api_check = true
    skip_region_validation = true
    skip_requesting_account_id = true
    skip_s3_checksum = true
    use_path_style = true
    acl = "private"
    endpoints = {
      s3 = "https://fra1.digitaloceanspaces.com"
    }
  }
}
