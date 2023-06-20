# resource "aws_db_subnet_group" "postgres" {
#   name = "${local.prefix}-postgres"
#   subnet_ids = [ # must cover at least 2 AZs
#     aws_subnet.private_a.id,
#     aws_subnet.private_b.id
#   ]

#   tags = local.common_tags
# }

# resource "aws_security_group" "postgres" {
#   name        = "${local.prefix}-postgres"
#   description = "Allows internal access to Postgres"
#   vpc_id      = aws_vpc.main.id

#   ingress {
#     protocol  = "tcp"
#     from_port = 5432
#     to_port   = 5432

#     security_groups = [aws_security_group.backend.id]
#   }

#   tags = local.common_tags
# }

# # resource "aws_rds_cluster_parameter_group" "postgres" {
# #   name        = "${local.prefix}-postgres"
# #   family      = "postgres15"
# #   description = "Enables logical replication for debezium"

# #   parameter {
# #     name  = "rds.logical_replication" # Error: parameter is not editable
# #     value = "1"
# #   }

# #   parameter {
# #     name  = "log_statement"
# #     value = "all"
# #   }

# #   tags = local.common_tags
# # }

# resource "aws_db_instance" "main" {
#   identifier               = "${local.prefix}-postgres"
#   multi_az                 = false
#   allocated_storage        = 10
#   storage_type             = "gp2"
#   db_subnet_group_name     = aws_db_subnet_group.postgres.id
#   db_name                  = "TruQuest"
#   engine                   = "postgres"
#   engine_version           = "15.3"
#   instance_class           = "db.t4g.micro"
#   username                 = var.db_username
#   password                 = var.db_password
#   parameter_group_name     = "${local.prefix}-postgres"
#   skip_final_snapshot      = true
#   backup_retention_period  = 1
#   delete_automated_backups = true
#   deletion_protection      = false
#   publicly_accessible      = false
#   vpc_security_group_ids   = [aws_security_group.postgres.id]

#   tags = local.common_tags
# }