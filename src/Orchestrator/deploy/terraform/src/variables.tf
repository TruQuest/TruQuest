variable "app_version" {
  type    = number
  default = 1
}

variable "image_registry_uri" {
  default = "547869531597.dkr.ecr.us-east-1.amazonaws.com"
}

variable "migrator_image_uri" {
  default = "547869531597.dkr.ecr.us-east-1.amazonaws.com/truquest_migrator:latest"
}

variable "db_username" {
  default = "truquestuser"
}

variable "db_password" {
  default = "password"
}

variable "bastion_private_key" {}