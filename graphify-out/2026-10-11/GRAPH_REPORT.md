# Graph Report - resto-core-back  (2026-10-11)

## Corpus Check
- 236 files · ~90,012 words
- Verdict: corpus is large enough that graph structure adds value.
- Unclassified: 4 file(s) not represented in the graph (top: (none) 2, .example 1, .rego 1)

## Summary
- 1366 nodes · 3246 edges · 104 communities (80 shown, 24 thin omitted)
- Extraction: 85% EXTRACTED · 15% INFERRED · 0% AMBIGUOUS · INFERRED: 483 edges (avg confidence: 0.89)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `b7541f70`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- PublicMenuResponse
- Structured Logging & Traces Specification
- Tenant Entity
- .CompleteLifecycle_TenantToPublicMenuAndQr_SucceedsEndToEnd
- Order
- DateTimeOffset
- RestoCore Backend Constitution
- TenantContext
- OpaInput
- Program.cs
- API Contracts: Menu Item CRUD, Soft Delete with 7-Day Retention, and Restore
- fluentassertions
- system
- IApplicationDbContext
- Feature Specification: Menu Item CRUD, Soft Delete with 7-Day Retention, and Purge Worker
- OpaClient.cs
- ModifierGroup
- common.ps1
- http
- MenuPublishJobResponse
- 4. Application Models & Transfer Objects (DTOs)
- Tasks: Repositories, Unit of Work, and Concurrency Control
- RestoCore.Infrastructure.csproj
- app.py
- Table
- .ComputeVersionHash
- MenuItem
- RestoCore.Domain.Entities
- 1. Mapa de Fuentes en `RestoCore-Docs`
- RestoCore.Application.Common.Interfaces
- CreateMenuItemCommand
- LayoutConfigDto
- .ProcessOrderMessageAsync
- Tenant
- ITenantContext
- Guid
- RestoCore.IntegrationTests.csproj
- RestoCore.Infrastructure.Persistence
- Task
- CanvasElement
- RestoCore.Application.csproj
- AuthEndpoints.cs
- IQrCodeService
- RestoCore.SecurityTests.csproj
- RestoCore.UnitTests.csproj
- ref_k6
- Category
- DeleteMenuItemCommand
- create-new-feature.ps1
- CanvasElementDto
- 2. Testing Endpoints via cURL or REST Client
- RestoCore.Api.csproj
- IKitchenRepository
- SubmitOrderCommand.cs
- MenuPublishJob
- PurgeArchivedDishesWorker
- Speckit SDD Workflow
- Observability & Tracing Research
- 1.2 `ICatalogRepository`
- 2. Flujo de Ejecucion
- Skill: Spec-Kit Checklist
- Skill: Spec-Kit Git Feature
- Admin App Streamlit Stack
- Skill: Spec-Kit Git Initialize
- Vector and Raster QR Generation
- Feature Specification: Asynchronous Order Processing with FIFO Queue and WebSockets
- Fact
- ApplicationDbContext
- BrandingConfig
- .Handle
- 1. Resolved Clarifications & Key Technical Decisions
- Implementation Plan: Asynchronous Order Processing with FIFO Queue and WebSockets
- ICdnPurgeService
- ModifierOption
- OrderItemSnapshot
- MenuConfiguration
- IStorageService
- CreateCategoryCommand
- 2. Contracts & Services
- 2. Research Decisions
- ICatalogRepository
- .AddApplicationMediator
- .Send
- RestoreMenuItemCommand
- PresignedUploadResponse
- MenuItemResponse
- .ConfigureWebHost
- GetTableQrQueryHandler
- ILogger
- RedisOrderQueueService
- IRequest
- ToggleItemAvailabilityCommand
- OpenTelemetryExtensions
- LayoutConfig
- MenuCompilationResult

## God Nodes (most connected - your core abstractions)
1. `MenuItem` - 72 edges
2. `RestoCore.Application.Common.Interfaces` - 50 edges
3. `RestoCore.Domain.Entities` - 46 edges
4. `ApplicationDbContext` - 45 edges
5. `Tenant` - 44 edges
6. `ICatalogRepository` - 43 edges
7. `IUnitOfWork` - 42 edges
8. `Category` - 36 edges
9. `Order` - 36 edges
10. `RestoCore.Application.Common.Interfaces.Repositories` - 34 edges

## Surprising Connections (you probably didn't know these)
- `Phase 5: Verification, Regressions & Graphify Re-evaluation` --references--> `IApplicationDbContext`  [INFERRED]
  specs/006-repository-unitofwork-concurrency/tasks.md → src/RestoCore.Application/Common/Interfaces/IApplicationDbContext.cs
- `2.2 `IKitchenNotifier` (WebSocket Connection Registry)` --references--> `IKitchenNotifier`  [INFERRED]
  specs/007-order-fifo-processing-websockets/data-model.md → src/RestoCore.Application/Common/Interfaces/IKitchenNotifier.cs
- `2.1 `IOrderQueueService`` --references--> `IOrderQueueService`  [INFERRED]
  specs/007-order-fifo-processing-websockets/data-model.md → src/RestoCore.Application/Common/Interfaces/IOrderQueueService.cs
- `1.1 `IUnitOfWork`` --references--> `IUnitOfWork`  [INFERRED]
  specs/006-repository-unitofwork-concurrency/data-model.md → src/RestoCore.Application/Common/Interfaces/IUnitOfWork.cs
- `1.3 `IKitchenRepository`` --references--> `IKitchenRepository`  [INFERRED]
  specs/006-repository-unitofwork-concurrency/data-model.md → src/RestoCore.Application/Common/Interfaces/Repositories/IKitchenRepository.cs

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

## Communities (104 total, 24 thin omitted)

### Community 0 - "PublicMenuResponse"
Cohesion: 0.05
Nodes (46): PublicBrandingDto, BackgroundColor, CoverBannerUrl, FontFamily, LayoutMode, LogoUrl, PrimaryColor, SecondaryColor (+38 more)

### Community 1 - "Structured Logging & Traces Specification"
Cohesion: 0.05
Nodes (42): k6 Aspire Swagger Requirements Checklist, Observability and Swagger API Contract, Swagger OpenAPI Explorer Endpoints, k6 & Aspire Quickstart Guide, k6 Performance Testing Scenarios, k6 Aspire Swagger Specification, US1: Interactive API Exploration, US2: Real-time Telemetry Dashboard (+34 more)

### Community 2 - "Tenant Entity"
Cohesion: 0.08
Nodes (24): Digital Menu Quality Checklist, Digital Menu OpenAPI Specification, Category Entity, MenuItem Entity, ModifierGroup Entity, ModifierOption Entity, Table Entity, Tenant Entity (+16 more)

### Community 3 - ".CompleteLifecycle_TenantToPublicMenuAndQr_SucceedsEndToEnd"
Cohesion: 0.14
Nodes (11): Program, OpaClient, OpaResponse, Result, AdminMenuLayoutEndpointsTests, FullLifecycleE2ETests, MenuPublishEndpointsTests, PublicMenuCanvasEndpointsTests (+3 more)

### Community 4 - "Order"
Cohesion: 0.12
Nodes (18): Phase 1: Setup & Data Contracts, IOrderRepository, Order, CompletedAt, CreatedAt, CustomerNotes, Id, ItemsSnapshot (+10 more)

### Community 5 - "DateTimeOffset"
Cohesion: 0.19
Nodes (3): InitialCreate, AddCanvasLayoutAndMenuPublishJobs, ApplicationDbContextModelSnapshot

### Community 6 - "RestoCore Backend Constitution"
Cohesion: 0.09
Nodes (20): AI Agents Operating Guidelines, Skill: Git Commit, Skill: Review TODOs, Skill: Spec-Kit Analyze, Skill: Spec-Kit Clarify, Skill: Spec-Kit Constitution, Skill: Spec-Kit Git Commit, Skill: Spec-Kit Git Remote (+12 more)

### Community 7 - "TenantContext"
Cohesion: 0.23
Nodes (6): TenantContext, TenantId, TenantSlug, TenantResolutionMiddleware, CrossTenantDatabaseIsolationTests, TenantResolutionTests

### Community 8 - "OpaInput"
Cohesion: 0.10
Nodes (18): OpaAuthorizationHandler, OpaRequirement, Action, ResourceType, IOpaClient, OpaInput, Action, Method (+10 more)

### Community 9 - "Program.cs"
Cohesion: 0.08
Nodes (8): RestoCore.Api.Endpoints, RestoCore.Infrastructure.MultiTenancy, RestoCore.Infrastructure.Qr, RestoCore.Api.Middlewares, RestoCore.Infrastructure.Cdn, RestoCore.UnitTests.MultiTenancy, RestoCore.Infrastructure.Telemetry, RestoCore.Infrastructure.Authorization

### Community 10 - "API Contracts: Menu Item CRUD, Soft Delete with 7-Day Retention, and Restore"
Cohesion: 0.14
Nodes (13): 1. Full Modification of Menu Item (`PUT /api/v1/admin/items/{id}`), 2. Partial Modification of Menu Item (`PATCH /api/v1/admin/items/{id}`), 3. Soft Delete Menu Item (`DELETE /api/v1/admin/items/{id}`), 4. Restore Menu Item (`POST /api/v1/admin/items/{id}/restore`), API Contracts: Menu Item CRUD, Soft Delete with 7-Day Retention, and Restore, Request, Request, Request (+5 more)

### Community 11 - "fluentassertions"
Cohesion: 0.12
Nodes (11): RestoCore.UnitTests.Features.Kitchen, RestoCore.IntegrationTests.Infrastructure, RestoCore.UnitTests.Features.Orders, RestoCore.Application.Features.PublicMenu.Queries, RestoCore.SecurityTests.MultiTenancy, RestoCore.UnitTests.Features.PublicMenu, RestoCore.Application.Features.PublicMenu.DTOs, RestoCore.UnitTests.PublicMenu (+3 more)

### Community 12 - "system"
Cohesion: 0.17
Nodes (9): RestoCore.Domain.ValueObjects, RestoCore.UnitTests.Features.MenuLayout, RestoCore.Application.Features.MenuLayout.Commands, RestoCore.Infrastructure.Services.Compilation, RestoCore.Application.Features.MenuLayout.Services, RestoCore.IntegrationTests.Endpoints, RestoCore.IntegrationTests.Fixtures, RestoCore.Infrastructure.BackgroundServices (+1 more)

### Community 13 - "IApplicationDbContext"
Cohesion: 0.15
Nodes (14): Background & Architectural Drivers, Edge Cases, Feature Specification: Repositories and Unit of Work for Concurrency and Persistence Decoupling, Measurable Outcomes, Requirements *(mandatory)*, Success Criteria *(mandatory)*, IApplicationDbContext, Categories (+6 more)

### Community 14 - "Feature Specification: Menu Item CRUD, Soft Delete with 7-Day Retention, and Purge Worker"
Cohesion: 0.20
Nodes (9): 1. Context & Business Need, 2. User Scenarios & Testing *(mandatory)*, 3. Edge Cases, 5. Success Criteria *(mandatory)*, Feature Specification: Menu Item CRUD, Soft Delete with 7-Day Retention, and Purge Worker, User Story 1 - Dish Modification & Relational Validation (Priority: P1) 🎯 MVP, User Story 2 - Soft Delete with 7-Day Retention & Friendly Response (Priority: P2), User Story 3 - Administrative Dish Restoration (Priority: P3) (+1 more)

### Community 15 - "OpaClient.cs"
Cohesion: 0.11
Nodes (4): RestoCore.Infrastructure.Storage, RestoCore.Infrastructure.WebSockets, RestoCore.Infrastructure.Messaging, HealthEndpoints

### Community 16 - "ModifierGroup"
Cohesion: 0.18
Nodes (9): ModifierGroup, IsRequired, MaxSelection, MenuItem, MenuItemId, MinSelection, Name, Options (+1 more)

### Community 17 - "common.ps1"
Cohesion: 0.23
Nodes (11): Find-FeatureDirByPrefix(), Find-SpecifyRoot(), Get-CurrentBranch(), Get-FeatureDirFromBranchPrefixOrExit(), Get-FeaturePathsEnv(), Get-Python3Command(), Get-RepoRoot(), Get-SpecKitEffectiveBranchName() (+3 more)

### Community 18 - "http"
Cohesion: 0.13
Nodes (15): ASPNETCORE_ENVIRONMENT, applicationUrl, commandName, dotnetRunMessages, environmentVariables, launchBrowser, applicationUrl, commandName (+7 more)

### Community 19 - "MenuPublishJobResponse"
Cohesion: 0.14
Nodes (14): AdminMenuLayoutEndpoints, PublishMenuCommand, PublishMenuCommandHandler, MenuPublishJobResponse, AssetsQueued, CdnPurgeRequested, EstimatedDurationSeconds, JobId (+6 more)

### Community 20 - "4. Application Models & Transfer Objects (DTOs)"
Cohesion: 0.15
Nodes (12): 1. Domain Entities & Value Objects, 2. PostgreSQL DDL Mapping & Indexes, 3. State Lifecycle & Transitions, 4. Application Models & Transfer Objects (DTOs), Data Model: Menu Item CRUD, Soft Delete with 7-Day Retention, and Purge Worker, `DeleteMenuItemCommand`, `MenuItem` (`RestoCore.Domain.Entities`), `MenuItemArchiveResponse` (+4 more)

### Community 21 - "Tasks: Repositories, Unit of Work, and Concurrency Control"
Cohesion: 0.33
Nodes (5): Dependencies & Execution Order, Format: `[ID] [P?] [Story] Description`, Phase 1: Setup & Foundational Contracts, Phase 5: Verification, Regressions & Graphify Re-evaluation, Tasks: Repositories, Unit of Work, and Concurrency Control

### Community 22 - "RestoCore.Infrastructure.csproj"
Cohesion: 0.13
Nodes (14): AWSSDK.S3 (4.0.102.5), Microsoft.Extensions.Caching.StackExchangeRedis (10.0.12), Npgsql.EntityFrameworkCore.PostgreSQL (10.0.3), Npgsql.OpenTelemetry (10.0.3), OpenTelemetry.Exporter.OpenTelemetryProtocol (1.18.0), OpenTelemetry.Extensions.Hosting (1.18.0), OpenTelemetry.Instrumentation.AspNetCore (1.18.0), OpenTelemetry.Instrumentation.Http (1.18.0) (+6 more)

### Community 24 - "Table"
Cohesion: 0.14
Nodes (13): Implementation for User Story 3, Phase 4: User Story 3 - Canvas Layout Compilation & Tenant Repositories (Priority: P3), IMenuLayoutRepository, ITenantRepository, Table, IsActive, Label, TableNumber (+5 more)

### Community 25 - ".ComputeVersionHash"
Cohesion: 0.33
Nodes (3): IMenuCompilationService, MenuCompilationService, MenuCompilationServiceTests

### Community 26 - "MenuItem"
Cohesion: 0.09
Nodes (20): 4. Requirements *(mandatory)*, Functional Requirements, Key Entities, MenuItem, Allergens, BasePrice, Category, CategoryId (+12 more)

### Community 27 - "RestoCore.Domain.Entities"
Cohesion: 0.14
Nodes (4): RestoCore.Domain.Entities, RestoCore.Domain.Common, RestoCore.Infrastructure.Persistence.Repositories, RestoCore.Infrastructure.Persistence.Configurations

### Community 28 - "1. Mapa de Fuentes en `RestoCore-Docs`"
Cohesion: 0.22
Nodes (8): 1. Mapa de Fuentes en `RestoCore-Docs`, 2. Flujo de Ejecución Paso a Paso, 3. Ejemplo Práctico de Resolución, Consulta de Documentación Arquitectónica (`consult-docs`), Paso 1: Búsqueda Rápida de Referencias, Paso 2: Lectura Dirigida del Documento, Paso 3: Consulta en el Grafo de Graphify (Opcional), Paso 4: Aplicación y Citado de la Fuente

### Community 29 - "RestoCore.Application.Common.Interfaces"
Cohesion: 0.13
Nodes (13): RestoCore.Infrastructure.Workers, RestoCore.Application.Common.Interfaces, RestoCore.Application.Common.Mediator, RestoCore.Application.Features.Tenants.Commands, RestoCore.Application.Common.Interfaces.Repositories, RestoCore.Application.Features.Kitchen.Commands, RestoCore.Application.Features.MenuItems.DTOs, RestoCore.Application.Features.MenuItems.Commands (+5 more)

### Community 30 - "CreateMenuItemCommand"
Cohesion: 0.13
Nodes (13): RestoCore.Application.Features.Categories.Validators, RestoCore.Application.Features.Tenants.Validators, RestoCore.UnitTests.Domain, RestoCore.Application.Features.MenuLayout.Validators, RestoCore.Application.Features.MenuItems.Validators, RestoCore.Application.Features.Categories.Commands, CreateCategoryValidator, CreateMenuItemCommand (+5 more)

### Community 31 - "LayoutConfigDto"
Cohesion: 0.28
Nodes (8): UpdateMenuLayoutCommand, UpdateMenuLayoutCommandHandler, LayoutConfigDto, BackgroundColor, BackgroundUrl, CanvasEnabled, Elements, UpdateMenuLayoutValidatorTests

### Community 32 - ".ProcessOrderMessageAsync"
Cohesion: 0.25
Nodes (7): Constitution Check, Phase 4: User Story 3 - Real-Time Kitchen Dispatch via WebSockets (Priority: P3), IKitchenNotifier, OrderCreatedNotification, IOrderQueueService, OrderProcessorWorker, KitchenWebSocketManager

### Community 33 - "Tenant"
Cohesion: 0.11
Nodes (12): Tenant, BrandingConfig, Categories, CurrentVersionHash, CustomDomain, LayoutConfig, MenuItems, MenuPublishJobs (+4 more)

### Community 34 - "ITenantContext"
Cohesion: 0.14
Nodes (12): 1. Summary, 2. Technical Context, 3. Constitution Check, 4. Project Structure, 5. Implementation Phases, Documentation (this feature), Implementation Plan: Menu Item CRUD, Soft Delete with 7-Day Retention, and Purge Worker, Source Code Changes Planned (+4 more)

### Community 36 - "RestoCore.IntegrationTests.csproj"
Cohesion: 0.18
Nodes (10): Testcontainers.Redis (4.14.0), net10.0, coverlet.collector (6.0.4), FluentAssertions (8.10.0), Microsoft.AspNetCore.Mvc.Testing (10.0.11), Microsoft.NET.Test.Sdk (17.14.1), Testcontainers.PostgreSql (4.14.0), xunit (2.9.3) (+2 more)

### Community 39 - "CanvasElement"
Cohesion: 0.25
Nodes (7): CanvasElement, DishId, Height, Width, X, Y, ZIndex

### Community 40 - "RestoCore.Application.csproj"
Cohesion: 0.22
Nodes (7): FluentValidation (12.1.1), Microsoft.EntityFrameworkCore (10.0.11), Microsoft.Extensions.Caching.Abstractions (10.0.11), net10.0, Microsoft.NET.Sdk, net10.0, Microsoft.NET.Sdk

### Community 41 - "AuthEndpoints.cs"
Cohesion: 0.33
Nodes (5): AuthEndpoints, LoginRequest, LoginResponse, SeededUser, UserProfileResponse

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
Cohesion: 0.18
Nodes (8): Category, Description, DisplayOrder, IsActive, Items, Name, Tenant, TenantId

### Community 47 - "DeleteMenuItemCommand"
Cohesion: 0.32
Nodes (7): Implementation for User Story 2, Phase 4: User Story 2 - Soft Delete with 7-Day Retention & Friendly Response (Priority: P2), Tests for User Story 2, DeleteMenuItemCommand, DeleteMenuItemCommandHandler, MenuItemArchiveResponse, DeleteMenuItemCommandHandlerTests

### Community 48 - "create-new-feature.ps1"
Cohesion: 0.46
Nodes (7): ConvertTo-CleanBranchName(), Get-BranchName(), Get-HighestNumberFromBranches(), Get-HighestNumberFromNames(), Get-HighestNumberFromRemoteRefs(), Get-HighestNumberFromSpecs(), Get-NextBranchNumber()

### Community 49 - "CanvasElementDto"
Cohesion: 0.17
Nodes (8): CanvasElementDto, DishId, Height, Width, X, Y, ZIndex, PublicMenuCanvasTests

### Community 50 - "2. Testing Endpoints via cURL or REST Client"
Cohesion: 0.29
Nodes (6): 1. Running Automated Tests, 2. Testing Endpoints via cURL or REST Client, A. Modifying a Dish (PUT), B. Soft Deleting a Dish (DELETE), C. Restoring an Archived Dish (POST), Quickstart: Menu Item CRUD, Soft Delete with 7-Day Retention, and Purge Worker

### Community 51 - "RestoCore.Api.csproj"
Cohesion: 0.29
Nodes (6): Microsoft.AspNetCore.Authentication.JwtBearer (10.0.11), Microsoft.AspNetCore.OpenApi (10.0.11), Swashbuckle.AspNetCore (10.2.3), Microsoft.NET.Sdk.Web, net10.0, Microsoft.EntityFrameworkCore.Design (10.0.11)

### Community 52 - "IKitchenRepository"
Cohesion: 0.29
Nodes (12): 2. Research Decisions, Decision 1: Scope and Granularity of Repositories, Decision 2: Unit of Work Implementation Pattern, Decision 3: Optimistic Concurrency Control & Retry Strategy, Decision 4: Querying (CQRS Query Handlers), Implementation for User Story 1, Phase 2: User Story 1 - Kitchen Availability Updates with Concurrency Resilience (Priority: P1) 🎯 MVP, Tests for User Story 1 (+4 more)

### Community 53 - "SubmitOrderCommand.cs"
Cohesion: 0.26
Nodes (8): OrderIngestionPayload, OrderItemInput, OrderStreamMessage, SubmitOrderCommand, SubmitOrderCommandHandler, SubmitOrderItemDto, SubmitOrderResponse, SubmitOrderCommandHandlerTests

### Community 54 - "MenuPublishJob"
Cohesion: 0.12
Nodes (16): User Scenarios & Testing *(mandatory)*, User Story 1 - Kitchen Availability Updates with Concurrency Resilience (Priority: P1), User Story 2 - Catalog Management via Isolated Catalog Repository & Unit of Work (Priority: P2), User Story 3 - Canvas Layout Compilation & Job State Management (Priority: P3), MenuPublishJob, AssetsQueued, CdnPurgeRequested, CompletedAt (+8 more)

### Community 55 - "PurgeArchivedDishesWorker"
Cohesion: 0.16
Nodes (10): Dependencies & Execution Order, Implementation for User Story 4, Parallel Execution Opportunities, Phase 1: Setup (Shared Infrastructure), Phase 6: User Story 4 - Background Purge Worker (Priority: P4), Phase 7: Polish & Cross-Cutting Concerns, Tasks: Menu Item CRUD, Soft Delete with 7-Day Retention, and Purge Worker, Tests for User Story 4 (+2 more)

### Community 57 - "Speckit SDD Workflow"
Cohesion: 0.50
Nodes (4): Plan Template Structure, Spec Template Structure, Tasks Template Structure, Speckit SDD Workflow

### Community 59 - "1.2 `ICatalogRepository`"
Cohesion: 0.20
Nodes (9): 1.1 `IUnitOfWork`, 1.2 `ICatalogRepository`, 1.3 `IKitchenRepository`, 1.4 `IMenuLayoutRepository`, 1.5 `ITenantRepository`, 1. Domain Interface Contracts, 2. Infrastructure Entity Framework Implementations, 3. Concurrency Strategy: Retry with Exponential Backoff (+1 more)

### Community 60 - "2. Flujo de Ejecucion"
Cohesion: 0.25
Nodes (7): 1. Activacion y Entradas del Usuario, 2. Flujo de Ejecucion, 3. Ejemplo de Salida y Reporte, Paso 1: Deteccion y Comparacion de Artefactos, Paso 2: Determinacion de Gaps Prioritarios, Paso 3: Generacion de Especificacion SDD, Sync Docs to Backend Specs (`sync-docs-specs`)

### Community 70 - "Feature Specification: Asynchronous Order Processing with FIFO Queue and WebSockets"
Cohesion: 0.17
Nodes (11): 1. Context & Business Need, 2. User Scenarios & Testing *(mandatory)*, 3. Requirements *(mandatory)*, 4. Success Criteria *(mandatory)*, 5. Assumptions & Constraints, Feature Specification: Asynchronous Order Processing with FIFO Queue and WebSockets, Functional Requirements, Measurable Outcomes (+3 more)

### Community 71 - "Fact"
Cohesion: 0.23
Nodes (3): SwaggerEndpointTests, AspireDashboardReadinessTests, ContainerReadinessTests

### Community 72 - "ApplicationDbContext"
Cohesion: 0.11
Nodes (13): 1. Context and Problem Statement, 3. Best Practices Alignment, Phase 0 Research: Repositories, Unit of Work, and Concurrency Control, ApplicationDbContext, Categories, CurrentTenantId, MenuItems, MenuPublishJobs (+5 more)

### Community 74 - "BrandingConfig"
Cohesion: 0.19
Nodes (11): UpdateBrandingCommand, UpdateBrandingCommandHandler, BrandingConfig, BackgroundColor, CoverBannerUrl, FontFamily, LayoutMode, LogoUrl (+3 more)

### Community 75 - ".Handle"
Cohesion: 0.67
Nodes (4): AddModifierGroupCommand, ModifierGroupResponse, ModifierOptionInput, ModifierOptionResponse

### Community 76 - "1. Resolved Clarifications & Key Technical Decisions"
Cohesion: 0.33
Nodes (6): 1. Resolved Clarifications & Key Technical Decisions, D-01: Soft Delete Implementation in EF Core, D-02: Active Kitchen Orders Verification Before Soft Deletion, D-03: Human-Friendly Response for Soft Delete (`MenuItemArchiveResponse`), D-04: Restoration within 7-Day Window (`POST /api/v1/admin/items/{id}/restore`), D-05: Scheduled Purge Worker (`PurgeArchivedDishesWorker`)

### Community 77 - "Implementation Plan: Asynchronous Order Processing with FIFO Queue and WebSockets"
Cohesion: 0.40
Nodes (4): Implementation Plan: Asynchronous Order Processing with FIFO Queue and WebSockets, Project Structure, Summary, Technical Context

### Community 79 - "ModifierOption"
Cohesion: 0.12
Nodes (13): BaseEntity, CreatedAt, Id, UpdatedAt, ITenantScopedEntity, TenantId, ModifierOption, IsAvailable (+5 more)

### Community 80 - "OrderItemSnapshot"
Cohesion: 0.18
Nodes (11): OrderItemSnapshot, Instructions, MenuItemId, Modifiers, Name, Quantity, UnitPrice, OrderModifierSnapshot (+3 more)

### Community 81 - "MenuConfiguration"
Cohesion: 0.27
Nodes (4): MenuConfiguration, MenuPublishJobConfiguration, TableConfiguration, TenantConfiguration

### Community 82 - "IStorageService"
Cohesion: 0.24
Nodes (3): Assumptions, IStorageService, SeaweedStorageService

### Community 83 - "CreateCategoryCommand"
Cohesion: 0.31
Nodes (3): CategoryResponse, CreateCategoryCommand, CreateCategoryCommandHandler

### Community 84 - "2. Contracts & Services"
Cohesion: 0.25
Nodes (7): 1.1 `Order` Entity, 1. Domain Entities & Value Objects, 2.1 `IOrderQueueService`, 2.2 `IKitchenNotifier` (WebSocket Connection Registry), 2.3 `IOrderRepository`, 2. Contracts & Services, Data Model & Interface Contracts: Asynchronous Order Processing & WebSockets

### Community 85 - "2. Research Decisions"
Cohesion: 0.25
Nodes (7): 1. Context and Problem Statement, 2. Research Decisions, Decision 1: Queue Technology: Redis Streams vs RabbitMQ vs PostgreSQL LISTEN/NOTIFY, Decision 2: Real-Time Protocol: Native ASP.NET Core WebSockets vs SignalR, Decision 3: Idempotency and Stream Acknowledgment, Decision 4: Entity Model and Comanda Snapshot Persistence, Phase 0 Research: Asynchronous Order Processing with Redis Streams and WebSockets

### Community 86 - "ICatalogRepository"
Cohesion: 0.14
Nodes (20): Constitution Check, Documentation (this feature), Implementation Plan: Repositories, Unit of Work, and Concurrency Control, Project Structure, Source Code, Summary, Technical Context, 1. Dependency Injection in Command Handlers (+12 more)

### Community 87 - ".AddApplicationMediator"
Cohesion: 0.29
Nodes (4): IMediator, ISender, Mediator, MediatorServiceCollectionExtensions

### Community 88 - ".Send"
Cohesion: 0.29
Nodes (3): AdminTableEndpoints, OrderEndpoints, PublicMenuEndpoints

### Community 89 - "RestoreMenuItemCommand"
Cohesion: 0.44
Nodes (6): Implementation for User Story 3, Phase 5: User Story 3 - Administrative Dish Restoration (Priority: P3), Tests for User Story 3, RestoreMenuItemCommand, RestoreMenuItemCommandHandler, RestoreMenuItemCommandHandlerTests

### Community 90 - "PresignedUploadResponse"
Cohesion: 0.36
Nodes (6): RestoCore.Application.Features.Storage.Commands, StorageEndpoints, PresignedUploadResponse, GeneratePresignedUploadCommand, GeneratePresignedUploadCommandHandler, GeneratePresignedUploadValidator

### Community 91 - "MenuItemResponse"
Cohesion: 0.22
Nodes (11): 2. Best Practices & Design Constraints, Research & Architectural Findings: Menu Item CRUD, Soft Delete with 7-Day Retention, and Purge Worker, Implementation for User Story 1, Phase 3: User Story 1 - Dish Modification & Relational Validation (Priority: P1) 🎯 MVP, Tests for User Story 1, MenuItemResponse, PatchMenuItemCommand, PatchMenuItemCommandHandler (+3 more)

### Community 96 - "GetTableQrQueryHandler"
Cohesion: 0.43
Nodes (4): RestoCore.Application.Features.QrCodes.Queries, GetTableQrQuery, GetTableQrQueryHandler, QrResult

### Community 98 - "RedisOrderQueueService"
Cohesion: 0.33
Nodes (5): Phase 2: User Story 1 - Client Order Ingestion (Priority: P1) 🎯 MVP, Phase 3: User Story 2 - Sequential FIFO Background Worker (Priority: P2), Phase 5: Verification, Regressions & Graphify, Tasks: Asynchronous Order Processing with FIFO Queue and WebSockets, RedisOrderQueueService

### Community 99 - "IRequest"
Cohesion: 0.33
Nodes (6): AdminTenantEndpoints, IRequest, IRequestHandler, ProvisionTenantCommand, ProvisionTenantCommandHandler, TenantSummaryResponse

### Community 102 - "LayoutConfig"
Cohesion: 0.33
Nodes (5): LayoutConfig, BackgroundColor, BackgroundUrl, CanvasEnabled, Elements

### Community 103 - "MenuCompilationResult"
Cohesion: 0.50
Nodes (4): MenuCompilationResult, AssetsQueued, CompiledLayout, VersionHash

## Knowledge Gaps
- **288 isolated node(s):** `net10.0`, `Microsoft.AspNetCore.Authentication.JwtBearer (10.0.11)`, `Microsoft.AspNetCore.OpenApi (10.0.11)`, `Microsoft.EntityFrameworkCore.Design (10.0.11)`, `Swashbuckle.AspNetCore (10.2.3)` (+283 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 472 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **24 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `MenuItem` connect `MenuItem` to `.CompleteLifecycle_TenantToPublicMenuAndQr_SucceedsEndToEnd`, `DateTimeOffset`, `IApplicationDbContext`, `ModifierGroup`, `MenuPublishJobResponse`, `4. Application Models & Transfer Objects (DTOs)`, `RestoCore.Domain.Entities`, `1. Mapa de Fuentes en `RestoCore-Docs``, `Tenant`, `Guid`, `Task`, `Category`, `DeleteMenuItemCommand`, `IKitchenRepository`, `PurgeArchivedDishesWorker`, `1.2 `ICatalogRepository``, `ApplicationDbContext`, `1. Resolved Clarifications & Key Technical Decisions`, `ModifierOption`, `MenuConfiguration`, `RestoreMenuItemCommand`, `MenuItemResponse`, `List`, `ToggleItemAvailabilityCommand`?**
  _High betweenness centrality (0.071) - this node is a cross-community bridge._
- **Are the 28 inferred relationships involving `MenuItem` (e.g. with `1. Mapa de Fuentes en `RestoCore-Docs`` and `1.2 `ICatalogRepository``) actually correct?**
  _`MenuItem` has 28 INFERRED edges - model-reasoned connections that need verification._
- **What connects `net10.0`, `Microsoft.AspNetCore.Authentication.JwtBearer (10.0.11)`, `Microsoft.AspNetCore.OpenApi (10.0.11)` to the rest of the system?**
  _288 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `PublicMenuResponse` be split into smaller, more focused modules?**
  _Cohesion score 0.05272108843537415 - nodes in this community are weakly interconnected._
- **Why does `ITenantContext` connect `ITenantContext` to `GetTableQrQueryHandler`, `ILogger`, `Guid`, `.CompleteLifecycle_TenantToPublicMenuAndQr_SucceedsEndToEnd`, `TenantContext`, `OpaInput`, `ApplicationDbContext`, `BrandingConfig`, `DeleteMenuItemCommand`, `CreateCategoryCommand`, `MenuPublishJobResponse`, `ICatalogRepository`, `RestoreMenuItemCommand`, `PresignedUploadResponse`, `MenuItemResponse`, `LayoutConfigDto`?**
  _High betweenness centrality (0.047) - this node is a cross-community bridge._
- **Should `Structured Logging & Traces Specification` be split into smaller, more focused modules?**
  _Cohesion score 0.05102040816326531 - nodes in this community are weakly interconnected._
- **Why does `ApplicationDbContext` connect `ApplicationDbContext` to `.CompleteLifecycle_TenantToPublicMenuAndQr_SucceedsEndToEnd`, `Order`, `DateTimeOffset`, `TenantContext`, `IApplicationDbContext`, `ModifierGroup`, `MenuPublishJobResponse`, `Tasks: Repositories, Unit of Work, and Concurrency Control`, `Table`, `MenuItem`, `RestoCore.Domain.Entities`, `.ProcessOrderMessageAsync`, `Tenant`, `ITenantContext`, `Guid`, `Task`, `Category`, `IKitchenRepository`, `MenuPublishJob`, `1.2 `ICatalogRepository``, `1. Resolved Clarifications & Key Technical Decisions`, `ModifierOption`, `ILogger`?**
  _High betweenness centrality (0.047) - this node is a cross-community bridge._