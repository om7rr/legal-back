# legal-back — Legal Platform API

The backend for a production multi-tenant SaaS for Saudi law firms, with ZATCA (e-invoicing) and
Najiz (Ministry of Justice) integrations. ASP.NET Core (.NET 10) modular monolith. First deliverable:
a pilot-ready MVP for one firm, on multi-tenant-ready foundations.

> Part of a three-repo system (see `adr/0004`):
> **legal-back** (this repo, API) · **legal-front** (Next.js web) · **legal-db** (Postgres/Redis runtime, RLS, backups).
> The vanilla-HTML sales prototype lives separately (`legal-management-`).

## Repository layout

| Path | Purpose |
|---|---|
| `src/LegalPlatform.Api/` | ASP.NET host: middleware (authN/Z, rate-limit, tenant resolver), Swagger, `/health` |
| `src/LegalPlatform.SharedKernel/` | Result, events, data classification, tenancy abstractions |
| `src/LegalPlatform.Infrastructure/` | EF Core `AppDbContext`, Npgsql, migrations, Redis |
| `src/modules/` | 11 bounded contexts (no cross-references — enforced by arch tests) |
| `tests/` | API + architecture tests |
| `infra/` | Terraform IaC skeleton (provider-agnostic; apply is human-executed) |
| `postman/` | API collection + environments (placeholder vars only) |
| `adr/` · `docs/` | Architecture Decision Records · architecture/security/CI/DR docs |
| `.github/workflows/` | CI pipeline |

**Bounded contexts:** Identity & Access · Tenancy · Clients · Cases · Court Integrations (Najiz) ·
Documents · Billing · ZATCA · Notifications · Reporting · Audit.

## Prerequisites
- **.NET 10 SDK** (verified: `10.0.103`)
- **legal-db** running for Postgres + Redis (`docker compose up -d` in that repo)

## Getting started
```bash
# 1) Start the database (in the legal-db repo):
#    docker compose up -d
# 2) Then here:
dotnet tool restore
dotnet dotnet-ef database update \
  -p src/LegalPlatform.Infrastructure/LegalPlatform.Infrastructure.csproj \
  -s src/LegalPlatform.Api/LegalPlatform.Api.csproj
dotnet run --project src/LegalPlatform.Api    # Swagger at /swagger, health at /health
```
Set the DB connection string without committing it:
```bash
cd src/LegalPlatform.Api
dotnet user-secrets set "ConnectionStrings:Postgres" "Host=localhost;Port=5432;Database=legalplatform;Username=postgres;Password=postgres"
```

## Quality gates
`dotnet build` · `dotnet format --verify-no-changes` · `dotnet test` · dependency + secret scan (see
CI). No feature is done without tests, docs, and a green pipeline.

## Compliance
PDPL/SDAIA: all PII is KSA-resident. See `SECURITY.md` and `docs/security/`.
