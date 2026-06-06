# ADR-0002: Multi-tenancy via shared schema + row-level isolation

- **Status:** Proposed
- **Date:** 2026-06-06

## Context
All tenants (law firms) share infrastructure but must be strictly isolated. PDPL makes a tenant
data leak a serious compliance and legal event.

## Decision
- **Shared database, shared schema, `tenant_id` discriminator** on every tenant-owned entity
  (`ITenantOwned`).
- **Application layer:** EF Core global query filter on `tenant_id`, driven by a request-scoped
  `ITenantContext` set by `TenantResolverMiddleware`. Proven by `TenantIsolationTests`.
- **Defense-in-depth:** Postgres Row-Level Security policies (maintained in the `legal-db` repo at
  `sql/rls-policies.sql`). RLS is **documented now, enabled later** — it needs a per-connection tenant
  GUC (`app.current_tenant`) set on each request, wired alongside authentication.

## Consequences
- (+) Simple, cost-efficient, scales to many tenants; two independent isolation layers once RLS is on.
- (−) A shared schema means a bug in filter logic is high-impact — hence the mandatory negative tests
  and the planned RLS backstop.
- (−) Noisy-neighbor and per-tenant data-residency-by-region needs are not addressed here; revisit if
  a tenant requires physical isolation.

## Alternatives considered
- **Database-per-tenant** — strongest isolation, highest operational cost; reconsider for enterprise
  tenants with contractual isolation needs.
- **Schema-per-tenant** — middle ground; migration/management overhead grows with tenant count.
