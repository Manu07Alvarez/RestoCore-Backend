<!--
Sync Impact Report
Version change: 0.0.0 -> 1.0.0
Modified principles: None (initial ratification)
Added sections:
- Core Principles (I. Spec-Driven Development, II. Multi-Tenancy & Declarative Authorization, III. Clean Architecture & Domain Cohesion, IV. Latency Budget & Asynchronous Decoupling, V. Relational & Flexible Persistence Discipline, VI. Test-Driven Quality & Reliability)
- Technical Constraints and Performance Standards
- Development Workflow and Quality Gates
- Governance
Removed sections: None
Templates requiring updates:
- .specify/templates/plan-template.md: Synchronized
- .specify/templates/spec-template.md: Synchronized
- .specify/templates/tasks-template.md: Synchronized
Follow-up TODOs: None
-->

# RestoCore Backend Constitution

## Core Principles

### I. Spec-Driven Development (Spec-First)
All backend APIs, data models, endpoints, status codes, and behavioral flows MUST strictly adhere to the contracts established in OpenAPI 3.1 and approved feature specifications before any production code is written. No undocumented endpoints, unapproved schemas, or deviation from approved specifications are permitted. Any contract change requires a prior specification amendment and review.

### II. Multi-Tenancy and Declarative Authorization
Strict data isolation across tenants is mandatory. Every database query, cache key, and event payload MUST include and enforce tenant identification (`tenant_id` or `tenant_slug`). Authorization decisions MUST NOT be hardcoded in application business logic; they MUST be evaluated declaratively against Open Policy Agent (OPA) / Rego policies, ensuring deterministic role verification across all actors (Anonymous Client, Waiter, Cook, Owner, SuperAdmin).

### III. Clean Architecture and Domain Cohesion
The codebase MUST enforce a clear separation of concerns across layers: Transport (HTTP handlers, middleware), Application / Domain Services (use cases, business rules), and Infrastructure / Data Access (PostgreSQL repositories, Redis clients, SeaweedFS integration). Domain models, naming conventions, and terminology MUST strictly align with the ubiquitous language defined in the RestoCore Domain Glossary.

### IV. Latency Budget and Asynchronous Decoupling
The backend MUST process dynamic API requests within a 500ms budget to guarantee the end-to-end target of less than 2 seconds Largest Contentful Paint (LCP) for public QR menu access. Public catalog endpoints MUST emit HTTP cache validation headers (`Cache-Control`, `ETag`). Heavy binary transfers (e.g. image uploads) MUST NOT traverse backend application workers; the API MUST generate time-limited pre-signed URLs directly for SeaweedFS. High-frequency operations like order creation MUST respond asynchronously with HTTP 202 Accepted and delegate sequential processing to FIFO queues (Redis Streams).

### V. Relational and Flexible Persistence Discipline
PostgreSQL MUST serve as the single source of truth for persistent application state. Semi-structured entity attributes (such as dynamic menu item customizations, variations, and flexible pricing structures) MUST leverage PostgreSQL `JSONB` columns indexed with `GIN` indexes to prevent schema sprawl while maintaining indexing performance.

### VI. Test-Driven Quality and Reliability (NON-NEGOTIABLE)
All new features, bug fixes, and refactoring efforts MUST follow a Test-Driven Development (TDD) discipline. Unit tests for domain logic and integration tests for HTTP routes, database repositories, and authorization policies MUST pass in continuous integration pipelines prior to branch merging. Flaky or slow tests MUST be treated as critical defects.

## Technical Constraints and Performance Standards

- API Specification: REST API complying with OpenAPI 3.1.0 standards.
- Database: PostgreSQL with JSONB columns and GIN indexing for flexible schemas.
- File and Object Storage: SeaweedFS accessed via backend-generated pre-signed URLs for decoupled binary upload.
- Security and Policy Engine: Open Policy Agent (OPA) with declarative Rego policies.
- Messaging and Queueing: Redis Streams for sequential FIFO order ingestion and WebSocket Secure (`wss://`) connections for kitchen and hall real-time dispatch.
- Documentation and Writing Style: Strictly professional tone. Emojis and informal expressions are prohibited in specifications, code comments, and technical records.

## Development Workflow and Quality Gates

- Specification Gate: Every feature MUST start with `/speckit-specify` producing an approved `spec.md`.
- Planning Gate: Architectural designs and research MUST pass `/speckit-plan` before task creation.
- Task Breakdown Gate: Implementation tasks MUST be generated via `/speckit-tasks` with dependency order and verification criteria.
- Pull Request Gate: All PRs MUST include comprehensive context descriptions, test evidence, and architectural rationale to serve as deliberation centers for code review.

## Governance

This Constitution supersedes all informal habits and individual developer preferences. Amendments to this document require:
1. Formal pull request with an explicit description and technical justification.
2. Semantic version increment:
   - MAJOR: Incompatible governance or principle deprecations.
   - MINOR: Additions of new principles or major architectural guidelines.
   - PATCH: Clarifications, wording improvements, or typo fixes.
3. Review and approval by project custodians.

**Version**: 1.0.0 | **Ratified**: 2026-09-03 | **Last Amended**: 2026-09-03
