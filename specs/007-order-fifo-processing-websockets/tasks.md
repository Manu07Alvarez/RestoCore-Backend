# Tasks: Asynchronous Order Processing with FIFO Queue and WebSockets

**Input**: Design documents from `specs/007-order-fifo-processing-websockets/`
**Prerequisites**: [plan.md](file:///r:/Programando/RestoCore/resto-core-back/specs/007-order-fifo-processing-websockets/plan.md), [spec.md](file:///r:/Programando/RestoCore/resto-core-back/specs/007-order-fifo-processing-websockets/spec.md), [research.md](file:///r:/Programando/RestoCore/resto-core-back/specs/007-order-fifo-processing-websockets/research.md), [data-model.md](file:///r:/Programando/RestoCore/resto-core-back/specs/007-order-fifo-processing-websockets/data-model.md)

---

## Phase 1: Setup & Data Contracts

- [x] T001 [P] Create `Order` entity and snapshot models in `src/RestoCore.Domain/Entities/Order.cs`.
- [x] T002 [P] Configure EF Core mapping for `Order` with JSONB in `src/RestoCore.Infrastructure/Persistence/ApplicationDbContext.cs`.
- [x] T003 [P] Create `IOrderRepository` interface in `src/RestoCore.Application/Common/Interfaces/Repositories/IOrderRepository.cs`.
- [x] T004 [P] Implement `OrderRepository` in `src/RestoCore.Infrastructure/Persistence/Repositories/OrderRepository.cs`.

---

## Phase 2: User Story 1 - Client Order Ingestion (Priority: P1) 🎯 MVP

- [x] T005 [P] [US1] Create `IOrderQueueService` interface in `src/RestoCore.Application/Common/Interfaces/IOrderQueueService.cs`.
- [x] T006 [US1] Implement `RedisOrderQueueService` with Redis Streams `XADD` in `src/RestoCore.Infrastructure/Messaging/RedisOrderQueueService.cs`.
- [x] T007 [US1] Implement `SubmitOrderCommand` handler in `src/RestoCore.Application/Features/Orders/Commands/SubmitOrderCommand.cs`.
- [x] T008 [US1] Expose `POST /api/v1/orders` returning HTTP 202 in `src/RestoCore.Api/Endpoints/OrderEndpoints.cs`.
- [x] T009 [P] [US1] Create unit tests for order ingestion in `tests/RestoCore.UnitTests/Features/Orders/SubmitOrderCommandHandlerTests.cs`.

---

## Phase 3: User Story 2 - Sequential FIFO Background Worker (Priority: P2)

- [x] T010 [US2] Implement `OrderProcessorWorker` (ASP.NET Core BackgroundService) in `src/RestoCore.Infrastructure/BackgroundServices/OrderProcessorWorker.cs`.
- [x] T011 [US2] Add message acknowledgment (`XACK`) and error handling logic to `RedisOrderQueueService`.
- [x] T012 [P] [US2] Create unit tests for worker execution in `tests/RestoCore.UnitTests/Infrastructure/OrderProcessorWorkerTests.cs`.

---

## Phase 4: User Story 3 - Real-Time Kitchen Dispatch via WebSockets (Priority: P3)

- [x] T013 [P] [US3] Create `IKitchenNotifier` interface in `src/RestoCore.Application/Common/Interfaces/IKitchenNotifier.cs`.
- [x] T014 [US3] Implement `KitchenWebSocketManager` in `src/RestoCore.Infrastructure/WebSockets/KitchenWebSocketManager.cs`.
- [x] T015 [US3] Add WebSocket middleware/route at `/ws/kitchen` in `src/RestoCore.Api/Program.cs`.
- [x] T016 [US3] Connect `OrderProcessorWorker` to invoke `IKitchenNotifier.BroadcastOrderCreatedAsync`.

---

## Phase 5: Verification, Regressions & Graphify

- [x] T017 Run solution unit tests (`dotnet test tests/RestoCore.UnitTests`) and verify zero regressions.
- [x] T018 Run `graphify update .` to update the architectural knowledge graph.
