data "docker_registry_image" "migrator" {
  name = var.migrator_image_uri
}

resource "docker_image" "migrator" {
  name          = data.docker_registry_image.migrator.name
  pull_triggers = [data.docker_registry_image.migrator.sha256_digest]
}

resource "docker_container" "migrator" {
  name  = "${local.prefix}-migrator"
  image = docker_image.migrator.image_id
  # If true attaches to the container after its creation and waits the end of its execution.
  attach = true
  # Save the container logs (attach must be enabled).
  logs = true
  # If true, then the container will be kept running.
  # If false, then as long as the container exists, Terraform assumes it is successful.
  must_run = false
  # If true, then the container will be automatically removed when it exits.
  # rm = true # Error: No such container XXX
  env = [
    "BASTION_HOST=${aws_elastic_beanstalk_environment.backend_staging.cname}", # should tunnel through a separate bastion host
    "BASTION_USER=ec2-user",
    "BASTION_PRIVATE_KEY=${var.bastion_private_key}",
    "DB_HOST=${aws_db_instance.main.address}",
    "DB_PORT=5432",
    "DB_NAME=TruQuest",
    "DB_USERNAME=${var.db_username}",
    "DB_PASSWORD=${var.db_password}"
  ]
}

# Docker provider stores the info about the current docker_image in the terraform state,
# but the info about the current docker_container is obtained from docker API, meaning
# that since rm == false, when we run locally if we don't manually prune the container,
# provider would see that it is still hanging around (does must_run affect this?) and deduce
# that the container resource doesn't need to be created. But if we prune the container,
# then provider would create it again even when the underlying docker_image hasn't been changed.

# In a ci/cd pipeline the container would be created every time we apply regardless of the
# updates to the underlying image.

# When we DO update the image, then of course docker_image gets updated and docker_container
# gets replaced/created (depending if the old one is still around).

# Making the attach=true container a dependency of another resource makes the resource wait
# until the container runs to completion and exits (true for both initial creation and updates).
# If the container resourse doesn't need to be created because it's still around,
# then terraform simply considers it to be already completed (which is true I guess).
