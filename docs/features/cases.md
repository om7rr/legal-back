# Feature: Cases (first domain)

**Bounded context:** Cases. First real domain — the spine that Sessions, Documents, Billing, and
Court Integrations later hang off.

## User stories
- **As a firm admin / lawyer**, I can **create a case** (number, title, type, court, client reference,
  responsible lawyer) so the firm has a system of record.
- **As a lawyer**, I can **list my firm's cases** and **open one** to see its details.
- **(Later)** As a client, I can view the status of *my* case in the portal (read-only).

## Acceptance criteria
- Creating a case persists it scoped to the caller's tenant and returns its id + case number.
- Listing returns only the caller tenant's cases (never another tenant's) — covered by a negative test.
- Fetching a non-existent / other-tenant case returns 404 (not another tenant's data).
- Case number is unique within a tenant.
- Every create is recorded in the Audit log (via an integration event), attributable to the actor.

## Data touched
| Entity | Classification | Notes |
|---|---|---|
| `Case` | Confidential | Case metadata; client identity lives in the Clients context (referenced by id). Uploaded documents (later) are `LegalPrivileged`. |

`Case` implements `ITenantOwned` → global `tenant_id` query filter + (later) Postgres RLS.

## Cross-domain events
- Raises **`CaseCreatedIntegrationEvent`** (caseId, tenantId, caseNumber, actorId, occurredAt).
- **Audit** consumes it and appends a hash-chained `AuditEntry`. No direct call from Cases → Audit.

## API surface (until Nafath: tenant via `X-Tenant-Id`, actor via `X-Actor-Id`)
| Method | Route | Body / returns |
|---|---|---|
| POST | `/api/cases` | `{ caseNumber, title, type, court, clientId, leadLawyerId }` → `201 { id, caseNumber }` |
| GET | `/api/cases` | → `200 [ { id, caseNumber, title, type, court, status } ]` (tenant-scoped) |
| GET | `/api/cases/{id}` | → `200 { …full case… }` or `404` |

Reflected in OpenAPI + the Postman `Cases` folder (incl. a tenant-isolation negative test).

## Out of scope (now)
Update/close transitions, documents, sessions, client-portal read, search/paging — follow-ups.
