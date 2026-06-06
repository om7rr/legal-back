# Threat note: Health endpoints

## Endpoints
- `GET /health` — liveness (no dependencies).
- `GET /health/ready` — readiness (Postgres + Redis).

## Exposure / who can call
Unauthenticated by design (used by load balancers, orchestrators, uptime monitors).

## Trust boundaries
Public → API. No tenant context required; no data access beyond a connectivity probe.

## Risks & mitigations
| Risk | Mitigation |
|---|---|
| Information disclosure (infra detail, stack traces) | Response is status + per-check name/status/description only. **No exception detail** (`HealthResponseWriter`). Connection strings never surfaced. |
| DoS via probe spam | Health is exempt from rate limiting for legitimate probes; protect at the edge/LB (allowlist prober IPs) in production. |
| Readiness reveals dependency topology | Check names are generic (`postgres`, `redis`); acceptable for ops. Restrict `/health/ready` at the edge to internal networks in production. |

## Follow-ups
- In production, scope `/health/ready` to internal/LB networks.
