# Architecture

See `adr/0001-foundational-architecture.md` for the authoritative decision. This is the working overview.

## Shape
Modular monolith (ASP.NET Core, .NET 10). Clients (Next.js web, React Native mobile) call the API.
Gateway concerns are middleware, not a separate process.

```
Clients ─▶ ASP.NET API
            │  middleware: exception → correlation-id → security-headers → request-log
            │              → CORS → rate-limiter → tenant-resolver
            ├─ 11 bounded contexts (separate projects, no cross-refs)
            │   Identity · Tenancy · Clients · Cases · CourtIntegrations · Documents
            │   Billing · ZATCA · Notifications · Reporting · Audit
            ├─ SharedKernel (Result, events, classification, tenancy abstractions)
            └─ Infrastructure (AppDbContext/Postgres, Redis, outbox)
```

## Key rules
- **No module references another module** — enforced by `LegalPlatform.Architecture.Tests`.
- **Cross-context only via integration events** (`IEventBus`, outbox-backed).
- **Every tenant-owned entity** implements `ITenantOwned`; the persistence layer applies a global
  `tenant_id` query filter (ADR-0002).
- **Every entity is data-classified** (`[DataClassification]`); see `docs/security/data-classification.md`.

## Persistence
One `AppDbContext` in Infrastructure discovers each context's `IEntityTypeConfiguration` by assembly
scan (assemblies supplied by the API composition root) — Infrastructure references no specific module.
Migrations live in `Infrastructure/Persistence/Migrations`.

## Project layout
`src/{LegalPlatform.Api, LegalPlatform.SharedKernel, LegalPlatform.Infrastructure, modules/*}`,
`tests/{LegalPlatform.Api.Tests, LegalPlatform.Architecture.Tests}`.
(This is the `legal-back` repo; web is `legal-front`, DB runtime is `legal-db` — see `adr/0004`.)
