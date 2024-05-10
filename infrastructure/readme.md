## Guideline for Using the Provided Terraform and Shell Scripts

### Overview
The files provided are used for setting up, managing, and tearing down infrastructure with Terraform, specifically for deploying Minitwit. The infrastructure is setup for 2 environments, such as "production" and "staging". This means that based on the environment, it will dynamically create vm's  tailored for each environment. 

#### Files
- **Terraform Configuration Files (.tf)**: Define the infrastructure for different components like networks, volumes, and providers.
- **bootstrap.sh**: Script to initialize and deploy /update the Minitwit application.
- **destroy.sh**: Script to tear down the infrastructure.

#### Infrastructure Components Managed by Terraform
1. **Swarm Cluster of VMs**:
    - Creates a total of four droplets: one designated as the leader, two as managers, and one as a worker.
    - These are orchestrated to work as a Docker Swarm cluster to manage deployments and scaling of the Minitwit application.
2. **VM Monitoring Server**:
    - Provisions a single droplet dedicated to monitoring tasks.
    - Runs Grafana, Prometheus, and Loki inside Docker containers on this server.
    - Terraform utilizes Grafana provisioner to integrate with the Grafana API for setting up data sources and monitoring dashboards automatically.
3. **Volume for Monitoring Data**:
    - Attaches a persistent volume to the monitoring server droplet.
    - Stores all monitoring data generated by Grafana, Prometheus, and Loki.
    - Important: As the `destroy.sh` script tears down this volume, it is crucial to back up the data stored on it to prevent loss.
4. **Reserved IPs**:
    - Assigns pre-reserved IPs to the swarm leader server and the monitoring server, ensuring stable and consistent access points for these services.
5. **Terraform State Management**:
    - Saves the state of the Terraform deployment in Digital Ocean Spaces (compatible with AWS S3), providing a secure and centralized state management solution.

### Required Environment Variables

#### DigitalOcean Configuration

- **`export TF_VAR_do_token=`**: The DigitalOcean API token; this is used to authenticate and interact with DigitalOcean account.
- **`export SPACE_NAME=`**: The name of the DigitalOcean Space where Terraform state files will be stored.
- **`export STATE_FILE=`**: The name of the state file within the DigitalOcean Space that Terraform uses to track resource state.
- **`export AWS_ACCESS_KEY_ID=`**: AWS access key ID, used for accessing AWS services that integrate with DigitalOcean, such as S3-compatible Spaces.
- **`export AWS_SECRET_ACCESS_KEY=`**:  AWS secret access key

#### Docker Configuration
- **`export DOCKER_USERNAME=`**:  Docker Hub username, necessary for pulling and managing Docker images possibly stored in private repositories.

#### IP Configuration for VMs
- **`export TF_VAR_existing_ip=`**: The reserved IP address assigned to the swarm leader server.
- **`export TF_VAR_grafana_ip=`**: The reserved IP address assigned to the VM that hosts the monitoring services.

### Grafana Related Terraform Configuration
- **`export TF_VAR_grafana_auth=`**: The authentication string under the format of "user:password" for accessing the Grafana API.
- **`export TF_VAR_database_name=`**: The name of the database used by Grafana for storing metrics and data.
- **`export TF_VAR_database_user=`**: The username for accessing the Grafana database.
- **`export TF_VAR_database_pwd=`**: The password for the database user.
- **`export TF_VAR_database_url=`**: The URL of the database accessed by Grafana (including the port).
- **`export TF_VAR_CA_cert_path=`**: The path to the CA certificate file needed for secure database connections (default should be `grafana/ca_cert`). Store the CA certificate contents under the name "ca_cert" inside the following directory `infrastructure/grafana/`. 

#### Database Connection for Bootstrap Script
- **`export ConnectionStrings__DefaultConnection=`**: The connection string used by the bootstrap script to connect to your application's primary database. This should include credentials, server information, and other necessary details to ensure a successful database connection.

#### Setting Up These Variables
These environment variables should be set in `secrets-production` or `secrets-staging` files to load environment-specific variables. the files must be saved in the same directory as `bootstrap.sh`. Use `secrets-template` as base to setup the environment variables. Make sure to replace the placeholder values with actual, sensitive data and treat them securely to avoid unauthorized access.

### Step-by-Step Usage Guide

#### 1. Pre-requisites
- Install Terraform.
- Ensure you have DO access and secret keys, DO tokens, and any other necessary credentials.
- Install required CLI tools (e.g. Docker).

#### 2. Initial Setup
- Clone or download the repository containing the files.
- Store your secret keys and access tokens in secure files or environment variables as suggested in the scripts.

#### 3. Using `bootstrap.sh` to Deploy Infrastructure
1. **Prepare Environment Variables:**
    - Set the necessary environment variables as the script checks these before proceeding.
    - Configure `secrets` or `secrets.staging` files to load environment-specific variables. the files must be saved in the same directory as `bootstrap.sh`
1. **Run the Script:**
    - Execute `bash bootstrap.sh production` or `bash bootstrap.sh staging` depending on the target environment.
    - The script will initialize Terraform, validate configurations, and apply them to create the infrastructure. It also handles SSH key generation if SSH keys are missing/not provided.
2. **Post-Deployment:**
    - The application will be deployed, and the script will output URLs for accessing the Minitwit application and associated monitoring tools.

#### 4. Managing Infrastructure
- **Modify Terraform Configuration (.tf) Files:**
    - Adjust the configuration files to scale resources, modify network settings, or integrate additional services.
    - Run `bash bootstrap.sh production`  or `bash bootstrap.sh staging` to update the infrastructure based on the environment.
    
#### 5. Using `destroy.sh` to Tear Down Infrastructure
1. **Set Required Environment:**
    - Ensure all environment variables and secrets are correctly set as the script will verify these before proceeding.
2. **Execute the Script:**
    - Run `./destroy.sh production` or `./destroy.sh staging`.
    - This script will remove all the deployed resources and delete the associated Terraform workspace.