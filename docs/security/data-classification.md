# Data Classification

Every persisted entity is tagged with `[DataClassification(...)]` (SharedKernel) and listed here.
Classification drives access control, logging/redaction, retention, and residency. **Code review
blocks any new entity missing a classification.**

| Entity | Context | Classification | Access | Logging | Retention | Residency |
|---|---|---|---|---|---|---|
| `Tenant` | Tenancy | Confidential | Firm admin (own), platform ops | Id + slug only; no contacts | Life of contract + legal retention | KSA |
| `AuditEntry` | Audit | Confidential | Read: firm admin, compliance; append-only | References only — never raw content/secrets | Long-term (legal defensibility) | KSA |
| `Case` | Cases | Confidential | Firm admin, lawyer (own tenant) | Case number/title OK; client identity is a reference (id) | Life of case + legal retention | KSA |

## Levels
- **Public** — freely shareable.
- **Internal** — operational, non-personal.
- **Confidential** — sensitive business data; role-restricted.
- **LegalPrivileged** — attorney-client privileged; strict access; **never leaves KSA**.
- **RegulatedPersonalData** — PDPL personal data; data-subject rights apply; **KSA-resident**.

## Rules
- `LegalPrivileged` and `RegulatedPersonalData` never transit a non-KSA service.
- Logs never contain raw values of `Confidential`+ fields — mask/redact (see checklist).
- Retention and erasure honored per `docs/security/dpia.md`.
