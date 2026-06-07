# Threat note: Authentication (Nafath mock + JWT)

## Endpoints (anonymous)
- `POST /api/auth/nafath/initiate`, `GET /api/auth/nafath/status/{id}`, `POST /api/auth/refresh`.
- `POST /api/auth/nafath/_simulate-confirm` — **mapped only outside Production** (test simulator).

## Trust boundaries
Public → API → identity store. Issues JWTs that the rest of the system trusts for tenant/role/actor.

## Risks & mitigations
| Risk | Mitigation |
|---|---|
| Forged tokens | HMAC-signed JWT; signature + issuer + audience + lifetime validated on every request. |
| Weak/leaked signing key | Production **requires** `Jwt:SigningKey` from secrets (startup fails otherwise); dev key used only outside Production and is gitleaks-allowlisted, never valid in prod. |
| Mock confirm abused | `_simulate-confirm` exists only outside Production; the real Nafath app confirmation replaces it. |
| Refresh token theft/replay | Single-use rotation on refresh (old token consumed). **Gap:** in-memory store, not persisted/revocable — production needs a persisted, revocable store. |
| Privilege escalation | Role + tenant come from signed claims, not client input. Authorize per endpoint (Cases requires auth). |
| National id (PII) exposure | Classified RegulatedPersonalData; never logged; KSA-resident. |

## Known gaps (pre-production)
- Replace `MockNafathClient` with the real Nafath integration (same `INafathClient`).
- Persisted + revocable refresh tokens; access-token lifetime tuning.
- Per-action authorization policies (e.g., only FirmAdmin issues invoices) as those features land.
