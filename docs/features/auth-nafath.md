# Feature: Authentication (Nafath mock + JWT)

**Bounded context:** Identity & Access. See ADR-0006.

## User stories
- **As a lawyer/firm admin/client**, I authenticate with my national id via Nafath (mocked in the test
  environment) so I get a token carrying my verified identity, firm (tenant), and role.
- **As the system**, every protected endpoint trusts the JWT claims (tenant, role, actor) — never
  client-supplied headers.

## Flow (mock)
1. `POST /api/auth/nafath/initiate { nationalId }` → `{ transactionId, number }` (display `number`).
2. *(real Nafath: user taps the matching number in their app)* — test env:
   `POST /api/auth/nafath/_simulate-confirm { transactionId, accept }` **(non-Production only)**.
3. `GET /api/auth/nafath/status/{transactionId}` → `{ status }`, and when `Completed`,
   `{ accessToken, refreshToken, expiresIn }`.
4. `POST /api/auth/refresh { refreshToken }` → new `{ accessToken, refreshToken }`.

## Tokens
JWT (HMAC). Claims: `sub`, `tenant_id`, `role`, `name`, `national_id`. Access ~15 min; refresh longer.
Key from `Jwt:SigningKey` (Production: required; else dev fallback).

## Authorization
Bearer required on `/api/cases/*`. `TenantResolverMiddleware` sets the tenant from the `tenant_id`
claim. Unauthenticated → 401.

## Seeded test identities (test environment)
| National ID | Role | Firm (tenant) |
|---|---|---|
| 1111111111 | FirmAdmin | Firm A |
| 2222222222 | Lawyer | Firm A |
| 3333333333 | FirmAdmin | Firm B |

## Data touched
| Entity | Classification |
|---|---|
| `AppUser` (national id, name, tenant, role) | RegulatedPersonalData (national id is PII) |

## Out of scope (now)
Real Nafath integration, persisted/rotated refresh tokens, firm self-onboarding, client-portal nuances,
fine-grained policies beyond role. Follow-ups.
