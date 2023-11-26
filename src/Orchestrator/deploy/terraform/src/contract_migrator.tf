# data "docker_registry_image" "contract_migrator" {
#   name = var.contract_migrator_image_uri
# }

# resource "docker_image" "contract_migrator" {
#   name          = data.docker_registry_image.contract_migrator.name
#   pull_triggers = [data.docker_registry_image.contract_migrator.sha256_digest]
# }

# resource "docker_container" "contract_migrator" {
#   name  = "${local.prefix}-contract-migrator"
#   image = docker_image.contract_migrator.image_id
#   # If true attaches to the container after its creation and waits the end of its execution.
#   attach = true
#   # Save the container logs (attach must be enabled).
#   logs = true
#   # If true, then the container will be kept running.
#   # If false, then as long as the container exists, Terraform assumes it is successful.
#   must_run = false
#   # If true, then the container will be automatically removed when it exits.
#   # rm = true # Error: No such container XXX
#   mounts {
#     type = "bind"
#     source = var.artifacts_host_path
#     target = "/app/artifacts"
#   }
# }
