resource "aws_ebs_volume" "backend" {
  availability_zone = "${data.aws_region.current.name}a"
  size              = 10

  tags = merge(
    local.common_tags,
    tomap({ "Name" = "${local.prefix}-backend" })
  )
}

data "archive_file" "backend_source" {
  type        = "zip"
  output_path = "./backend_source.zip"

  source {
    content = templatefile(
      "./eb/docker-compose.tftpl",
      {
        hostname               = var.hostname
        file_archive_image_uri = "${var.file_archive_image_uri}:${var.application_version_index_to_tag[var.application_version_count - 1]}"
        orchestrator_image_uri = "${var.orchestrator_image_uri}:${var.application_version_index_to_tag[var.application_version_count - 1]}"
        db_connection_string   = "Host=${aws_db_instance.main.address};Port=${aws_db_instance.main.port};Database=${var.db_name};Username=${var.db_username};Password=${var.db_password};"
        ethereum_rpc_url       = var.ethereum_rpc_url
        ethereum_chain_id      = var.ethereum_chain_id
        ethereum_l1_rpc_url    = var.ethereum_l1_rpc_url
        ethereum_l1_chain_id   = var.ethereum_l1_chain_id
        erc4337_bundler_host   = var.erc4337_bundler_host
      }
    )
    filename = "docker-compose.yml"
  }

  source {
    content  = file("./eb/.env")
    filename = ".env"
  }

  source {
    content = templatefile(
      "./eb/otel-collector-config.tftpl",
      {
        uptrace_truquest_project_secret_token = var.uptrace_truquest_project_secret_token
      }
    )
    filename = "otel-collector-config.yml"
  }

  source {
    content  = file("./eb/truquest.crt")
    filename = "truquest.crt"
  }

  source {
    content  = file("./eb/truquest.key")
    filename = "truquest.key"
  }

  source {
    content = templatefile(
      "./eb/uptrace.tftpl",
      {
        db_host                               = aws_db_instance.main.address
        db_port                               = aws_db_instance.main.port
        db_username                           = var.db_username
        db_password                           = var.db_password
        db_name                               = var.db_name
        uptrace_default_project_secret_token  = var.uptrace_default_project_secret_token
        uptrace_truquest_project_secret_token = var.uptrace_truquest_project_secret_token
        uptrace_user_email                    = var.uptrace_user_email
        uptrace_user_password                 = var.uptrace_user_password
        uptrace_secret_key                    = var.uptrace_secret_key
      }
    )
    filename = "uptrace.yml"
  }

  source {
    content  = file("./eb/commands.config")
    filename = ".ebextensions/commands.config"
  }

  source {
    content = templatefile(
      "./eb/mount-volume.tftpl",
      {
        mount_dir = "/backend"
        volume_id = aws_ebs_volume.backend.id
        region    = data.aws_region.current.name
      }
    )
    filename = ".platform/hooks/predeploy/mount-volume.sh"
  }
}

resource "aws_s3_bucket" "backend_source" {
  bucket        = "${local.prefix}-backend-source"
  force_destroy = true

  tags = local.common_tags
}

resource "aws_s3_object" "backend_source" {
  depends_on = [data.archive_file.backend_source]
  count      = var.application_version_count
  bucket     = aws_s3_bucket.backend_source.id
  key        = "backend_source_${var.application_version_index_to_tag[count.index]}.zip"
  source     = "./backend_source.zip"
}

resource "aws_iam_role" "backend" {
  name               = "${local.prefix}-backend"
  assume_role_policy = file("./eb/policies/ec2-assume-role-policy.json")

  tags = local.common_tags
}

resource "aws_iam_role_policy_attachment" "backend" {
  role       = aws_iam_role.backend.name
  policy_arn = "arn:aws:iam::aws:policy/AWSElasticBeanstalkWebTier"
}

resource "aws_iam_policy" "backend_volume_management" {
  name   = "${local.prefix}-backend-volume-management"
  policy = file("./eb/policies/backend-volume-management.json")
}

resource "aws_iam_role_policy_attachment" "backend_volume_management" {
  role       = aws_iam_role.backend.name
  policy_arn = aws_iam_policy.backend_volume_management.arn
}

resource "aws_iam_instance_profile" "backend" {
  name = "${local.prefix}-backend"
  role = aws_iam_role.backend.name
}

resource "aws_iam_role" "eb" {
  name               = "${local.prefix}-eb"
  assume_role_policy = file("./eb/policies/eb-assume-role-policy.json")

  tags = local.common_tags
}

resource "aws_iam_role_policy_attachment" "eb_updates" {
  role       = aws_iam_role.eb.name
  policy_arn = "arn:aws:iam::aws:policy/AWSElasticBeanstalkManagedUpdatesCustomerRolePolicy"
}

resource "aws_iam_role_policy_attachment" "eb_health" {
  role       = aws_iam_role.eb.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSElasticBeanstalkEnhancedHealth"
}

resource "aws_security_group" "backend" {
  name        = "${local.prefix}-backend"
  description = "Allows access from local machine"
  vpc_id      = aws_vpc.main.id

  ingress {
    protocol    = "tcp"
    from_port   = 22
    to_port     = 22
    cidr_blocks = ["${var.local_public_ip}/32"]
  }

  ingress {
    protocol    = "tcp"
    from_port   = 80
    to_port     = 80
    cidr_blocks = ["${var.local_public_ip}/32"]
  }

  ingress {
    protocol    = "tcp"
    from_port   = 443
    to_port     = 443
    cidr_blocks = ["${var.local_public_ip}/32"]
  }

  ingress {
    protocol    = "tcp"
    from_port   = 8080
    to_port     = 8080
    cidr_blocks = ["${var.local_public_ip}/32"]
  }

  ingress {
    protocol    = "tcp"
    from_port   = 5001
    to_port     = 5001
    cidr_blocks = ["${var.local_public_ip}/32"]
  }

  ingress {
    protocol    = "tcp"
    from_port   = 9094
    to_port     = 9094
    cidr_blocks = ["${var.local_public_ip}/32"]
  }

  ingress {
    protocol    = "tcp"
    from_port   = 14318
    to_port     = 14318
    cidr_blocks = ["${var.local_public_ip}/32"]
  }

  egress {
    protocol         = "-1"
    from_port        = 0
    to_port          = 0
    cidr_blocks      = ["0.0.0.0/0"]
    ipv6_cidr_blocks = ["::/0"]
  }

  tags = local.common_tags
}

resource "aws_elastic_beanstalk_application" "backend" {
  name = "${local.prefix}-backend"
  tags = local.common_tags
}

# Terraform /replaces/ (i.e. destroys and creates anew) application version,
# which means that EB version history is not maintained.

# If we want to maintain history, then we have to use 'count' here, and increment
# it with each update, so that TF /adds/ application versions.
# This necessitates also adding /new/ s3 objects (one for every version), which
# in turn necessitates creating new source archive files. But this would lead to
# us having to maintain multiple source docker-compose template files (one for every version),
# so that the archives could actually be created. We wouldn't be able to just use a single
# template file and update it in place, because then /all/ s3 object resources will use this
# single file as source.

# We use a workaround: maintain a single template file, an array of s3
# objects, and an array of EB application versions, but do not specify an 'etag' for s3 objects,
# so that when we change the template file and add a new version, it doesn't cause old s3 objects
# to be overwritten.

resource "aws_elastic_beanstalk_application_version" "backend" {
  count       = var.application_version_count
  name        = "${local.prefix}-backend-version-${var.application_version_index_to_tag[count.index]}"
  application = aws_elastic_beanstalk_application.backend.name
  bucket      = aws_s3_bucket.backend_source.id
  key         = aws_s3_object.backend_source[count.index].id

  tags = local.common_tags
}

resource "aws_elastic_beanstalk_environment" "backend" {
  depends_on          = [docker_container.db_migrator]
  name                = "${local.prefix}-backend"
  application         = aws_elastic_beanstalk_application.backend.name
  tier                = "WebServer"
  solution_stack_name = "64bit Amazon Linux 2 v3.5.8 running Docker"
  version_label       = aws_elastic_beanstalk_application_version.backend[var.application_version_count - 1].name

  setting {
    namespace = "aws:ec2:vpc"
    name      = "VPCId"
    value     = aws_vpc.main.id
  }

  setting {
    namespace = "aws:ec2:vpc"
    name      = "Subnets"
    value     = aws_subnet.public_a.id
  }

  #   setting { # Not supported for VPC environments
  #     namespace = "aws:autoscaling:asg"
  #     name      = "Custom Availability Zones"
  #     value     = "${data.aws_region.current.name}a"
  #   }

  setting {
    namespace = "aws:ec2:instances"
    name      = "InstanceTypes"
    value     = "t3a.large"
  }

  setting {
    namespace = "aws:autoscaling:launchconfiguration"
    name      = "IamInstanceProfile"
    value     = aws_iam_instance_profile.backend.name
  }

  setting {
    namespace = "aws:autoscaling:launchconfiguration"
    name      = "SecurityGroups"
    value     = aws_security_group.backend.id
  }

  setting {
    namespace = "aws:autoscaling:launchconfiguration"
    name      = "EC2KeyName"
    value     = "${local.prefix}-backend"
  }

  setting {
    namespace = "aws:autoscaling:launchconfiguration"
    name      = "RootVolumeSize"
    value     = "16"
  }

  setting {
    namespace = "aws:elasticbeanstalk:cloudwatch:logs"
    name      = "StreamLogs"
    value     = true
  }

  setting {
    namespace = "aws:elasticbeanstalk:cloudwatch:logs"
    name      = "DeleteOnTerminate"
    value     = true
  }

  setting {
    namespace = "aws:elasticbeanstalk:environment"
    name      = "EnvironmentType"
    value     = "SingleInstance"
  }

  setting {
    namespace = "aws:elasticbeanstalk:environment"
    name      = "ServiceRole"
    value     = aws_iam_role.eb.name
  }

  tags = local.common_tags
}
