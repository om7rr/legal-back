# Security Model

This document describes the security posture of the Legal Platform. It is updated in the same change as any code that affects it.

## Principles

- **Secure by default / default-deny.** Every endpoint authorizes by role **and** tenant. Authentication alone is never sufficient.
- **No secrets in code or repo.** Local secrets via `dotnet user-secrets`; runtime config via environment; cloud via a secrets manager (human-provisioned). Enforced by gitleaks (pre-commit + CI).
- **Multi-tenant isolation.** Every query is scoped by `tenant_id` via resolver middleware + EF Core global query filter, with Postgres Row-Level Security as defense-in-depth (ADR-0002). Each module ships a negative test proving tenant A cannot read tenant B's data.
- **Compliance (PDPL/SDAIA).** All personal data is KSA-resident; no dependency or call ships PII outside the Kingdom. Cross-border transfer requires SDAIA-approved SCCs and an ADR.

## Data classification

Every entity is tagged with one of: `Public · Internal · Confidential · LegalPrivileged · RegulatedPersonalData`. Classification drives access control, logging/redaction, retention, and residency. See [`docs/security/data-classification.md`](docs/security/data-classification.md). `LegalPrivileged` and `RegulatedPersonalData` never leave KSA and never transit a non-KSA service.

## Authentication & authorization

- **Nafath** (national SSO) primary + email/OTP fallback. JWT access (short-lived) + refresh with rotation/revocation.
- RBAC roles: firm admin, lawyer, paralegal, client. Policy-based checks on sensitive actions (issue invoice, file a case).

## Audit log

Separate from operational logs: append-only, tamper-evident (hash-chained), records case/document/invoice actions, auth events, permission changes, and AI actions (model, input/output refs, approver). Never contains raw secrets or unredacted privileged data. (Implemented in the Audit context.)

## Logging & redaction

Structured JSON logs carry correlation-id, tenant-id, user-id, action, outcome. **Secrets and raw PII are never logged** — masked at the sink. See [`docs/security/checklist.md`](docs/security/checklist.md).

## Transport & storage

TLS in transit everywhere; encryption at rest for DB, backups, and object storage. Security headers (HSTS, CSP, X-Content-Type-Options, etc.) and strict CORS allowlists at the API.

## Continuous security checks (gate every change)

- Dependency scan: `dotnet list package --vulnerable`, `npm audit` — fail on high/critical unless waived with a noted reason.
- Secret scan: gitleaks (pre-commit + CI).
- SAST: CodeQL / analyzers.
- Before any integration uses real credentials (ZATCA PCSID, Najiz prod, Nafath), sandbox/staging must pass.

## AI governance

AI output is assistive/draft until a qualified human approves it; never auto-persisted or presented as legal advice. Every AI action is logged and attributable, under the same classification, residency, and tenancy rules.

## Reporting a vulnerability

Email the maintainers privately; do not open a public issue. (Replace with the firm's security contact before pilot.)
