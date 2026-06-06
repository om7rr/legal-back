# CI/CD

Pipeline: `.github/workflows/ci.yml`. All stages gate merge to `main`.

| Stage | Job | What |
|---|---|---|
| 1 Build | `build-test` | `dotnet build -c Release` |
| 2 Lint/format | `build-test` | `dotnet format --verify-no-changes` |
| 3 Test | `build-test` | `dotnet test` with Postgres + Redis **service containers** |
| 4 Security | `build-test` + `secret-scan` | `dotnet list package --vulnerable` (fail on High/Critical) + gitleaks |
| 5 Contract | `contract-tests` | apply EF migrations, run API, run Postman collection via Newman |
| 6 Image | `container` | `docker build` (push needs registry creds — human-provisioned) |
| 7 Deploy | `deploy-staging` / `deploy-production` | staging on merge; production behind a manual approval gate |

## Run each stage locally
```bash
docker compose up -d
cd backend
dotnet format LegalPlatform.slnx --verify-no-changes
dotnet build  LegalPlatform.slnx -c Release
dotnet test   LegalPlatform.slnx
dotnet list   LegalPlatform.slnx package --vulnerable --include-transitive
# contract: dotnet dotnet-ef database update ... ; dotnet run ... ; newman run postman/...
```

## Grounding
Actual cloud deploy and secret injection **require credentials this pipeline does not hold**. Deploy
jobs are placeholders marked "requires human execution with credentials". Configure a required
reviewer on the GitHub `production` environment to make the approval gate real.
