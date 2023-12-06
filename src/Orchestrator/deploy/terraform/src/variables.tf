variable "hostname" {
  default = ""
}

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

variable "db_migrator_image_uri" {
  default = "chekhenkho/truquest_db_migrator"
}

variable "file_archive_image_uri" {
  default = "chekhenkho/truquest_file_archive"
}

variable "orchestrator_image_uri" {
  default = "chekhenkho/truquest_orchestrator"
}

variable "db_name" {
  default = ""
}

variable "db_username" {
  default = ""
}

variable "db_password" {
  default = ""
}

variable "uptrace_user_email" {
  default = ""
}

variable "uptrace_user_password" {
  default = ""
}

variable "uptrace_default_project_secret_token" {
  default = ""
}

variable "uptrace_truquest_project_secret_token" {
  default = ""
}

variable "uptrace_secret_key" {
  default = ""
}

variable "ethereum_rpc_url" {
  default = ""
}

variable "ethereum_chain_id" {
  default = "901"
}

variable "ethereum_l1_rpc_url" {
  default = ""
}

variable "ethereum_l1_chain_id" {
  default = "900"
}

variable "erc4337_bundler_url" {
  default = ""
}

variable "local_public_ip" {
  default = ""
}

# variable "artifacts_host_path" {
#   default = ""
# }

# variable "bastion_private_key" {}
