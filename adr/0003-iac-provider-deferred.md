# ADR-0003: Defer cloud provider selection; Terraform skeleton now

- **Status:** Proposed
- **Date:** 2026-06-06

## Context
PDPL requires hosting in a **live, CST-licensed KSA region**. Candidate clouds (Azure, Google Cloud
Dammam, Oracle Jeddah, local providers) differ in region availability and licensing status, and that
landscape changes. Committing prematurely risks rework.

## Decision
Ship a **provider-agnostic Terraform skeleton** (`infra/`) with module placeholders (network,
database, cache, storage, secrets, observability) and **defer the provider choice**. Select the
provider in a follow-up ADR after confirming the region is live and CST-licensed for our data
classification, then add the provider block, backend, and module bodies.

## Consequences
- (+) Foundations proceed without lock-in; IaC structure is in place and reviewable.
- (−) No infrastructure can be applied until the provider ADR lands (acceptable — apply is
  human-executed and not part of the pilot foundations).

## Alternatives considered
- **Pick Azure now** (natural .NET fit) — deferred until KSA region/licensing is confirmed for our
  data classes.
- **No IaC yet** — rejected: we want the structure and review discipline from the start.
