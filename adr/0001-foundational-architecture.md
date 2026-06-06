# ADR-0001: Foundational architecture — modular monolith with bounded contexts

- **Status:** Accepted (on approval of the first-session foundations plan)
- **Date:** 2026-06-06

## Context
We are building a multi-tenant SaaS legal platform, pilot-first but scalable. We need clean domain
boundaries that can split into services later, without paying microservice tax during the pilot.

## Decision
- **Modular monolith** in ASP.NET Core (.NET 10): one deployable, 11 bounded contexts as separate
  projects — Identity & Access, Tenancy, Clients, Cases, Court Integrations (Najiz), Documents,
  Billing, ZATCA, Notifications, Reporting, Audit.
- **No module references another module.** Enforced by `LegalPlatform.Architecture.Tests`.
- **Cross-context communication via integration events only** (`IEventBus`, outbox-backed). No
  reaching into another context's tables.
- **Gateway concerns as ASP.NET middleware** (authN/Z, rate limiting, tenant resolution, correlation
  id, security headers) — **not** a separate gateway process for the pilot.
- **Shared `AppDbContext`** in Infrastructure that discovers each context's EF configurations by
  assembly scan, so Infrastructure stays decoupled from individual modules.

## Consequences
- (+) Fast to build and operate now; cheap to extract services later (events already in place).
- (+) Boundary violations fail the build.
- (−) A shared database/context means discipline (not infrastructure) prevents cross-context table
  access; mitigated by the arch tests and code review. Revisit if a context needs its own datastore.

## Alternatives considered
- **Microservices from day one** — rejected: operational overhead unjustified pre-pilot.
- **Separate API gateway process (YARP/Ocelot)** — rejected for the pilot: middleware covers the
  same concerns with less moving infrastructure. Revisit at multi-service scale.
- **Database-per-module** — deferred: more isolation but heavier; the shared context + arch tests are
  sufficient for the pilot.
