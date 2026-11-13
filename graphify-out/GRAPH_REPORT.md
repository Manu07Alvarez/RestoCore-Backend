# Graph Report - resto-core-back  (2026-11-12)

## Corpus Check
- 322 files · ~126,857 words
- Verdict: corpus is large enough that graph structure adds value.
- Unclassified: 4 file(s) not represented in the graph (top: (none) 2, .example 1, .rego 1)

## Summary
- 2096 nodes · 5544 edges · 133 communities (114 shown, 19 thin omitted)
- Extraction: 81% EXTRACTED · 19% INFERRED · 0% AMBIGUOUS · INFERRED: 1072 edges (avg confidence: 0.9)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `c854b620`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- PublicMenuItemDto
- Structured Logging & Traces Specification
- Tenant Entity
- ContainerizedStackFixture
- Order
- Feature Specification: Asynchronous Order Processing with FIFO Queue and WebSockets
- RestoCore Backend Constitution
- TenantResolutionMiddleware
- OpaAuthorizationHandler
- Program.cs
- API Contracts: Menu Item CRUD, Soft Delete with 7-Day Retention, and Restore
- microsoft_extensions_dependencyinjection
- system
- IApplicationDbContext
- Feature Specification: Category ABMC & Restaurant Contact Profile Management
- CategoryResponse
- ModifierGroup
- common.ps1
- http
- MenuPublishJobResponse
- Data Model: Menu Item CRUD, Soft Delete with 7-Day Retention, and Purge Worker
- 1. Category ABMC Endpoints (`Admin - Catalog`)
- RestoCore.Infrastructure.csproj
- app.py
- Table
- LayoutConfigDto
- MenuItem
- RestoCore.Application.Common.Interfaces.Repositories
- 1. Mapa de Fuentes en `RestoCore-Docs`
- RestoCore.Application.Common.Interfaces
- Functional Requirements
- Guid
- .ProcessOrderMessageAsync
- Tenant
- UserRepository
- .ConfigureWebHost
- RestoCore.IntegrationTests.csproj
- EntityTypeBuilder
- Task
- CanvasElementDto
- RestoCore.Application.csproj
- AuthEndpoints.cs
- IQrCodeService
- RestoCore.SecurityTests.csproj
- RestoCore.UnitTests.csproj
- ref_k6
- Category
- .MapAdminCatalogEndpoints
- create-new-feature.ps1
- PublicMenuHybridCacheService
- ValidationBehavior
- RestoCore.Api.csproj
- IKitchenRepository
- .Handle
- MenuPublishJob
- RestoreMenuItemCommand
- Speckit SDD Workflow
- Observability & Tracing Research
- RestoCore.Application.Features.Analytics.DTOs
- 2. Flujo de Ejecucion
- Skill: Spec-Kit Checklist
- Skill: Spec-Kit Git Feature
- Admin App Streamlit Stack
- Skill: Spec-Kit Git Initialize
- Vector and Raster QR Generation
- 2. CQRS Commands, Queries & DTOs
- Fact
- ApplicationDbContext
- BrandingConfig
- ScopeCapturingLogger
- 1. Resolved Clarifications & Key Technical Decisions
- UnitOfWork
- .Mediator_Send_ShouldThrowValidationExceptionWhenValidationBehaviorFails
- DailyMenuTrafficSummary
- PublicMenuResponse
- RestoCore.Domain.Entities
- User
- CreateMenuItemCommand
- .Send
- 2. Research Decisions
- Implementation Plan: Repositories, Unit of Work, and Concurrency Control
- Tasks: Optimizaciones Arquitectónicas de Persistencia JSONB/GIN, Caché Híbrido L1/L2, Mediador CQRS, Gobernanza OpenTelemetry/OPA y Ciclo de Vida de Streams/WebSockets
- IEndpointRouteBuilder
- ITenantContext
- .Handle
- IOrderQueueService
- DateTimeOffset
- Quickstart & Verification Guide: Category ABMC & Restaurant Profile
- Feature Specification: Menu Analytics, Interaction Telemetry and Menu Engineering Dashboard
- ICatalogRepository
- GetTableQrQueryHandler
- IPasswordHasher
- .Handle
- TenantProfileResponse
- OrderItemSnapshot
- Tasks: Menu Analytics, Interaction Telemetry and Menu Engineering Dashboard
- MenuCompilationResult
- .OrderProcessorWorker_ProcessOrderMessageAsync_ShouldBatchLoadMenuItemsInSingleQuery
- CollectAnalyticsEventCommandHandler
- LoginCommandHandler
- Functional Requirements
- User Scenarios & Testing *(mandatory)*
- 1. REST Endpoint: `POST /api/v1/auth/login`
- 2. Contracts & Services
- PurgeArchivedDishesWorker
- AnalyticsAggregationWorker
- AnalyticsEventMessage
- Data Model: PostgreSQL User Authentication & On-Demand Database Seeding
- Architectural Research: PostgreSQL User Authentication & On-Demand Database Seeding
- ICdnPurgeService
- Implementation Plan: Asynchronous Order Processing with FIFO Queue and WebSockets
- Phase 1 Data Model: Optimizaciones Arquitectónicas del Backend (`012-backend-architecture-optimizations`)
- 2. `GET /api/v1/admin/analytics/dashboard`
- Implementation Plan: Optimizaciones Arquitectónicas de Persistencia JSONB/GIN, Caché Híbrido L1/L2, Mediador CQRS, Gobernanza OpenTelemetry/OPA y Ciclo de Vida de Streams/WebSockets
- .SeedAsync
- Quickstart & Verification Guide: PostgreSQL User Authentication & On-Demand Database Seeding
- CapturingHttpMessageHandler
- Quickstart: Menu Analytics, Interaction Telemetry and Menu Engineering Dashboard
- OpenTelemetryExtensions
- IRequest
- Implementation Plan: Menu Analytics, Interaction Telemetry and Menu Engineering Dashboard
- AnalyticsAggregationWorker.cs
- Quickstart: Verificación de Optimizaciones Arquitectónicas (`012-backend-architecture-optimizations`)
- Phase 1 Data Model: Menu Analytics, Interaction Telemetry and Menu Engineering Dashboard
- Phase 0 Research: Menu Analytics, Interaction Telemetry and Menu Engineering Dashboard

## God Nodes (most connected - your core abstractions)
1. `MenuItem` - 88 edges
2. `RestoCore.Application.Common.Interfaces` - 79 edges
3. `Tenant` - 77 edges
4. `RestoCore.Domain.Entities` - 72 edges
5. `ICatalogRepository` - 66 edges
6. `ApplicationDbContext` - 65 edges
7. `RestoCore.Application.Common.Interfaces.Repositories` - 63 edges
8. `ITenantContext` - 51 edges
9. `IUnitOfWork` - 51 edges
10. `RestoCore.Application.Common.Mediator` - 51 edges

## Surprising Connections (you probably didn't know these)
- `1. Asynchronous Telemetry Ingestion Mechanism (`< 50ms p95`)` --references--> `IAnalyticsEventQueue`  [INFERRED]
  specs/011-menu-analytics-and-telemetry/research.md → src/RestoCore.Application/Common/Interfaces/IAnalyticsEventQueue.cs
- `Phase 5: Verification, Regressions & Graphify Re-evaluation` --references--> `IApplicationDbContext`  [INFERRED]
  specs/006-repository-unitofwork-concurrency/tasks.md → src/RestoCore.Application/Common/Interfaces/IApplicationDbContext.cs
- `Decision` --references--> `ICdnPurgeService`  [INFERRED]
  specs/009-category-abmc-restaurant-profile/research.md → src/RestoCore.Application/Common/Interfaces/ICdnPurgeService.cs
- `1. Context & Business Need` --references--> `ICdnPurgeService`  [INFERRED]
  specs/009-category-abmc-restaurant-profile/spec.md → src/RestoCore.Application/Common/Interfaces/ICdnPurgeService.cs
- `2.2 `IKitchenNotifier` (WebSocket Connection Registry)` --references--> `IKitchenNotifier`  [INFERRED]
  specs/007-order-fifo-processing-websockets/data-model.md → src/RestoCore.Application/Common/Interfaces/IKitchenNotifier.cs

## Import Cycles
- None detected.

## Hyperedges (group relationships)
- **Decoupled Canvas Layout AOT Publishing Pipeline** — specs_005_canvas_layout_aot_compilation_data_model_layout_config, specs_005_canvas_layout_aot_compilation_data_model_menu_publish_job, specs_005_canvas_layout_aot_compilation_research_jsonb_storage, specs_005_canvas_layout_aot_compilation_research_aot_compilation, specs_005_canvas_layout_aot_compilation_contracts_openapi_canvas_publish_endpoint [EXTRACTED 1.00]
- **Load Testing and Real-Time Telemetry Feedback Loop** — specs_003_k6_aspire_swagger_spec_us2_aspire_telemetry, specs_003_k6_aspire_swagger_spec_us3_k6_stress_tests, specs_003_k6_aspire_swagger_research_aspire_dashboard, specs_003_k6_aspire_swagger_research_k6_testing [EXTRACTED 1.00]
- **OpenTelemetry Logging and Trace Correlation Framework** — specs_004_structured_logging_traces_data_model_telemetry_event, specs_004_structured_logging_traces_data_model_span_attributes, specs_004_structured_logging_traces_plan_anti_redundancy, specs_004_structured_logging_traces_plan_w3c_propagation, specs_004_structured_logging_traces_contracts_telemetry_contract_otlp_exporter [EXTRACTED 1.00]
- **Multi-Tenant Catalog Data Model Structure** — specs_001_multi_tenant_digital_menu_data_model_tenant, specs_001_multi_tenant_digital_menu_data_model_category, specs_001_multi_tenant_digital_menu_data_model_menuitem, specs_001_multi_tenant_digital_menu_data_model_table [EXTRACTED 1.00]
- **Spec-Driven Development Artifacts Pipeline** — specify_workflows_speckit_workflow_speckit, specify_templates_spec_template_structure, specify_templates_plan_template_structure, specify_templates_tasks_template_structure [EXTRACTED 1.00]
- **** — agents_skills_speckit_git_initialize, agents_skills_speckit_git_feature, agents_skills_speckit_git_validate, agents_skills_speckit_git_commit, agents_skills_speckit_git_remote [INFERRED]
- **** — agents_spec_driven_development, specify_memory_constitution, agents_skills_speckit_specify, agents_skills_speckit_plan, agents_skills_speckit_tasks, agents_skills_speckit_implement [INFERRED]

## Communities (133 total, 19 thin omitted)

### Community 0 - "PublicMenuItemDto"
Cohesion: 0.08
Nodes (27): PublicCategoryDto, Description, Id, Items, Name, PublicMenuItemDto, Allergens, BasePrice (+19 more)

### Community 1 - "Structured Logging & Traces Specification"
Cohesion: 0.05
Nodes (42): k6 Aspire Swagger Requirements Checklist, Observability and Swagger API Contract, Swagger OpenAPI Explorer Endpoints, k6 & Aspire Quickstart Guide, k6 Performance Testing Scenarios, k6 Aspire Swagger Specification, US1: Interactive API Exploration, US2: Real-time Telemetry Dashboard (+34 more)

### Community 2 - "Tenant Entity"
Cohesion: 0.08
Nodes (24): Digital Menu Quality Checklist, Digital Menu OpenAPI Specification, Category Entity, MenuItem Entity, ModifierGroup Entity, ModifierOption Entity, Table Entity, Tenant Entity (+16 more)

### Community 3 - "ContainerizedStackFixture"
Cohesion: 0.12
Nodes (10): PublicMenuEndpoints, Program, AdminMenuLayoutEndpointsTests, AnalyticsEndpointsTests, FullLifecycleE2ETests, MenuPublishEndpointsTests, PublicMenuCanvasEndpointsTests, ContainerizedStackFixture (+2 more)

### Community 4 - "Order"
Cohesion: 0.13
Nodes (17): Phase 1: Setup & Data Contracts, IOrderRepository, Order, CompletedAt, CreatedAt, CustomerNotes, Id, ItemsSnapshot (+9 more)

### Community 5 - "Feature Specification: Asynchronous Order Processing with FIFO Queue and WebSockets"
Cohesion: 0.17
Nodes (11): 1. Context & Business Need, 2. User Scenarios & Testing *(mandatory)*, 3. Requirements *(mandatory)*, 4. Success Criteria *(mandatory)*, 5. Assumptions & Constraints, Feature Specification: Asynchronous Order Processing with FIFO Queue and WebSockets, Functional Requirements, Measurable Outcomes (+3 more)

### Community 6 - "RestoCore Backend Constitution"
Cohesion: 0.09
Nodes (20): AI Agents Operating Guidelines, Skill: Git Commit, Skill: Review TODOs, Skill: Spec-Kit Analyze, Skill: Spec-Kit Clarify, Skill: Spec-Kit Constitution, Skill: Spec-Kit Git Commit, Skill: Spec-Kit Git Remote (+12 more)

### Community 7 - "TenantResolutionMiddleware"
Cohesion: 0.21
Nodes (6): TenantContext, TenantId, TenantSlug, TenantResolutionMiddleware, CrossTenantDatabaseIsolationTests, TenantResolutionTests

### Community 8 - "OpaAuthorizationHandler"
Cohesion: 0.10
Nodes (15): OpaAuthorizationHandler, OpaRequirement, Action, ResourceType, IOpaClient, OpaInput, Action, Method (+7 more)

### Community 9 - "Program.cs"
Cohesion: 0.07
Nodes (10): RestoCore.Application.Common.Caching, RestoCore.Api.Endpoints, RestoCore.Infrastructure.Qr, RestoCore.Infrastructure.Caching, RestoCore.Infrastructure.Storage, RestoCore.Api.Middlewares, RestoCore.Infrastructure.Cdn, RestoCore.UnitTests.MultiTenancy (+2 more)

### Community 10 - "API Contracts: Menu Item CRUD, Soft Delete with 7-Day Retention, and Restore"
Cohesion: 0.14
Nodes (13): 1. Full Modification of Menu Item (`PUT /api/v1/admin/items/{id}`), 2. Partial Modification of Menu Item (`PATCH /api/v1/admin/items/{id}`), 3. Soft Delete Menu Item (`DELETE /api/v1/admin/items/{id}`), 4. Restore Menu Item (`POST /api/v1/admin/items/{id}/restore`), API Contracts: Menu Item CRUD, Soft Delete with 7-Day Retention, and Restore, Request, Request, Request (+5 more)

### Community 11 - "microsoft_extensions_dependencyinjection"
Cohesion: 0.14
Nodes (7): RestoCore.Infrastructure.Identity, RestoCore.Application.Features.PublicMenu.DTOs, RestoCore.IntegrationTests.Endpoints, RestoCore.UnitTests.PublicMenu, RestoCore.IntegrationTests.Fixtures, RestoCore.IntegrationTests.Storage, RestoCore.Infrastructure.Persistence

### Community 12 - "system"
Cohesion: 0.11
Nodes (11): RestoCore.Application.Common.Behaviors, RestoCore.UnitTests.Infrastructure, RestoCore.UnitTests.Features.MenuLayout, RestoCore.Application.Features.MenuLayout.Commands, RestoCore.UnitTests.Features.Orders, RestoCore.Infrastructure.Services.Compilation, RestoCore.Application.Features.MenuLayout.Services, RestoCore.Infrastructure.WebSockets (+3 more)

### Community 13 - "IApplicationDbContext"
Cohesion: 0.09
Nodes (22): 1. Dependency Injection in Command Handlers, 2. Using Explicit Transactions, 3. High-Concurrency Item Toggling, Developer Quickstart: Using Repositories & Unit of Work in RestoCore, Background & Architectural Drivers, Feature Specification: Repositories and Unit of Work for Concurrency and Persistence Decoupling, Measurable Outcomes, Requirements *(mandatory)* (+14 more)

### Community 14 - "Feature Specification: Category ABMC & Restaurant Contact Profile Management"
Cohesion: 0.18
Nodes (10): 1. Context & Business Need, 2. User Scenarios & Testing *(mandatory)*, 3. Requirements *(mandatory)*, 4. Success Criteria *(mandatory)*, Feature Specification: Category ABMC & Restaurant Contact Profile Management, Functional Requirements, Key Entities, User Story 1 - Full Category ABMC with Referential Integrity Guardrail (Priority: P1) 🎯 MVP (+2 more)

### Community 15 - "CategoryResponse"
Cohesion: 0.19
Nodes (15): 2.1 Category ABMC Operations, Category Response DTO:, Commands:, Queries:, Implementation for User Story 1, Tests for User Story 1, CategoryResponse, PatchCategoryCommand (+7 more)

### Community 16 - "ModifierGroup"
Cohesion: 0.16
Nodes (14): AddModifierGroupCommand, AddModifierGroupCommandHandler, ModifierGroupResponse, ModifierOptionInput, ModifierOptionResponse, ModifierGroup, IsRequired, MaxSelection (+6 more)

### Community 17 - "common.ps1"
Cohesion: 0.23
Nodes (11): Find-FeatureDirByPrefix(), Find-SpecifyRoot(), Get-CurrentBranch(), Get-FeatureDirFromBranchPrefixOrExit(), Get-FeaturePathsEnv(), Get-Python3Command(), Get-RepoRoot(), Get-SpecKitEffectiveBranchName() (+3 more)

### Community 18 - "http"
Cohesion: 0.13
Nodes (15): ASPNETCORE_ENVIRONMENT, applicationUrl, commandName, dotnetRunMessages, environmentVariables, launchBrowser, applicationUrl, commandName (+7 more)

### Community 19 - "MenuPublishJobResponse"
Cohesion: 0.15
Nodes (13): AdminMenuLayoutEndpoints, PublishMenuCommand, MenuPublishJobResponse, AssetsQueued, CdnPurgeRequested, EstimatedDurationSeconds, JobId, Status (+5 more)

### Community 20 - "Data Model: Menu Item CRUD, Soft Delete with 7-Day Retention, and Purge Worker"
Cohesion: 0.29
Nodes (6): 1. Domain Entities & Value Objects, 2. PostgreSQL DDL Mapping & Indexes, 3. State Lifecycle & Transitions, Data Model: Menu Item CRUD, Soft Delete with 7-Day Retention, and Purge Worker, `MenuItem` (`RestoCore.Domain.Entities`), Transition Invariants

### Community 21 - "1. Category ABMC Endpoints (`Admin - Catalog`)"
Cohesion: 0.12
Nodes (16): 1.1 List Categories, 1.2 Get Category by ID, 1.3 Create Category, 1.4 Full Update Category (PUT), 1.5 Partial Update Category (PATCH), 1.6 Delete Category (DELETE), 1. Category ABMC Endpoints (`Admin - Catalog`), 2.1 Get Restaurant Profile (+8 more)

### Community 22 - "RestoCore.Infrastructure.csproj"
Cohesion: 0.13
Nodes (14): AWSSDK.S3 (4.0.102.5), Microsoft.Extensions.Caching.StackExchangeRedis (10.0.12), Npgsql.EntityFrameworkCore.PostgreSQL (10.0.3), Npgsql.OpenTelemetry (10.0.3), OpenTelemetry.Exporter.OpenTelemetryProtocol (1.18.0), OpenTelemetry.Extensions.Hosting (1.18.0), OpenTelemetry.Instrumentation.AspNetCore (1.18.0), OpenTelemetry.Instrumentation.Http (1.18.0) (+6 more)

### Community 24 - "Table"
Cohesion: 0.15
Nodes (8): Table, IsActive, Label, TableNumber, Tenant, TenantId, Token, TenantRepository

### Community 25 - "LayoutConfigDto"
Cohesion: 0.17
Nodes (11): UpdateMenuLayoutCommand, UpdateMenuLayoutCommandHandler, LayoutConfigDto, BackgroundColor, BackgroundUrl, CanvasEnabled, Elements, IMenuCompilationService (+3 more)

### Community 26 - "MenuItem"
Cohesion: 0.07
Nodes (20): Phase 3: User Story 1 - Caché Híbrido Multinivel L1/L2 y Filtrado Estricto de Baja Suave en Carta QR (Priority: P1) 🎯 MVP, Tests for User Story 1, MenuItem, Allergens, BasePrice, Category, CategoryId, DeletedAt (+12 more)

### Community 27 - "RestoCore.Application.Common.Interfaces.Repositories"
Cohesion: 0.12
Nodes (14): RestoCore.UnitTests.Features.Categories, RestoCore.UnitTests.Application, RestoCore.UnitTests.Features.Kitchen, RestoCore.IntegrationTests.Infrastructure, RestoCore.UnitTests.Domain, RestoCore.Application.Features.PublicMenu.Queries, RestoCore.Application.Common.Interfaces.Repositories, RestoCore.UnitTests.Features.PublicMenu (+6 more)

### Community 28 - "1. Mapa de Fuentes en `RestoCore-Docs`"
Cohesion: 0.22
Nodes (8): 1. Mapa de Fuentes en `RestoCore-Docs`, 2. Flujo de Ejecución Paso a Paso, 3. Ejemplo Práctico de Resolución, Consulta de Documentación Arquitectónica (`consult-docs`), Paso 1: Búsqueda Rápida de Referencias, Paso 2: Lectura Dirigida del Documento, Paso 3: Consulta en el Grafo de Graphify (Opcional), Paso 4: Aplicación y Citado de la Fuente

### Community 29 - "RestoCore.Application.Common.Interfaces"
Cohesion: 0.13
Nodes (11): RestoCore.Application.Features.MenuItems.Queries, RestoCore.Domain.ValueObjects, RestoCore.Application.Common.Interfaces, RestoCore.Application.Features.Tenants.Queries, RestoCore.Application.Common.Mediator, RestoCore.Application.Features.Tenants.Commands, RestoCore.Application.Features.MenuItems.DTOs, RestoCore.Application.Features.MenuItems.Commands (+3 more)

### Community 30 - "Functional Requirements"
Cohesion: 0.15
Nodes (22): 1. Contrato de Caché Híbrido para Carta QR (`GET /api/v1/tenants/{tenant_slug}/menu`), 2. Contrato de Respuesta de Error Estándar RFC 7807 (`application/problem+json`), 3. Contrato de Evaluación OPA (`POST /v1/data/restocore/authz/allow`), Phase 1 Contracts: Optimizaciones Arquitectónicas del Backend (`012-backend-architecture-optimizations`), Summary, 3. Verificaciones Específicas por ADR, 5. Gobernanza OpenTelemetry, RFC 7807, SeaweedFS SSL y OPA RBAC (`ADR-0017` / `OPT-07`, `OPT-09`, `OPT-10`), Functional Requirements (+14 more)

### Community 31 - "Guid"
Cohesion: 0.19
Nodes (3): Phase 2: Foundational (Blocking Prerequisites), IAnalyticsRepository, AnalyticsRepository

### Community 32 - ".ProcessOrderMessageAsync"
Cohesion: 0.22
Nodes (10): Phase 4: User Story 3 - Real-Time Kitchen Dispatch via WebSockets (Priority: P3), Measurable Outcomes, User Story 4 - Procesamiento Reactivo de Redis Streams, Carga SQL por Lote (`ANY`) y Ciclo de Vida Determinista de WebSockets y Redis (Priority: P2), Implementation for User Story 4, Phase 6: User Story 4 - Procesamiento Reactivo de Redis Streams, Carga SQL por Lote (`ANY`) y Ciclo de Vida Determinista de WebSockets y Redis (Priority: P2), Tests for User Story 4, IKitchenNotifier, OrderCreatedNotification (+2 more)

### Community 33 - "Tenant"
Cohesion: 0.07
Nodes (25): 1. Summary, 2. Technical Context, 3. Constitution Check, 4. Project Structure, 5. Complexity Tracking, Documentation (this feature), Implementation Plan: Category ABMC & Restaurant Contact Profile Management, Source Code Changes Planned (+17 more)

### Community 34 - "UserRepository"
Cohesion: 0.19
Nodes (10): Implementation for User Story 2, Phase 1: Setup & Domain Entity, Phase 2: Foundational Infrastructure, Phase 3: User Story 1 - PostgreSQL-Backed Authentication & Elimination of Hardcoded Users (Priority: P1) 🎯 MVP, Phase 4: User Story 2 - On-Demand Complete Database Seeding Script (Priority: P2), Phase 5: Polish & Verification, Tasks: PostgreSQL User Authentication & On-Demand Database Seeding, Tests for User Story 1 (+2 more)

### Community 36 - "RestoCore.IntegrationTests.csproj"
Cohesion: 0.18
Nodes (10): Testcontainers.Redis (4.14.0), net10.0, coverlet.collector (6.0.4), FluentAssertions (8.10.0), Microsoft.AspNetCore.Mvc.Testing (10.0.11), Microsoft.NET.Test.Sdk (17.14.1), Testcontainers.PostgreSql (4.14.0), xunit (2.9.3) (+2 more)

### Community 37 - "EntityTypeBuilder"
Cohesion: 0.23
Nodes (6): 1.2 Índices en PostgreSQL (`OrderConfiguration` y `UserConfiguration`), User Story 2 - Persistencia JSONB con Operador Nativo `@>`, Índices GIN/B-Tree Parciales y Normalización de Login (Priority: P1), OrderConfiguration, TableConfiguration, TenantConfiguration, UserConfiguration

### Community 39 - "CanvasElementDto"
Cohesion: 0.08
Nodes (20): CanvasElementDto, DishId, Height, Width, X, Y, ZIndex, CanvasElement (+12 more)

### Community 40 - "RestoCore.Application.csproj"
Cohesion: 0.22
Nodes (7): FluentValidation (12.1.1), Microsoft.EntityFrameworkCore (10.0.11), Microsoft.Extensions.Caching.Abstractions (10.0.11), net10.0, Microsoft.NET.Sdk, net10.0, Microsoft.NET.Sdk

### Community 41 - "AuthEndpoints.cs"
Cohesion: 0.27
Nodes (6): RestoCore.Application.Features.Auth.Commands, Implementation for User Story 1, AuthEndpoints, LoginRequest, LoginResponse, UserProfileResponse

### Community 42 - "IQrCodeService"
Cohesion: 0.29
Nodes (3): IQrCodeService, QrCodeService, QrCodeServiceTests

### Community 43 - "RestoCore.SecurityTests.csproj"
Cohesion: 0.20
Nodes (9): net10.0, coverlet.collector (6.0.4), FluentAssertions (8.10.0), Microsoft.AspNetCore.Mvc.Testing (10.0.11), Microsoft.NET.Test.Sdk (17.14.1), Testcontainers.PostgreSql (4.14.0), xunit (2.9.3), xunit.runner.visualstudio (3.1.4) (+1 more)

### Community 44 - "RestoCore.UnitTests.csproj"
Cohesion: 0.22
Nodes (8): Moq (4.20.72), net10.0, coverlet.collector (6.0.4), FluentAssertions (8.10.0), Microsoft.NET.Test.Sdk (17.14.1), xunit (2.9.3), xunit.runner.visualstudio (3.1.4), Microsoft.NET.Sdk

### Community 45 - "ref_k6"
Cohesion: 0.22
Nodes (4): options, options, options, options

### Community 46 - "Category"
Cohesion: 0.08
Nodes (22): 1.1 `IUnitOfWork`, 1.2 `ICatalogRepository`, 1.3 `IKitchenRepository`, 1.4 `IMenuLayoutRepository`, 1.5 `ITenantRepository`, 1. Domain Interface Contracts, Category, Description (+14 more)

### Community 47 - ".MapAdminCatalogEndpoints"
Cohesion: 0.17
Nodes (14): 4. Application Models & Transfer Objects (DTOs), `DeleteMenuItemCommand`, `MenuItemArchiveResponse`, `PatchMenuItemCommand`, `RestoreMenuItemCommand`, `UpdateMenuItemCommand`, Implementation for User Story 2, Phase 4: User Story 2 - Soft Delete with 7-Day Retention & Friendly Response (Priority: P2) (+6 more)

### Community 48 - "create-new-feature.ps1"
Cohesion: 0.46
Nodes (7): ConvertTo-CleanBranchName(), Get-BranchName(), Get-HighestNumberFromBranches(), Get-HighestNumberFromNames(), Get-HighestNumberFromRemoteRefs(), Get-HighestNumberFromSpecs(), Get-NextBranchNumber()

### Community 49 - "PublicMenuHybridCacheService"
Cohesion: 0.17
Nodes (3): PublicMenuCacheEntry, PublicMenuHybridCacheService, PublicMenuHybridCacheServiceTests

### Community 50 - "ValidationBehavior"
Cohesion: 0.17
Nodes (13): Implementation for User Story 3, Implementation Strategy, Incremental Delivery (`US4` + `US5` - Priority P2), MVP First (`US1` + `US2` + `US3` - Priority P1), Phase 5: User Story 3 - Mediador CQRS Precompilado sin Reflexión y Canalización `IPipelineBehavior` (Priority: P1), Tests for User Story 3, ValidationBehavior, IPipelineBehavior (+5 more)

### Community 51 - "RestoCore.Api.csproj"
Cohesion: 0.29
Nodes (6): Microsoft.AspNetCore.Authentication.JwtBearer (10.0.11), Microsoft.AspNetCore.OpenApi (10.0.11), Swashbuckle.AspNetCore (10.2.3), Microsoft.NET.Sdk.Web, net10.0, Microsoft.EntityFrameworkCore.Design (10.0.11)

### Community 52 - "IKitchenRepository"
Cohesion: 0.13
Nodes (20): RestoCore.Application.Features.Kitchen.Commands, 2. Infrastructure Entity Framework Implementations, 3. Concurrency Strategy: Retry with Exponential Backoff, Data Model & Interface Contracts: Repositories and Unit of Work, 2. Research Decisions, Decision 1: Scope and Granularity of Repositories, Decision 2: Unit of Work Implementation Pattern, Decision 3: Optimistic Concurrency Control & Retry Strategy (+12 more)

### Community 53 - ".Handle"
Cohesion: 0.35
Nodes (6): OrderIngestionPayload, OrderItemInput, SubmitOrderCommand, SubmitOrderCommandHandler, SubmitOrderItemDto, SubmitOrderResponse

### Community 54 - "MenuPublishJob"
Cohesion: 0.11
Nodes (17): User Scenarios & Testing *(mandatory)*, User Story 1 - Kitchen Availability Updates with Concurrency Resilience (Priority: P1), User Story 2 - Catalog Management via Isolated Catalog Repository & Unit of Work (Priority: P2), User Story 3 - Canvas Layout Compilation & Job State Management (Priority: P3), MenuPublishJob, AssetsQueued, CdnPurgeRequested, CompletedAt (+9 more)

### Community 55 - "RestoreMenuItemCommand"
Cohesion: 0.13
Nodes (17): 1. Running Automated Tests, 2. Testing Endpoints via cURL or REST Client, A. Modifying a Dish (PUT), B. Soft Deleting a Dish (DELETE), C. Restoring an Archived Dish (POST), Quickstart: Menu Item CRUD, Soft Delete with 7-Day Retention, and Purge Worker, Dependencies & Execution Order, Implementation for User Story 3 (+9 more)

### Community 57 - "Speckit SDD Workflow"
Cohesion: 0.50
Nodes (4): Plan Template Structure, Spec Template Structure, Tasks Template Structure, Speckit SDD Workflow

### Community 59 - "RestoCore.Application.Features.Analytics.DTOs"
Cohesion: 0.16
Nodes (4): RestoCore.Application.Features.Analytics.Commands, RestoCore.Application.Features.Analytics.DTOs, RestoCore.Application.Features.Analytics.Queries, RestoCore.Infrastructure.Messaging

### Community 60 - "2. Flujo de Ejecucion"
Cohesion: 0.25
Nodes (7): 1. Activacion y Entradas del Usuario, 2. Flujo de Ejecucion, 3. Ejemplo de Salida y Reporte, Paso 1: Deteccion y Comparacion de Artefactos, Paso 2: Determinacion de Gaps Prioritarios, Paso 3: Generacion de Especificacion SDD, Sync Docs to Backend Specs (`sync-docs-specs`)

### Community 70 - "2. CQRS Commands, Queries & DTOs"
Cohesion: 0.15
Nodes (12): 1.1 Category Entity (`categories` table), 1.2 Tenant Entity Extension (`tenants` table), 1. Domain Entities & Database Schema Changes, 2.3 Public Menu Response Model Extension, 2. CQRS Commands, Queries & DTOs, 3.1 Referential Integrity Guardrail on Category Deletion, 3.2 Category Field Validation Rules, 3.3 Tenant Profile Field Validation Rules (+4 more)

### Community 71 - "Fact"
Cohesion: 0.18
Nodes (4): SwaggerEndpointTests, AspireDashboardReadinessTests, ContainerReadinessTests, ItemAvailabilityStateTests

### Community 72 - "ApplicationDbContext"
Cohesion: 0.09
Nodes (18): 1. Context and Problem Statement, 3. Best Practices Alignment, Phase 0 Research: Repositories, Unit of Work, and Concurrency Control, ApplicationDbContext, Categories, CurrentTenantId, DailyCategoryInteractionSummaries, DailyItemViewSummaries (+10 more)

### Community 74 - "BrandingConfig"
Cohesion: 0.11
Nodes (20): 2. Restaurant Profile Extension & Multi-Tenant Isolation, Alternatives Considered, Context & Problem, Decision, PatchBrandingCommand, PatchBrandingCommandHandler, UpdateBrandingCommand, UpdateBrandingCommandHandler (+12 more)

### Community 75 - "ScopeCapturingLogger"
Cohesion: 0.18
Nodes (3): NullScope, ScopeCapturingLogger, Scopes

### Community 76 - "1. Resolved Clarifications & Key Technical Decisions"
Cohesion: 0.22
Nodes (8): 1. Resolved Clarifications & Key Technical Decisions, 2. Best Practices & Design Constraints, D-01: Soft Delete Implementation in EF Core, D-02: Active Kitchen Orders Verification Before Soft Deletion, D-03: Human-Friendly Response for Soft Delete (`MenuItemArchiveResponse`), D-04: Restoration within 7-Day Window (`POST /api/v1/admin/items/{id}/restore`), D-05: Scheduled Purge Worker (`PurgeArchivedDishesWorker`), Research & Architectural Findings: Menu Item CRUD, Soft Delete with 7-Day Retention, and Purge Worker

### Community 77 - "UnitOfWork"
Cohesion: 0.18
Nodes (7): Edge Cases, Dependencies & Execution Order, Format: `[ID] [P?] [Story] Description`, Phase 1: Setup & Foundational Contracts, Phase 5: Verification, Regressions & Graphify Re-evaluation, Tasks: Repositories, Unit of Work, and Concurrency Control, UnitOfWork

### Community 78 - ".Mediator_Send_ShouldThrowValidationExceptionWhenValidationBehaviorFails"
Cohesion: 0.30
Nodes (4): MediatorPipelineTests, SampleCommand, SampleCommandHandler, SampleCommandValidator

### Community 79 - "DailyMenuTrafficSummary"
Cohesion: 0.05
Nodes (41): RestoCore.Domain.Common, 2.1 `DailyMenuTrafficSummary`, 2.2 `DailyItemViewSummary`, 2.3 `DailyCategoryInteractionSummary`, 2.4 `HourlyMenuTrafficSummary`, 2. New Analytics Roll-Up Entities (`src/RestoCore.Domain/Entities/`), BaseEntity, CreatedAt (+33 more)

### Community 80 - "PublicMenuResponse"
Cohesion: 0.09
Nodes (24): Implementation for User Story 3, Phase 5: User Story 3 - Public Menu Exposure of Contact Profile (Priority: P3), Tests for User Story 3, PublicBrandingDto, BackgroundColor, CoverBannerUrl, FontFamily, LayoutMode (+16 more)

### Community 81 - "RestoCore.Domain.Entities"
Cohesion: 0.14
Nodes (6): RestoCore.Domain.Entities, RestoCore.SecurityTests.MultiTenancy, RestoCore.Infrastructure.MultiTenancy, RestoCore.Infrastructure.Persistence.Repositories, RestoCore.Infrastructure.Persistence.Seed, RestoCore.Infrastructure.Persistence.Configurations

### Community 82 - "User"
Cohesion: 0.20
Nodes (11): 2. Multi-Tenancy Scope for `User` Entity, Decision, User, Email, IsActive, Name, PasswordHash, PrimaryRole (+3 more)

### Community 83 - "CreateMenuItemCommand"
Cohesion: 0.11
Nodes (12): RestoCore.Application.Features.Categories.Validators, RestoCore.Application.Features.Tenants.Validators, RestoCore.Application.Features.MenuLayout.Validators, RestoCore.Application.Features.MenuItems.Validators, CreateCategoryCommand, CreateCategoryValidator, CreateMenuItemCommand, CreateMenuItemValidator (+4 more)

### Community 84 - ".Send"
Cohesion: 0.39
Nodes (4): User Story 3 - Mediador CQRS Precompilado sin Reflexión y Canalización `IPipelineBehavior` (Priority: P1), IMediator, ISender, Mediator

### Community 85 - "2. Research Decisions"
Cohesion: 0.25
Nodes (7): 1. Context and Problem Statement, 2. Research Decisions, Decision 1: Queue Technology: Redis Streams vs RabbitMQ vs PostgreSQL LISTEN/NOTIFY, Decision 2: Real-Time Protocol: Native ASP.NET Core WebSockets vs SignalR, Decision 3: Idempotency and Stream Acknowledgment, Decision 4: Entity Model and Comanda Snapshot Persistence, Phase 0 Research: Asynchronous Order Processing with Redis Streams and WebSockets

### Community 86 - "Implementation Plan: Repositories, Unit of Work, and Concurrency Control"
Cohesion: 0.29
Nodes (6): Constitution Check, Documentation (this feature), Implementation Plan: Repositories, Unit of Work, and Concurrency Control, Project Structure, Source Code, Technical Context

### Community 87 - "Tasks: Optimizaciones Arquitectónicas de Persistencia JSONB/GIN, Caché Híbrido L1/L2, Mediador CQRS, Gobernanza OpenTelemetry/OPA y Ciclo de Vida de Streams/WebSockets"
Cohesion: 0.20
Nodes (9): Dependencies & Execution Order, Format: `[ID] [P?] [Story] Description`, Parallel Example: User Story 4 (`ADR-0018`), Parallel Opportunities, Phase 1: Setup (Shared Infrastructure), Phase 2: Foundational (Blocking Prerequisites), Phase 8: Polish & Cross-Cutting Concerns, Phase Dependencies (+1 more)

### Community 88 - "IEndpointRouteBuilder"
Cohesion: 0.17
Nodes (5): RestoCore.Application.Features.Storage.Commands, AdminTenantEndpoints, HealthEndpoints, OrderEndpoints, StorageEndpoints

### Community 89 - "ITenantContext"
Cohesion: 0.16
Nodes (18): Implementation for User Story 1, Phase 3: User Story 1 - Dish Modification & Relational Validation (Priority: P1) 🎯 MVP, Tests for User Story 1, ITenantContext, HasTenant, TenantId, TenantSlug, MenuItemResponse (+10 more)

### Community 90 - ".Handle"
Cohesion: 0.16
Nodes (19): 5. Deferral of QR Table Occupancy & Turnover (ADR-0012 Compliance), Functional Requirements, Implementation for User Story 2, Phase 1: Setup (Shared Infrastructure), Phase 4: User Story 2 - Executive Analytics Dashboard & Menu Engineering Matrix (Priority: P2), Tests for User Story 2, AnalyticsEndpoints, AnalyticsDashboardResponse (+11 more)

### Community 91 - "IOrderQueueService"
Cohesion: 0.23
Nodes (8): Phase 2: User Story 1 - Client Order Ingestion (Priority: P1) 🎯 MVP, Phase 3: User Story 2 - Sequential FIFO Background Worker (Priority: P2), Phase 5: Verification, Regressions & Graphify, Tasks: Asynchronous Order Processing with FIFO Queue and WebSockets, 4. Redis Streams Reactivo, Consulta por Lote (`ANY`) y Ciclo de Vida de Sockets/Redis (`ADR-0018` / `OPT-05`, `OPT-06`), IOrderQueueService, OrderStreamMessage, RedisOrderQueueService

### Community 93 - "DateTimeOffset"
Cohesion: 0.10
Nodes (10): RestoCore.Infrastructure.Persistence.Migrations, Assumptions, IStorageService, PresignedUploadResponse, GeneratePresignedUploadCommand, GeneratePresignedUploadCommandHandler, GeneratePresignedUploadValidator, InitialCreate (+2 more)

### Community 94 - "Quickstart & Verification Guide: Category ABMC & Restaurant Profile"
Cohesion: 0.29
Nodes (6): 1. Prerequisites, 2. Running Unit Tests, 3.1 Category ABMC Verification, 3.2 Restaurant Profile Management Verification, 3. Manual / API Verification with cURL, Quickstart & Verification Guide: Category ABMC & Restaurant Profile

### Community 95 - "Feature Specification: Menu Analytics, Interaction Telemetry and Menu Engineering Dashboard"
Cohesion: 0.17
Nodes (12): 1. Context & Business Need, 2. User Scenarios & Testing *(mandatory)*, 3. Requirements *(mandatory)*, 4. Success Criteria *(mandatory)*, 5. Assumptions & Scope Boundaries, Edge Cases, Feature Specification: Menu Analytics, Interaction Telemetry and Menu Engineering Dashboard, Key Entities (+4 more)

### Community 96 - "ICatalogRepository"
Cohesion: 0.14
Nodes (27): Summary, Functional Requirements, Implementation for User Story 2, Implementation for User Story 3, Phase 3: User Story 2 - Catalog Management via Isolated Catalog Repository & Unit of Work (Priority: P2), Phase 4: User Story 3 - Canvas Layout Compilation & Tenant Repositories (Priority: P3), Tests for User Story 2, Phase 1: Setup (Shared Infrastructure) (+19 more)

### Community 97 - "GetTableQrQueryHandler"
Cohesion: 0.39
Nodes (5): RestoCore.Application.Features.QrCodes.Queries, AdminTableEndpoints, GetTableQrQuery, GetTableQrQueryHandler, QrResult

### Community 98 - "IPasswordHasher"
Cohesion: 0.31
Nodes (3): IPasswordHasher, PasswordHasher, PasswordHasherTests

### Community 99 - ".Handle"
Cohesion: 0.26
Nodes (6): Implementation for User Story 1, IPublicMenuCacheService, GetPublicMenuQuery, GetPublicMenuQueryHandler, LocalDevelopmentCdnPurgeService, PublicMenuSoftDeleteFilterTests

### Community 100 - "TenantProfileResponse"
Cohesion: 0.22
Nodes (15): 2.2 Restaurant Profile Operations, Commands:, Queries:, Implementation for User Story 2, Phase 4: User Story 2 - Restaurant Profile Management (Priority: P2), Tests for User Story 2, AdminProfileEndpoints, PatchTenantProfileCommand (+7 more)

### Community 101 - "OrderItemSnapshot"
Cohesion: 0.12
Nodes (21): 1.1 `OrderItemSnapshot` y `OrderModifierSnapshot` (`RestoCore.Domain.Entities`), 1. Modelos de Persistencia y Serialización JSONB (`ADR-0014`), 1. Despacho CQRS Precompilado y `IPipelineBehavior` (`ADR-0016` / `OPT-01`), 2. Caché Híbrido L1/L2 y Filtrado de Baja Suave en Carta QR (`ADR-0015` / `OPT-02`, `OPT-03`), 3. Índices GIN, Operador JSONB `@>` con camelCase y Normalización B-Tree (`ADR-0014` / `OPT-04`, `OPT-08`), Phase 0 Research: Optimizaciones Arquitectónicas del Backend (`012-backend-architecture-optimizations`), Implementation for User Story 2, Phase 4: User Story 2 - Persistencia JSONB con Operador Nativo `@>`, Índices GIN/B-Tree Parciales y Normalización de Login (Priority: P1) (+13 more)

### Community 102 - "Tasks: Menu Analytics, Interaction Telemetry and Menu Engineering Dashboard"
Cohesion: 0.14
Nodes (13): Dependencies & Execution Order, Format: `[ID] [P?] [Story] Description`, Implementation for User Story 3, Implementation Strategy, Incremental Delivery, MVP First (User Story 1 Only), Parallel Example: User Story 1, Parallel Opportunities (+5 more)

### Community 103 - "MenuCompilationResult"
Cohesion: 0.50
Nodes (4): MenuCompilationResult, AssetsQueued, CompiledLayout, VersionHash

### Community 104 - ".OrderProcessorWorker_ProcessOrderMessageAsync_ShouldBatchLoadMenuItemsInSingleQuery"
Cohesion: 0.25
Nodes (5): 4.1 `IPipelineBehavior<in TRequest, TResponse>` (`RestoCore.Application.Common.Mediator`), 4.2 `ICatalogRepository.GetMenuItemsWithModifiersByIdsAsync` (`RestoCore.Application.Common.Interfaces.Repositories`), 4.3 `IOrderQueueService.GetActiveTenantStreamsAsync` (`RestoCore.Application.Common.Interfaces`), 4. Interfaces Internas de Aplicación e Infraestructura, WebSocketAndStreamWorkerOptimizationTests

### Community 105 - "CollectAnalyticsEventCommandHandler"
Cohesion: 0.49
Nodes (3): CollectAnalyticsEventCommand, CollectAnalyticsEventCommandHandler, CollectAnalyticsEventCommandHandlerTests

### Community 106 - "LoginCommandHandler"
Cohesion: 0.51
Nodes (4): AuthenticatedUserResult, LoginCommand, LoginCommandHandler, LoginCommandHandlerTests

### Community 107 - "Functional Requirements"
Cohesion: 0.22
Nodes (8): 1. Context & Business Need, 2. User Scenarios & Testing *(mandatory)*, 3. Requirements *(mandatory)*, 4. Success Criteria *(mandatory)*, Feature Specification: PostgreSQL User Authentication & On-Demand Database Seeding, Functional Requirements, User Story 1 - PostgreSQL-Backed Authentication & Elimination of Hardcoded Users (Priority: P1) 🎯 MVP, User Story 2 - On-Demand Complete Database Seeding Script (Priority: P2)

### Community 108 - "User Scenarios & Testing *(mandatory)*"
Cohesion: 0.22
Nodes (8): Assumptions, Edge Cases, Feature Specification: Optimizaciones Arquitectónicas de Persistencia JSONB/GIN, Caché Híbrido L1/L2, Mediador CQRS, Gobernanza OpenTelemetry/OPA y Ciclo de Vida de Streams/WebSockets, Key Entities, Requirements *(mandatory)*, Success Criteria *(mandatory)*, User Scenarios & Testing *(mandatory)*, User Story 1 - Caché Híbrido Multinivel L1/L2 y Filtrado Estricto de Baja Suave en Carta QR (Priority: P1)

### Community 109 - "1. REST Endpoint: `POST /api/v1/auth/login`"
Cohesion: 0.22
Nodes (8): 1. REST Endpoint: `POST /api/v1/auth/login`, 2. CLI & Script Contract for On-Demand Seeding, API & CLI Contracts: PostgreSQL User Authentication & On-Demand Database Seeding, Direct .NET CLI Command, PowerShell Script, Request Body, Response `200 OK`, Response `401 Unauthorized`

### Community 110 - "2. Contracts & Services"
Cohesion: 0.25
Nodes (7): 1.1 `Order` Entity, 1. Domain Entities & Value Objects, 2.1 `IOrderQueueService`, 2.2 `IKitchenNotifier` (WebSocket Connection Registry), 2.3 `IOrderRepository`, 2. Contracts & Services, Data Model & Interface Contracts: Asynchronous Order Processing & WebSockets

### Community 111 - "PurgeArchivedDishesWorker"
Cohesion: 0.09
Nodes (24): 1. Summary, 2. Technical Context, 3. Constitution Check, 4. Project Structure, 5. Implementation Phases, Documentation (this feature), Implementation Plan: Menu Item CRUD, Soft Delete with 7-Day Retention, and Purge Worker, Source Code Changes Planned (+16 more)

### Community 112 - "AnalyticsAggregationWorker"
Cohesion: 0.29
Nodes (6): 3.1 `AnalyticsEventMessage` (Application DTO / Queue Payload), 3. Ephemeral Queue Message Model, Implementation for User Story 1, Phase 3: User Story 1 - Asynchronous Ingestion of Diner Interaction Events (Priority: P1) - MVP, Tests for User Story 1, AnalyticsAggregationWorker

### Community 113 - "AnalyticsEventMessage"
Cohesion: 0.33
Nodes (3): IAnalyticsEventQueue, AnalyticsEventMessage, AnalyticsEventQueue

### Community 114 - "Data Model: PostgreSQL User Authentication & On-Demand Database Seeding"
Cohesion: 0.29
Nodes (6): 1. Domain Entity: `User` (`users` table), 2. CQRS Command & DTOs (`Features/Auth`), 3. Seed Dataset Specification (`DatabaseSeeder`), Data Model: PostgreSQL User Authentication & On-Demand Database Seeding, EF Core Configuration (`UserConfiguration.cs`), `LoginCommand`

### Community 115 - "Architectural Research: PostgreSQL User Authentication & On-Demand Database Seeding"
Cohesion: 0.29
Nodes (6): 1. Cryptographic Password Hashing Strategy, 3. On-Demand Seeding Execution Mechanism, Architectural Research: PostgreSQL User Authentication & On-Demand Database Seeding, Decision, Decision, Rationale

### Community 116 - "ICdnPurgeService"
Cohesion: 0.12
Nodes (18): 1. Category Referential Integrity Guardrail on Deletion, 3. Public Menu Exposure & Latency Budget Compliance, 4. Category ABMC Partial Update (PATCH) Semantics, 5. DTO Naming & Serialization Strategy, Alternatives Considered, Architectural Research: Category ABMC & Restaurant Contact Profile Management, Context & Problem, Context & Problem (+10 more)

### Community 117 - "Implementation Plan: Asynchronous Order Processing with FIFO Queue and WebSockets"
Cohesion: 0.33
Nodes (5): Constitution Check, Implementation Plan: Asynchronous Order Processing with FIFO Queue and WebSockets, Project Structure, Summary, Technical Context

### Community 118 - "Phase 1 Data Model: Optimizaciones Arquitectónicas del Backend (`012-backend-architecture-optimizations`)"
Cohesion: 0.25
Nodes (7): 2.1 `PublicMenuCacheEntry` (`RestoCore.Application.Common.Caching`), 2. Modelos de Caché Híbrido L1/L2 (`ADR-0015`), 3.1 `IPipelineBehavior<in TRequest, TResponse>` y `IRequestInvoker<TResponse>`, 3. Modelos de Despacho CQRS (`ADR-0016`), 4.1 `OpaUser` (`RestoCore.Infrastructure.Authorization`), 4. Modelos de Seguridad y Autorización OPA (`ADR-0017`), Phase 1 Data Model: Optimizaciones Arquitectónicas del Backend (`012-backend-architecture-optimizations`)

### Community 119 - "2. `GET /api/v1/admin/analytics/dashboard`"
Cohesion: 0.17
Nodes (11): 1. `POST /api/v1/analytics/collect`, 2. `GET /api/v1/admin/analytics/dashboard`, Error Responses, Overview, Overview, Phase 1 API Contracts: Menu Analytics, Interaction Telemetry and Menu Engineering Dashboard, Query Parameters, Request Body (`application/json` — `AnalyticsEventRequest`) (+3 more)

### Community 120 - "Implementation Plan: Optimizaciones Arquitectónicas de Persistencia JSONB/GIN, Caché Híbrido L1/L2, Mediador CQRS, Gobernanza OpenTelemetry/OPA y Ciclo de Vida de Streams/WebSockets"
Cohesion: 0.25
Nodes (7): Complexity Tracking, Constitution Check, Documentation (this feature), Implementation Plan: Optimizaciones Arquitectónicas de Persistencia JSONB/GIN, Caché Híbrido L1/L2, Mediador CQRS, Gobernanza OpenTelemetry/OPA y Ciclo de Vida de Streams/WebSockets, Project Structure, Source Code (repository root), Technical Context

### Community 121 - ".SeedAsync"
Cohesion: 0.25
Nodes (6): 1. Summary, 2. Technical Context, 3. Constitution Check, 4. Source Code Layout, Implementation Plan: PostgreSQL User Authentication & On-Demand Database Seeding, DatabaseSeeder

### Community 122 - "Quickstart & Verification Guide: PostgreSQL User Authentication & On-Demand Database Seeding"
Cohesion: 0.40
Nodes (4): 1. Run Unit Tests, 2. Seed Database On Demand, 3. Authenticate via REST API, Quickstart & Verification Guide: PostgreSQL User Authentication & On-Demand Database Seeding

### Community 123 - "CapturingHttpMessageHandler"
Cohesion: 0.29
Nodes (3): CapturingHttpMessageHandler, CallCount, LastRequestBody

### Community 125 - "Quickstart: Menu Analytics, Interaction Telemetry and Menu Engineering Dashboard"
Cohesion: 0.20
Nodes (9): 1. Prerequisites, 2.1 Record a QR Menu Open (`menu_view`), 2.2 Record a Dish Modal Open (`item_view`), 2. Ingest Diner Telemetry Events (`POST /api/v1/analytics/collect`), 3.1 Authenticate as Restaurant Owner, 3.2 Retrieve Analytics & Menu Engineering Matrix, 3. Query Executive Analytics Dashboard (`GET /api/v1/admin/analytics/dashboard`), 4. Run Automated Tests (+1 more)

### Community 127 - "IRequest"
Cohesion: 0.70
Nodes (4): IRequest, ProvisionTenantCommand, ProvisionTenantCommandHandler, TenantSummaryResponse

### Community 128 - "Implementation Plan: Menu Analytics, Interaction Telemetry and Menu Engineering Dashboard"
Cohesion: 0.20
Nodes (8): Complexity Tracking, Constitution Check, Documentation (this feature), Implementation Plan: Menu Analytics, Interaction Telemetry and Menu Engineering Dashboard, Project Structure, Source Code (repository root), Summary, Technical Context

### Community 130 - "Quickstart: Verificación de Optimizaciones Arquitectónicas (`012-backend-architecture-optimizations`)"
Cohesion: 0.50
Nodes (3): 1. Compilación de la Solución, 2. Ejecución de Pruebas Unitarias y de Seguridad (`OPT-01` a `OPT-10`), Quickstart: Verificación de Optimizaciones Arquitectónicas (`012-backend-architecture-optimizations`)

### Community 131 - "Phase 1 Data Model: Menu Analytics, Interaction Telemetry and Menu Engineering Dashboard"
Cohesion: 0.33
Nodes (5): 1.1 `MenuItem` (`src/RestoCore.Domain/Entities/MenuItem.cs`), 1.2 `Tenant` (`src/RestoCore.Domain/Entities/Tenant.cs`), 1. Domain Entity Extensions, 4. Entity Relationship Diagram, Phase 1 Data Model: Menu Analytics, Interaction Telemetry and Menu Engineering Dashboard

### Community 132 - "Phase 0 Research: Menu Analytics, Interaction Telemetry and Menu Engineering Dashboard"
Cohesion: 0.33
Nodes (5): 1. Asynchronous Telemetry Ingestion Mechanism (`< 50ms p95`), 2. Pre-Aggregated Roll-Up Schema vs. Raw Event Scans, 3. Menu Engineering Matrix Computation & Deterministic Edge Cases, 4. Declarative OPA Authorization & Multi-Tenant Isolation, Phase 0 Research: Menu Analytics, Interaction Telemetry and Menu Engineering Dashboard

## Knowledge Gaps
- **345 isolated node(s):** `net10.0`, `Microsoft.AspNetCore.Authentication.JwtBearer (10.0.11)`, `Microsoft.AspNetCore.OpenApi (10.0.11)`, `Microsoft.EntityFrameworkCore.Design (10.0.11)`, `Swashbuckle.AspNetCore (10.2.3)` (+340 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 666 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **19 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `ApplicationDbContext` connect `ApplicationDbContext` to `Implementation Plan: Menu Analytics, Interaction Telemetry and Menu Engineering Dashboard`, `ContainerizedStackFixture`, `Order`, `TenantResolutionMiddleware`, `IApplicationDbContext`, `ModifierGroup`, `MenuPublishJobResponse`, `Table`, `MenuItem`, `Functional Requirements`, `Guid`, `.ProcessOrderMessageAsync`, `Tenant`, `UserRepository`, `Task`, `Category`, `PublicMenuHybridCacheService`, `IKitchenRepository`, `MenuPublishJob`, `1. Resolved Clarifications & Key Technical Decisions`, `UnitOfWork`, `DailyMenuTrafficSummary`, `RestoCore.Domain.Entities`, `User`, `ITenantContext`, `IOrderQueueService`, `DateTimeOffset`, `ICatalogRepository`, `.Handle`, `.SeedAsync`?**
  _High betweenness centrality (0.076) - this node is a cross-community bridge._
- **Are the 36 inferred relationships involving `MenuItem` (e.g. with `1. Mapa de Fuentes en `RestoCore-Docs`` and `1.2 `ICatalogRepository``) actually correct?**
  _`MenuItem` has 36 INFERRED edges - model-reasoned connections that need verification._
- **What connects `net10.0`, `Microsoft.AspNetCore.Authentication.JwtBearer (10.0.11)`, `Microsoft.AspNetCore.OpenApi (10.0.11)` to the rest of the system?**
  _345 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `PublicMenuItemDto` be split into smaller, more focused modules?**
  _Cohesion score 0.07936507936507936 - nodes in this community are weakly interconnected._
- **Why does `MenuItem` connect `MenuItem` to `Phase 1 Data Model: Menu Analytics, Interaction Telemetry and Menu Engineering Dashboard`, `Phase 0 Research: Menu Analytics, Interaction Telemetry and Menu Engineering Dashboard`, `ContainerizedStackFixture`, `IApplicationDbContext`, `ModifierGroup`, `MenuPublishJobResponse`, `Data Model: Menu Item CRUD, Soft Delete with 7-Day Retention, and Purge Worker`, `1. Mapa de Fuentes en `RestoCore-Docs``, `Guid`, `Tenant`, `Task`, `Category`, `.MapAdminCatalogEndpoints`, `IKitchenRepository`, `RestoreMenuItemCommand`, `Fact`, `ApplicationDbContext`, `1. Resolved Clarifications & Key Technical Decisions`, `DailyMenuTrafficSummary`, `ITenantContext`, `.Handle`, `DateTimeOffset`, `.Handle`, `.OrderProcessorWorker_ProcessOrderMessageAsync_ShouldBatchLoadMenuItemsInSingleQuery`, `PurgeArchivedDishesWorker`?**
  _High betweenness centrality (0.049) - this node is a cross-community bridge._
- **Should `Structured Logging & Traces Specification` be split into smaller, more focused modules?**
  _Cohesion score 0.05102040816326531 - nodes in this community are weakly interconnected._
- **Why does `ITenantContext` connect `ITenantContext` to `ContainerizedStackFixture`, `TenantResolutionMiddleware`, `OpaAuthorizationHandler`, `CategoryResponse`, `ModifierGroup`, `1. Category ABMC Endpoints (`Admin - Catalog`)`, `LayoutConfigDto`, `Guid`, `Tenant`, `.MapAdminCatalogEndpoints`, `RestoreMenuItemCommand`, `ApplicationDbContext`, `BrandingConfig`, `ScopeCapturingLogger`, `User`, `.Handle`, `DateTimeOffset`, `ICatalogRepository`, `GetTableQrQueryHandler`, `TenantProfileResponse`, `PurgeArchivedDishesWorker`, `ICdnPurgeService`?**
  _High betweenness centrality (0.046) - this node is a cross-community bridge._