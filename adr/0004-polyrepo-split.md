# ADR-0004: Split into three repositories (back / front / db)

- **Status:** Accepted
- **Date:** 2026-06-06

## Context
The foundations were built as a single repo (`legal-platform`). We want clearer separation of
concerns and independent lifecycles (CI, versioning, deploy) for the API, the web front end, and the
database/runtime. Supersedes the single-repo layout implied by the first-session plan (the modular
monolith decision in ADR-0001 is unchanged).

## Decision
Three separate repositories:
- **legal-back** — the ASP.NET Core (.NET 10) modular monolith (API + 11 bounded contexts +
  Infrastructure incl. EF Core DbContext & migrations), tests, ADRs, docs, Postman, IaC, CI.
- **legal-front** — the Next.js + TypeScript web app (RTL, Cairo, ported design tokens).
- **legal-db** — database infrastructure & ops: docker-compose (Postgres + Redis), RLS policy SQL,
  init/bootstrap SQL, backup/restore scripts, DB docs.

Ownership boundary: **schema (DDL) is owned by EF Core migrations in legal-back**; legal-db owns how
the database *runs* and is *operated* (runtime, RLS enablement, backups, DR). The web app talks to the
API over HTTP only.

## Consequences
- (+) Independent CI/CD and access control per repo; smaller, focused checkouts.
- (+) Front/back teams work without cross-tree noise.
- (−) Cross-repo coordination for changes that span API + client (contract changes) — mitigated by
  the OpenAPI/Postman contract and versioning.
- (−) Local dev now spans three folders; documented in each README.

## Alternatives considered
- **Single repo, three top-level folders** — simpler git, but not independent lifecycles; rejected
  per the explicit goal of separated projects.
- **Database schema in legal-db** — rejected: splits the .NET solution across repos (painful
  cross-repo project references). Schema stays as code in legal-back (see ADR-0002).
