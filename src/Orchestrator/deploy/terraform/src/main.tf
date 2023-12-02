terraform {
  backend "s3" {
    bucket  = "truquest"
    key     = "truquest.tfstate"
    region  = "ap-northeast-2"
    encrypt = true
  }

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 4.0"
    }

    docker = {
      source  = "kreuzwerker/docker"
      version = "3.0.2"
    }

    local = {
      source  = "hashicorp/local"
      version = "2.4.0"
    }
  }
}

provider "aws" {
  region = "us-east-1"
}

provider "docker" {
  # host = "tcp://docker:2376"
  host = "tcp://host.docker.internal:2375"

  # ca_material   = file("/certs/client/ca.pem")
  # cert_material = file("/certs/client/cert.pem")
  # key_material  = file("/certs/client/key.pem")

  # registry_auth {
  #   address = var.image_registry_uri
  # }
}

data "aws_region" "current" {}

locals {
  prefix = "trq-${terraform.workspace}"
  common_tags = {
    Project     = "TruQuest"
    Environment = "${terraform.workspace}"
    ManagedBy   = "Terraform"
  }
}
