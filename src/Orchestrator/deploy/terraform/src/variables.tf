variable "application_version_count" {
  default = 1
}

variable "application_version_index_to_tag" {
  type = map(string)
  default = {
    0 = "0.1.0"
  }
}

# variable "image_registry_uri" {
#   default = "547869531597.dkr.ecr.us-east-1.amazonaws.com"
# }

# variable "migrator_image_uri" {
#   default = "547869531597.dkr.ecr.us-east-1.amazonaws.com/truquest_migrator:latest"
# }

# variable "contract_migrator_image_uri" {
#   default = "547869531597.dkr.ecr.us-east-1.amazonaws.com/truquest_contract_migrator:latest"
# }

# variable "db_username" {
#   default = "truquestuser"
# }

# variable "db_password" {
#   default = "password"
# }

# variable "artifacts_host_path" {}

# variable "bastion_private_key" {}
