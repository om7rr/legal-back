# Data Protection Impact Assessment (DPIA) — living document

Status: **stub**, kept current as data flows change (PDPL/SDAIA).

## Scope
Processing of law-firm and client personal data in the platform (cases, clients, documents, invoices).

## Data flows (to complete as features land)
- Identity: Nafath authentication of lawyers/clients → identity attributes.
- Clients/Cases: personal data of clients and counterparties.
- Documents: uploaded legal files (often `LegalPrivileged`).
- Billing/ZATCA: invoice data shared with ZATCA (lawful, regulatory).
- Court Integrations: case/hearing data exchanged with Najiz.

## PDPL obligations checklist
- [ ] Lawful basis documented per processing activity.
- [ ] Data-subject rights implemented (access, correction, erasure) with audit trail.
- [ ] Retention schedule per entity (see data-classification.md).
- [ ] KSA residency for all PII; no cross-border transfer without SDAIA-approved SCCs (+ ADR).
- [ ] Consent capture for the client portal.
- [ ] Breach-response process defined and rehearsed.
- [ ] Processor agreements for any third parties (hosting, notifications, payments).

## Review cadence
Re-assess on every change to data flows, and at minimum before the pilot go-live.
