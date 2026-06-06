# Threat note: Cases endpoints

## Endpoints
- `POST /api/cases` — create a case.
- `GET /api/cases` — list the caller tenant's cases.
- `GET /api/cases/{id}` — fetch one case.

## Who can call / exposure
Authenticated firm users (firm admin, lawyer). **Until Nafath auth lands**, the tenant is taken from
the `X-Tenant-Id` header and the actor from `X-Actor-Id` — this is a **demo-only trust assumption**
and MUST be replaced by verified JWT/Nafath claims before any real data. Tracked as a follow-up.

## Trust boundaries
Client → API. Crosses into the Cases data store. Tenant scoping is the critical control.

## Risks & mitigations
| Risk | Mitigation |
|---|---|
| Cross-tenant read/write | EF global query filter on `tenant_id`; create stamps the resolved tenant; `GET /{id}` for another tenant returns 404. Negative tests in `CasesEndpointsTests`. RLS backstop pending (ADR-0002). |
| Spoofed tenant/actor headers | **Known gap (demo):** headers are unauthenticated. Replaced by verified Nafath/JWT claims before pilot data. |
| Duplicate / malformed input | Case number unique per tenant (DB index + handler check → 409); required-field validation (→ 400). |
| Info leak on error | Global handler returns sanitized ProblemDetails; exception detail only outside Production. |
| Mass data exfiltration via list | List is tenant-scoped; paging/limits to be added before large datasets. |

## Follow-ups
- Replace header-based tenant/actor with Nafath/JWT claims (auth milestone).
- Add paging + authorization policies (role checks) on list/create.
- Enable Postgres RLS (ADR-0002) once the per-connection tenant GUC is wired.
