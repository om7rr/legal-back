# Security checklist (ticked per module/endpoint)

Apply to every new endpoint/feature before it's "done".

## Per endpoint
- [ ] AuthN required (except explicitly public, e.g. health).
- [ ] AuthZ: role **and** tenant checked (default-deny).
- [ ] Tenant isolation: negative test (tenant A cannot read tenant B).
- [ ] Input validated/sanitized; parameterized queries only.
- [ ] No secrets/PII in logs (redacted); correlation id present.
- [ ] Threat note added under `docs/security/threat-notes/`.
- [ ] Entities touched are data-classified.
- [ ] OpenAPI + Postman updated.

## Per change (CI-gated)
- [ ] `dotnet format --verify-no-changes` clean.
- [ ] Unit + integration tests green.
- [ ] `dotnet list package --vulnerable` — no High/Critical.
- [ ] gitleaks — no findings.
- [ ] Security headers + CORS allowlist intact.

## Health endpoint (this session)
- [x] Public by design (liveness/readiness); not rate limited.
- [x] No secrets/PII in output (no exception detail leaked).
- [x] Security headers present (verified by test).
- [x] Threat note: `threat-notes/health.md`.
- [x] In Postman; covered by integration test.
