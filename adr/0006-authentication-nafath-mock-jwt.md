# ADR-0006: Authentication — Nafath (mock test IdP) + JWT

- **Status:** Accepted
- **Date:** 2026-06-07

## Context
Cases (ADR-0005) shipped with tenant/actor taken from `X-Tenant-Id`/`X-Actor-Id` headers — an
unauthenticated demo trust gap. We need real verified identity. Production auth is **Nafath** (the
Saudi national SSO), but we can't reach real Nafath without an official agreement/credentials. We need
a working auth flow now for development and testing.

## Decision
- **Provider behind an interface.** `INafathClient` models the Nafath flow (initiate → number-match →
  poll status). A **`MockNafathClient`** simulates it in-process (no external calls); the real adapter
  implements the same interface later and is swapped by configuration. Chosen via `Nafath:Mode`
  (`Mock` for now).
- **Flow.** `initiate(nationalId)` returns a transaction id + a 2-digit number to display; the user
  "confirms" on their Nafath app (mocked by a **non-Production** `_simulate-confirm` endpoint);
  `status(txId)` returns Pending/Completed/Rejected, and on Completed issues tokens.
- **Tokens = JWT.** Short-lived access token + refresh token. Claims: `sub` (user id), `tenant_id`,
  `role`, `name`, `national_id`. Symmetric (HMAC) signing key from config; **Production requires the
  key from config/secrets** (startup fails otherwise) — a dev-only fallback key is used outside
  Production for the test environment.
- **Authorization.** API protected with JWT bearer + `[Authorize]`/`RequireAuthorization`. Roles:
  `FirmAdmin`, `Lawyer`, `Paralegal`, `Client`. **Tenant + actor now come from verified JWT claims**,
  not headers — `TenantResolverMiddleware` reads `tenant_id` from the authenticated principal.
- **Identity context owns** the `AppUser` (national id → user → tenant + role) and seeds a small set
  of **test firms/users** so the mock flow authenticates known identities.

## Consequences
- (+) Closes the header-trust gap; the rest of the system consumes verified claims.
- (+) Real adapter is a drop-in (same `INafathClient`, same JWT pipeline).
- (−) Mock confirmation + in-memory transaction/refresh store are **test-only**; production needs the
  real Nafath integration, persisted refresh tokens with rotation/revocation, and the production key.
- (−) A dev fallback signing key exists outside Production (documented, gated, gitleaks-allowlisted).

## Alternatives considered
- **Keep header-based tenant** — rejected: insecure, the gap we're closing.
- **Full OIDC server (Duende/Keycloak)** — overkill pre-pilot; Nafath is the real IdP anyway.
- **Wait for real Nafath access** — blocks all authenticated work; the mock unblocks development now.
