# ADR-0005: Application/API conventions + cross-context eventing

- **Status:** Accepted
- **Date:** 2026-06-06

## Context
Implementing the first domain (Cases) forces conventions that will repeat in every module: how
endpoints are exposed, how the application layer is shaped, how a module reaches the database without
coupling to Infrastructure, and how cross-context events actually flow (ADR-0001 mandates events, but
the mechanism was unspecified). Decide once, apply everywhere.

## Decision
1. **API style — Minimal APIs grouped per module.** Each module exposes a
   `Map<Module>Endpoints(this IEndpointRouteBuilder)` extension under `/api`. No MVC controllers.
   Lighter, and keeps HTTP wiring beside the module.
2. **Application layer — lightweight handler classes** (one class per use case, e.g.
   `CreateCaseHandler`), injected into endpoints. **No MediatR** for the pilot (avoid the dependency
   and indirection); revisit if cross-cutting pipeline behaviors are needed.
3. **DB access via an abstraction in SharedKernel.** Add `IAppDbContext` (exposes `DbSet<T> Set<T>()`
   + `SaveChangesAsync`) in SharedKernel; `AppDbContext` implements it. Modules depend only on
   SharedKernel (not Infrastructure) — preserves the "no module → Infrastructure" boundary while
   giving modules typed data access. (No generic repository layer for now.)
4. **DTOs** are `record` request/response types per endpoint; mapping is explicit (no AutoMapper).
5. **Cross-context eventing — in-process dispatch now, transactional outbox next.** `IEventBus`
   (SharedKernel) gets an in-process implementation that dispatches an `IIntegrationEvent` to all
   registered `IIntegrationEventHandler<T>` synchronously, within the same request. Cases publishes
   `CaseCreatedIntegrationEvent`; an Audit handler appends an `AuditEntry`. **Reliability upgrade
   (deferred):** a DB **outbox** (event rows written in the same transaction as the state change, a
   background dispatcher publishing them) — tracked as a follow-up ADR before any event has external
   side effects (e.g. notifications).
6. **Integration-event contracts live in SharedKernel** (`IntegrationEvents/`), not inside a module —
   so the publisher and consumer each reference SharedKernel only and never each other (keeps the
   no-module-to-module rule). `IIntegrationEventHandler<T>` also lives in SharedKernel.

## Consequences
- (+) Minimal moving parts; consistent shape across modules; boundaries still enforced by arch tests.
- (+) Modules stay decoupled from Infrastructure via `IAppDbContext`.
- (−) In-process synchronous events are not crash-safe (an event lost if the process dies mid-handler).
  Acceptable while handlers are same-DB and idempotent; the outbox closes this before external effects.
- (−) Hand-mapping/validation is more boilerplate than MediatR + FluentValidation; deliberate trade
  for simplicity at pilot scale.

## Alternatives considered
- **MVC controllers** — heavier; rejected for the pilot.
- **MediatR/CQRS pipeline** — nice cross-cutting behaviors, but premature; revisit later.
- **Generic repository per module** — extra layer with little benefit over `IAppDbContext`; rejected.
- **Outbox from day one** — more robust but more infra now; deferred until an event has external
  side effects.
