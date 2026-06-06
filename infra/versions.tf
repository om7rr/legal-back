# Terraform skeleton — provider-agnostic. The concrete cloud provider (and thus the provider
# block + backend) is chosen later via ADR-0003, after confirming a live, CST-licensed KSA region.
# APPLY IS HUMAN-EXECUTED with credentials — see infra/README.md. Nothing here provisions anything yet.
terraform {
  required_version = ">= 1.7.0"
  # required_providers { ... }  # added when the cloud is selected (ADR-0003)
  # backend "..." { }           # remote state in a KSA-region bucket (human-provisioned)
}
