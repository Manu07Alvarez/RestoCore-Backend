# Implementation Plan: Asynchronous Order Processing with FIFO Queue and WebSockets

**Branch**: `feature/007-order-fifo-processing-websockets` | **Date**: 2026-10-10 | **Spec**: [specs/007-order-fifo-processing-websockets/spec.md](file:///r:/Programando/RestoCore/resto-core-back/specs/007-order-fifo-processing-websockets/spec.md)

**Input**: Feature specification from `specs/007-order-fifo-processing-websockets/spec.md`

---

## Summary

Implement the asynchronous order processing engine using Redis Streams for FIFO ordering, an ASP.NET Core BackgroundService (`OrderProcessorWorker`) for durable persistence to PostgreSQL with JSONB comanda snapshots, and an authenticated WebSockets endpoint (`/ws/kitchen`) for instantaneous (<100ms) notification dispatch to kitchen screens, fulfilling ADR-0007.

---

## Technical Context

**Language/Version**: C# 13 / .NET 10.0 (net10.0)

**Primary Dependencies**: StackExchange.Redis, Microsoft.AspNetCore.WebSockets, MediatR, FluentValidation, Npgsql.EntityFrameworkCore.PostgreSQL

**Storage & Messaging**:
- PostgreSQL: `orders` table with JSONB column `items_snapshot`
- Redis Streams: FIFO stream keys `orders:{tenant_slug}` with consumer groups

**Testing**: xUnit, Moq, FluentAssertions, WebApplicationFactory

**Constraints**: Ingestion latency p95 < 150ms; instant HTTP 202 Accepted; strict FIFO execution order.

---

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Principle I: Spec-Driven Development**: Adheres directly to ADR-0007 from `RestoCore-Docs`.
- **Principle II: Multi-Tenancy**: All stream keys and WebSocket connections strictly partitioned by `tenant_slug` or `tenant_id`.
- **Principle III: Clean Architecture & Domain Cohesion**: `IOrderQueueService`, `IKitchenNotifier`, and `IOrderRepository` defined in Application layer; implementations in Infrastructure.
- **Principle IV: Latency Budget & Asynchronous Decoupling**: API immediately returns HTTP 202 Accepted, completely decoupling ingestion from database disk writes.
- **Principle V: Relational & JSONB Discipline**: PostgreSQL holds persistent state; snapshots stored as JSONB.
- **Principle VI: Test-Driven Quality**: Unit and integration tests for queueing, worker, and WebSockets.

---

## Project Structure

```text
src/RestoCore.Domain/
└── Entities/
    └── Order.cs                     # Domain entity & snapshot value objects

src/RestoCore.Application/
├── Common/Interfaces/
│   ├── IOrderQueueService.cs        # Redis Streams contract
│   ├── IKitchenNotifier.cs          # Real-time WebSocket notifier
│   └── Repositories/
│       └── IOrderRepository.cs      # Order persistence contract
└── Features/Orders/
    └── Commands/
        └── SubmitOrderCommand.cs    # Ingestion handler (HTTP 202)

src/RestoCore.Infrastructure/
├── Messaging/
│   └── RedisOrderQueueService.cs    # Redis Streams implementation
├── WebSockets/
│   ├── KitchenWebSocketManager.cs   # In-memory tenant connection registry
│   └── KitchenWebSocketMiddleware.cs # Route /ws/kitchen
├── BackgroundServices/
│   └── OrderProcessorWorker.cs      # FIFO consumer worker
└── Persistence/
    └── Repositories/
        └── OrderRepository.cs       # EF Core order repository

src/RestoCore.Api/
└── Endpoints/
    └── OrderEndpoints.cs            # POST /api/v1/orders
```
