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
  }
}

provider "aws" {
  region = "us-east-1"
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