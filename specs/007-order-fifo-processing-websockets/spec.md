# Feature Specification: Asynchronous Order Processing with FIFO Queue and WebSockets

**Feature Branch**: `feature/007-order-fifo-processing-websockets`

**Created**: 2026-10-10

**Status**: Draft

**Source Architecture Decision**: [ADR-0007: Procesamiento de Pedidos con Cola FIFO y WebSockets](file:///r:/Programando/RestoCore/RestoCore-Docs/docs/adr/0007-procesamiento-pedidos-cola-fifo-websockets.md)

---

## 1. Context & Business Need

During rush hours (weekends, peak dining shifts), dining tables generate concurrent bursts of orders. Direct, synchronous processing against PostgreSQL creates:
1. **Race conditions** and duplicated commands under high table concurrency.
2. **Unfair ordering (lack of FIFO)**: Tables that ordered later could be processed earlier than previously placed orders.
3. **Kitchen latency & polling overhead**: Kitchen staff previously had to poll the REST API to discover new tickets.

Per **Backend Architectural Inviolable #6** and **ADR-0007**, all order placements must be ingested asynchronously with an immediate `HTTP 202 Accepted` response, appended to a sequential FIFO stream (Redis Streams) partitioned by tenant, consumed sequentially by a background worker, persisted in PostgreSQL with JSONB order item snapshots, and broadcasted in real time (< 100ms) via authenticated WebSockets (`wss://`) to Kitchen Display Systems (KDS).

---

## 2. User Scenarios & Testing *(mandatory)*

### User Story 1 - Client Order Ingestion (Priority: P1) 🎯 MVP

As a customer ordering from a dining table via QR, I want to submit my order and receive instant confirmation (`HTTP 202 Accepted`) with an `order_id` and tracking status, so that my order is registered immediately without network timeouts or race conditions.

**Why this priority**: Core ingestion path. If order submission fails or blocks, the business cannot operate.

**Independent Test**: Send a valid `POST /api/v1/orders` request with table token and items; verify `HTTP 202 Accepted` is returned in < 200ms with JSON response containing `order_id` and status `queued`.

**Acceptance Scenarios**:
1. **Given** a valid active tenant and table token, **When** submitting an order with selected menu items and modifiers, **Then** the API appends the payload to Redis Stream `orders:{tenant_slug}`, returns `HTTP 202 Accepted` with payload `{ "order_id": "...", "status": "queued" }`.
2. **Given** an invalid or inactive table token, **When** submitting an order, **Then** the API returns `HTTP 400 Bad Request` or `HTTP 404 Not Found` without enqueuing into Redis.

---

### User Story 2 - Sequential FIFO Background Processing (Priority: P2)

As a kitchen staff member, I need orders to be processed strictly in the order they were placed (First-In, First-Out), persisting the comanda snapshot into PostgreSQL atomically.

**Why this priority**: Eliminates order loss, double charging, and unfair customer service sequences during peak rushes.

**Independent Test**: Enqueue 5 orders simultaneously in Redis Streams; verify that the background worker reads them strictly in sequence, writes 5 immutable order records to PostgreSQL with status `received`, and records the execution timestamp.

**Acceptance Scenarios**:
1. **Given** queued orders in Redis Stream `orders:{tenant_slug}`, **When** the `OrderProcessorWorker` consumes the next stream message, **Then** it creates the `Order` entity in PostgreSQL with full item snapshot JSONB and acknowledges the stream message (`XACK`).
2. **Given** an item in the order that became unavailable or invalid, **When** processing, **Then** the worker marks the order with status `rejected` and emits a notification event.

---

### User Story 3 - Real-Time Kitchen Display Dispatch via WebSockets (Priority: P3)

As a cook or kitchen display operator, I want newly processed orders to appear immediately on my screen without manual refresh or HTTP polling.

**Why this priority**: Guarantees kitchen notification latency < 100ms to streamline meal preparation.

**Independent Test**: Establish an authenticated WebSocket connection to `/ws/kitchen?tenant={tenant_slug}`; trigger an order submission and verify that an `ORDER_CREATED` JSON event is received within 100ms of worker processing.

**Acceptance Scenarios**:
1. **Given** a connected kitchen WebSocket client with role `cook` or `owner`, **When** an order is persisted by the worker, **Then** the server broadcasts an `ORDER_CREATED` event containing order ID, table number, item details, and notes.
2. **Given** an unauthenticated WebSocket connection or invalid tenant token, **When** attempting to connect, **Then** the connection is rejected with status 4001/Unauthorized.

---

## 3. Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The API MUST expose endpoint `POST /api/v1/orders` accepting order items, quantities, and table identification, responding with `HTTP 202 Accepted` and enqueuing the message into Redis Streams.
- **FR-002**: Redis Stream keys MUST be segregated by tenant using pattern `orders:{tenant_slug}` or `orders:{tenant_id}` to preserve tenant data isolation.
- **FR-003**: The background worker MUST use Redis Stream Consumer Groups (`XREADGROUP`, `XACK`) to ensure strict FIFO ordering and at-least-once delivery with idempotency.
- **FR-004**: Orders MUST be persisted in PostgreSQL table `orders` with columns:
  - `id` (UUID, Primary Key)
  - `tenant_id` (UUID, Foreign Key)
  - `table_id` (UUID, Foreign Key)
  - `status` (Enum: `queued`, `received`, `preparing`, `ready`, `delivered`, `cancelled`)
  - `items_snapshot` (`JSONB`, immutable capture of item names, selected modifiers, and unit prices at moment of order)
  - `total_amount` (Decimal)
  - `created_at` (DateTimeOffset)
- **FR-005**: The system MUST provide a WebSocket hub/handler at `/ws/kitchen` requiring JWT authentication or tenant-scoped token with role verification (`cook`, `waiter`, `owner`).
- **FR-006**: Upon successful order persistence, the worker MUST publish an `ORDER_CREATED` event to connected kitchen WebSockets in < 100ms.

---

## 4. Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Ingestion endpoint `POST /api/v1/orders` latency p95 is under 150ms.
- **SC-002**: 100% of orders sent under concurrent load test (100 concurrent requests) are processed in strict FIFO order without dropped tickets or database deadlock exceptions.
- **SC-003**: End-to-end latency from order submission `POST /api/v1/orders` to WebSocket receipt on kitchen screen is under 500ms total (well below the 1000ms SLA).
- **SC-004**: Unit and integration test suites cover ingestion, FIFO stream worker, and WebSocket dispatch with >85% test coverage.

---

## 5. Assumptions & Constraints

- Redis is available and configured in `docker-compose.yml` (service `redis:7-alpine`).
- WebSockets utilize ASP.NET Core native WebSockets or SignalR with Redis backplane.
- OPA policies evaluate permissions for kitchen WebSocket connections.
- Aligns with ADR-0007 and ADR-0010 (immutable comanda snapshots).
