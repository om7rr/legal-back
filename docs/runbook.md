# Operations Runbook

## Local development
```bash
docker compose up -d                       # Postgres + Redis
cd backend
dotnet tool restore
dotnet dotnet-ef database update \
  -p src/LegalPlatform.Infrastructure/LegalPlatform.Infrastructure.csproj \
  -s src/LegalPlatform.Api/LegalPlatform.Api.csproj
dotnet run --project src/LegalPlatform.Api    # /swagger, /health, /health/ready
```
Set the DB connection string locally without committing it:
```bash
cd backend/src/LegalPlatform.Api
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:Postgres" "Host=localhost;Port=5432;Database=legalplatform;Username=postgres;Password=postgres"
```

## Health
- `GET /health` — liveness (no dependencies).
- `GET /health/ready` — readiness (Postgres + Redis). `200` Healthy / `503` Unhealthy with per-check detail.

## Migrations
- Add: `dotnet dotnet-ef migrations add <Name> -p ...Infrastructure -s ...Api -o Persistence/Migrations`
- Apply: `dotnet dotnet-ef database update -p ...Infrastructure -s ...Api`

## Restore drill (placeholder — flesh out with provider, ADR-0003)
1. Provision a scratch DB. 2. Restore latest PITR snapshot. 3. Point a staging API at it.
4. Verify health + key reads. 5. Record RPO/RTO achieved vs targets in `docs/dr.md`.
