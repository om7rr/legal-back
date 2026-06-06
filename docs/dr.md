# Disaster Recovery

Law firms cannot lose case files. These targets are non-negotiable.

- **RPO ≤ 15 minutes** (max acceptable data loss).
- **RTO ≤ 1 hour** (max acceptable downtime).

## Backups (when infra is provisioned — ADR-0003)
- Managed Postgres with **point-in-time recovery** (continuous WAL archiving) → meets RPO.
- Object storage (documents) versioned + replicated, **in-KSA** only.
- All backups encrypted; retention per `docs/security/data-classification.md`.

## Procedures (must be tested regularly)
- **Backup verification:** automated restore to a scratch instance, periodically.
- **Restore drill:** documented in `docs/runbook.md`; a restore-from-backup drill **must pass** during
  pilot hardening.
- **Failover:** region/instance failover steps (added with the provider).

## Status
Targets defined; implementation lands with the IaC provider (ADR-0003). No live backups exist yet.
