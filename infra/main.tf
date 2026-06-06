# Composition root for the platform infrastructure. Module calls are commented until the cloud
# provider is selected (ADR-0003); each module is a reviewable, reproducible unit (no click-ops).
#
# module "network"       { source = "./modules/network" }
# module "database"      { source = "./modules/database" }       # Postgres, PITR, in-KSA, encrypted
# module "cache"         { source = "./modules/cache" }          # Redis
# module "storage"       { source = "./modules/storage" }        # object storage for documents (in-KSA)
# module "secrets"       { source = "./modules/secrets" }        # secrets manager
# module "observability" { source = "./modules/observability" }  # logs, metrics, traces, alerting
