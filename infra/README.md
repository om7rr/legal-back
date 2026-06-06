# Infrastructure (Terraform skeleton)

⚠️ **Generated code only — not applied.** `terraform apply` provisions real cloud resources and
**requires human execution with credentials**. This directory currently contains a provider-agnostic
skeleton; the cloud provider, provider block, and remote-state backend are added once selected
(see `adr/0003-iac-provider-deferred.md`).

## Principles
- Everything reproducible and reviewable — no click-ops.
- Secrets come from the secrets manager, never from code or committed `*.tfvars`.
- All resources in a **live, CST-licensed KSA region** (PDPL residency).
- Encryption at rest + in transit; Postgres point-in-time recovery (DR targets: RPO ≤ 15 min, RTO ≤ 1 hr — see `docs/dr.md`).

## Layout
- `versions.tf` · `variables.tf` · `main.tf` — root composition.
- `modules/` — `network`, `database`, `cache`, `storage`, `secrets`, `observability` (stubs).

## When a provider is chosen (ADR-0003)
1. Add `required_providers` + `backend` in `versions.tf`.
2. Flesh out each module for the provider's resources.
3. `terraform init && terraform plan` reviewed in PR; `apply` is a human-gated, credentialed step.
