# Graph Report - resto-core-back  (2026-10-15)

## Corpus Check
- 257 files · ~98,224 words
- Verdict: corpus is large enough that graph structure adds value.
- Unclassified: 4 file(s) not represented in the graph (top: (none) 2, .example 1, .rego 1)

## Summary
- 1540 nodes · 3800 edges · 106 communities (84 shown, 22 thin omitted)
- Extraction: 84% EXTRACTED · 16% INFERRED · 0% AMBIGUOUS · INFERRED: 621 edges (avg confidence: 0.89)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `21e761c8`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- PublicModifierGroupDto
- Structured Logging & Traces Specification
- Tenant Entity
- ContainerizedStackFixture
- Order
- PurgeArchivedDishesWorker
- RestoCore Backend Constitution
- UnitOfWork
- OpaInput
- Program.cs
- API Contracts: Menu Item CRUD, Soft Delete with 7-Day Retention, and Restore
- ContainerizedStackFixture.cs
- system
- IApplicationDbContext
- Feature Specification: Category ABMC & Restaurant Contact Profile Management
- CategoryResponse
- ModifierGroup
- common.ps1
- http
- MenuPublishJobResponse
- 4. Application Models & Transfer Objects (DTOs)
- 1. Category ABMC Endpoints (`Admin - Catalog`)
- RestoCore.Infrastructure.csproj
- app.py
- ITenantRepository
- LayoutConfigDto
- MenuItem
- RestoCore.Domain.Entities
- 1. Mapa de Fuentes en `RestoCore-Docs`
- RestoCore.Application.Common.Interfaces
- ContainerReadinessTests.cs
- IMenuLayoutRepository
- KitchenWebSocketManager
- Tenant
- StorageEndpoints.cs
- .ConfigureWebHost
- RestoCore.IntegrationTests.csproj
- 20260906001559_InitialCreate.Designer.cs
- Task
- LayoutConfig
- RestoCore.Application.csproj
- AuthEndpoints.cs
- GetTableQrQueryHandler
- RestoCore.SecurityTests.csproj
- RestoCore.UnitTests.csproj
- ref_k6
- Category
- IOrderRepository
- create-new-feature.ps1
- CanvasElementDto
- 2. Testing Endpoints via cURL or REST Client
- RestoCore.Api.csproj
- IKitchenRepository
- .ProcessOrderMessageAsync
- MenuPublishJob
- Tasks: Menu Item CRUD, Soft Delete with 7-Day Retention, and Purge Worker
- Speckit SDD Workflow
- Observability & Tracing Research
- 1.2 `ICatalogRepository`
- 2. Flujo de Ejecucion
- Skill: Spec-Kit Checklist
- Skill: Spec-Kit Git Feature
- Admin App Streamlit Stack
- Skill: Spec-Kit Git Initialize
- Vector and Raster QR Generation
- 3. Referential Integrity & Validation Rules
- Fact
- ApplicationDbContext
- BrandingConfig
- .Handle
- 1. Resolved Clarifications & Key Technical Decisions
- Table
- PublicBrandingDto
- ModifierOption
- PublicMenuResponse
- microsoft_entityframeworkcore
- PublicMenuItemDto
- AbstractValidator
- .AddApplicationMediator
- 2. Research Decisions
- Summary
- Tasks: Category ABMC & Restaurant Contact Profile Management
- .Send
- RestoreMenuItemCommand
- IStorageService
- UpdateMenuItemCommand
- Guid
- Quickstart & Verification Guide: Category ABMC & Restaurant Profile
- .Handle
- ICatalogRepository
- ITenantContext
- OpenTelemetryExtensions
- IRequestHandler
- IRequest
- GetPublicMenuQuery.cs
- MenuItemResponse
- MenuCompilationResult
- PublicCategoryDto

## God Nodes (most connected - your core abstractions)
1. `MenuItem` - 72 edges
2. `RestoCore.Application.Common.Interfaces` - 61 edges
3. `Tenant` - 57 edges
4. `ICatalogRepository` - 55 edges
5. `RestoCore.Domain.Entities` - 51 edges
6. `IUnitOfWork` - 50 edges
7. `RestoCore.Application.Common.Interfaces.Repositories` - 46 edges
8. `Category` - 45 edges
9. `ApplicationDbContext` - 45 edges
10. `ITenantContext` - 43 edges

## Surprising Connections (you probably didn't know these)
- `Phase 5: Verification, Regressions & Graphify Re-evaluation` --references--> `IApplicationDbContext`  [INFERRED]
  specs/006-repository-unitofwork-concurrency/tasks.md → src/RestoCore.Application/Common/Interfaces/IApplicationDbContext.cs
- `Decision` --references--> `ICdnPurgeService`  [INFERRED]
  specs/009-category-abmc-restaurant-profile/research.md → src/RestoCore.Application/Common/Interfaces/ICdnPurgeService.cs
- `1. Context & Business Need` --references--> `ICdnPurgeService`  [INFERRED]
  specs/009-category-abmc-restaurant-profile/spec.md → src/RestoCore.Application/Common/Interfaces/ICdnPurgeService.cs
- `2.2 `IKitchenNotifier` (WebSocket Connection Registry)` --references--> `IKitchenNotifier`  [INFERRED]
  specs/007-order-fifo-processing-websockets/data-model.md → src/RestoCore.Application/Common/Interfaces/IKitchenNotifier.cs
- `2.1 `IOrderQueueService`` --references--> `IOrderQueueService`  [INFERRED]
  specs/007-order-fifo-processing-websockets/data-model.md → src/RestoCore.Application/Common/Interfaces/IOrderQueueService.cs

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

## Communities (106 total, 22 thin omitted)

### Community 0 - "PublicModifierGroupDto"
Cohesion: 0.17
Nodes (12): PublicModifierGroupDto, Id, IsRequired, MaxSelection, MinSelection, Name, Options, PublicModifierOptionDto (+4 more)

### Community 1 - "Structured Logging & Traces Specification"
Cohesion: 0.05
Nodes (42): k6 Aspire Swagger Requirements Checklist, Observability and Swagger API Contract, Swagger OpenAPI Explorer Endpoints, k6 & Aspire Quickstart Guide, k6 Performance Testing Scenarios, k6 Aspire Swagger Specification, US1: Interactive API Exploration, US2: Real-time Telemetry Dashboard (+34 more)

### Community 2 - "Tenant Entity"
Cohesion: 0.08
Nodes (24): Digital Menu Quality Checklist, Digital Menu OpenAPI Specification, Category Entity, MenuItem Entity, ModifierGroup Entity, ModifierOption Entity, Table Entity, Tenant Entity (+16 more)

### Community 3 - "ContainerizedStackFixture"
Cohesion: 0.16
Nodes (8): Program, AdminMenuLayoutEndpointsTests, FullLifecycleE2ETests, PublicMenuCanvasEndpointsTests, SwaggerEndpointTests, ContainerizedStackFixture, CustomWebApplicationFactory, SeaweedStorageTests

### Community 4 - "Order"
Cohesion: 0.06
Nodes (33): 1.1 `Order` Entity, 1. Domain Entities & Value Objects, 2.1 `IOrderQueueService`, 2.2 `IKitchenNotifier` (WebSocket Connection Registry), 2.3 `IOrderRepository`, 2. Contracts & Services, Data Model & Interface Contracts: Asynchronous Order Processing & WebSockets, Order (+25 more)

### Community 5 - "PurgeArchivedDishesWorker"
Cohesion: 0.05
Nodes (40): Implementation Plan: Asynchronous Order Processing with FIFO Queue and WebSockets, Project Structure, Summary, Technical Context, 1. Context & Business Need, 2. User Scenarios & Testing *(mandatory)*, 3. Requirements *(mandatory)*, 4. Success Criteria *(mandatory)* (+32 more)

### Community 6 - "RestoCore Backend Constitution"
Cohesion: 0.09
Nodes (20): AI Agents Operating Guidelines, Skill: Git Commit, Skill: Review TODOs, Skill: Spec-Kit Analyze, Skill: Spec-Kit Clarify, Skill: Spec-Kit Constitution, Skill: Spec-Kit Git Commit, Skill: Spec-Kit Git Remote (+12 more)

### Community 7 - "UnitOfWork"
Cohesion: 0.08
Nodes (13): Dependencies & Execution Order, Format: `[ID] [P?] [Story] Description`, Phase 1: Setup & Foundational Contracts, Phase 5: Verification, Regressions & Graphify Re-evaluation, Tasks: Repositories, Unit of Work, and Concurrency Control, GlobalExceptionHandler, TenantContext, TenantId (+5 more)

### Community 8 - "OpaInput"
Cohesion: 0.09
Nodes (21): OpaAuthorizationHandler, OpaRequirement, Action, ResourceType, IOpaClient, OpaClient, OpaInput, Action (+13 more)

### Community 9 - "Program.cs"
Cohesion: 0.06
Nodes (11): RestoCore.Api.Endpoints, RestoCore.Infrastructure.MultiTenancy, RestoCore.Infrastructure.Qr, RestoCore.Infrastructure.Storage, RestoCore.Api.Middlewares, RestoCore.Infrastructure.Cdn, RestoCore.Application.Features.Orders.Commands, RestoCore.UnitTests.MultiTenancy (+3 more)

### Community 10 - "API Contracts: Menu Item CRUD, Soft Delete with 7-Day Retention, and Restore"
Cohesion: 0.14
Nodes (13): 1. Full Modification of Menu Item (`PUT /api/v1/admin/items/{id}`), 2. Partial Modification of Menu Item (`PATCH /api/v1/admin/items/{id}`), 3. Soft Delete Menu Item (`DELETE /api/v1/admin/items/{id}`), 4. Restore Menu Item (`POST /api/v1/admin/items/{id}/restore`), API Contracts: Menu Item CRUD, Soft Delete with 7-Day Retention, and Restore, Request, Request, Request (+5 more)

### Community 11 - "ContainerizedStackFixture.cs"
Cohesion: 0.13
Nodes (4): RestoCore.Infrastructure.Workers, RestoCore.IntegrationTests.Storage, RestoCore.Infrastructure.BackgroundServices, RestoCore.UnitTests.Workers

### Community 12 - "system"
Cohesion: 0.14
Nodes (10): RestoCore.UnitTests.Features.MenuLayout, RestoCore.Application.Features.MenuLayout.Commands, RestoCore.Infrastructure.Services.Compilation, RestoCore.Application.Features.MenuLayout.Services, RestoCore.IntegrationTests.Endpoints, RestoCore.IntegrationTests.Fixtures, RestoCore.Infrastructure.WebSockets, RestoCore.Application.Features.MenuLayout.DTOs (+2 more)

### Community 13 - "IApplicationDbContext"
Cohesion: 0.10
Nodes (19): Assumptions, Background & Architectural Drivers, Edge Cases, Feature Specification: Repositories and Unit of Work for Concurrency and Persistence Decoupling, Measurable Outcomes, Requirements *(mandatory)*, Success Criteria *(mandatory)*, User Scenarios & Testing *(mandatory)* (+11 more)

### Community 14 - "Feature Specification: Category ABMC & Restaurant Contact Profile Management"
Cohesion: 0.18
Nodes (10): 1. Context & Business Need, 2. User Scenarios & Testing *(mandatory)*, 3. Requirements *(mandatory)*, 4. Success Criteria *(mandatory)*, Feature Specification: Category ABMC & Restaurant Contact Profile Management, Functional Requirements, Key Entities, User Story 1 - Full Category ABMC with Referential Integrity Guardrail (Priority: P1) 🎯 MVP (+2 more)

### Community 15 - "CategoryResponse"
Cohesion: 0.16
Nodes (20): 2.1 Category ABMC Operations, Category Response DTO:, Commands:, Queries:, Decision, Implementation for User Story 1, Phase 3: User Story 1 - Full Category ABMC with Referential Integrity Guardrail (Priority: P1) 🎯 MVP, Tests for User Story 1 (+12 more)

### Community 16 - "ModifierGroup"
Cohesion: 0.16
Nodes (10): ModifierGroup, IsRequired, MaxSelection, MenuItem, MenuItemId, MinSelection, Name, Options (+2 more)

### Community 17 - "common.ps1"
Cohesion: 0.23
Nodes (11): Find-FeatureDirByPrefix(), Find-SpecifyRoot(), Get-CurrentBranch(), Get-FeatureDirFromBranchPrefixOrExit(), Get-FeaturePathsEnv(), Get-Python3Command(), Get-RepoRoot(), Get-SpecKitEffectiveBranchName() (+3 more)

### Community 18 - "http"
Cohesion: 0.13
Nodes (15): ASPNETCORE_ENVIRONMENT, applicationUrl, commandName, dotnetRunMessages, environmentVariables, launchBrowser, applicationUrl, commandName (+7 more)

### Community 19 - "MenuPublishJobResponse"
Cohesion: 0.15
Nodes (12): MenuPublishJobResponse, AssetsQueued, CdnPurgeRequested, EstimatedDurationSeconds, JobId, Status, TenantSlug, TriggeredAt (+4 more)

### Community 20 - "4. Application Models & Transfer Objects (DTOs)"
Cohesion: 0.15
Nodes (12): 1. Domain Entities & Value Objects, 2. PostgreSQL DDL Mapping & Indexes, 3. State Lifecycle & Transitions, 4. Application Models & Transfer Objects (DTOs), Data Model: Menu Item CRUD, Soft Delete with 7-Day Retention, and Purge Worker, `DeleteMenuItemCommand`, `MenuItem` (`RestoCore.Domain.Entities`), `MenuItemArchiveResponse` (+4 more)

### Community 21 - "1. Category ABMC Endpoints (`Admin - Catalog`)"
Cohesion: 0.12
Nodes (16): 1.1 List Categories, 1.2 Get Category by ID, 1.3 Create Category, 1.4 Full Update Category (PUT), 1.5 Partial Update Category (PATCH), 1.6 Delete Category (DELETE), 1. Category ABMC Endpoints (`Admin - Catalog`), 2.1 Get Restaurant Profile (+8 more)

### Community 22 - "RestoCore.Infrastructure.csproj"
Cohesion: 0.13
Nodes (14): AWSSDK.S3 (4.0.102.5), Microsoft.Extensions.Caching.StackExchangeRedis (10.0.12), Npgsql.EntityFrameworkCore.PostgreSQL (10.0.3), Npgsql.OpenTelemetry (10.0.3), OpenTelemetry.Exporter.OpenTelemetryProtocol (1.18.0), OpenTelemetry.Extensions.Hosting (1.18.0), OpenTelemetry.Instrumentation.AspNetCore (1.18.0), OpenTelemetry.Instrumentation.Http (1.18.0) (+6 more)

### Community 24 - "ITenantRepository"
Cohesion: 0.17
Nodes (5): ITenantRepository, ProvisionTenantCommand, ProvisionTenantCommandHandler, TenantSummaryResponse, TenantRepository

### Community 25 - "LayoutConfigDto"
Cohesion: 0.18
Nodes (10): UpdateMenuLayoutCommand, LayoutConfigDto, BackgroundColor, BackgroundUrl, CanvasEnabled, Elements, IMenuCompilationService, MenuCompilationService (+2 more)

### Community 26 - "MenuItem"
Cohesion: 0.11
Nodes (16): MenuItem, Allergens, BasePrice, Category, CategoryId, DeletedAt, Description, DietaryLabels (+8 more)

### Community 27 - "RestoCore.Domain.Entities"
Cohesion: 0.14
Nodes (12): RestoCore.UnitTests.Features.Categories, RestoCore.Domain.Entities, RestoCore.UnitTests.Features.Kitchen, RestoCore.UnitTests.Domain, RestoCore.UnitTests.Features.Orders, RestoCore.SecurityTests.MultiTenancy, RestoCore.Application.Common.Interfaces.Repositories, RestoCore.UnitTests.Features.PublicMenu (+4 more)

### Community 28 - "1. Mapa de Fuentes en `RestoCore-Docs`"
Cohesion: 0.22
Nodes (8): 1. Mapa de Fuentes en `RestoCore-Docs`, 2. Flujo de Ejecución Paso a Paso, 3. Ejemplo Práctico de Resolución, Consulta de Documentación Arquitectónica (`consult-docs`), Paso 1: Búsqueda Rápida de Referencias, Paso 2: Lectura Dirigida del Documento, Paso 3: Consulta en el Grafo de Graphify (Opcional), Paso 4: Aplicación y Citado de la Fuente

### Community 29 - "RestoCore.Application.Common.Interfaces"
Cohesion: 0.12
Nodes (10): RestoCore.Domain.ValueObjects, RestoCore.Application.Common.Interfaces, RestoCore.Application.Features.Tenants.Queries, RestoCore.Application.Common.Mediator, RestoCore.Application.Features.Tenants.Commands, RestoCore.Application.Features.MenuItems.DTOs, RestoCore.Application.Features.MenuItems.Commands, RestoCore.Application.Features.Categories.Queries (+2 more)

### Community 31 - "IMenuLayoutRepository"
Cohesion: 0.26
Nodes (4): Implementation for User Story 3, Phase 4: User Story 3 - Canvas Layout Compilation & Tenant Repositories (Priority: P3), IMenuLayoutRepository, MenuLayoutRepository

### Community 33 - "Tenant"
Cohesion: 0.09
Nodes (22): 1. Summary, 3. Constitution Check, 4. Project Structure, 5. Complexity Tracking, Documentation (this feature), Implementation Plan: Category ABMC & Restaurant Contact Profile Management, Source Code Changes Planned, Tenant (+14 more)

### Community 34 - "StorageEndpoints.cs"
Cohesion: 0.29
Nodes (3): RestoCore.Application.Features.Storage.Commands, RestoCore.Application.Features.QrCodes.Queries, StorageEndpoints

### Community 36 - "RestoCore.IntegrationTests.csproj"
Cohesion: 0.18
Nodes (10): Testcontainers.Redis (4.14.0), net10.0, coverlet.collector (6.0.4), FluentAssertions (8.10.0), Microsoft.AspNetCore.Mvc.Testing (10.0.11), Microsoft.NET.Test.Sdk (17.14.1), Testcontainers.PostgreSql (4.14.0), xunit (2.9.3) (+2 more)

### Community 37 - "20260906001559_InitialCreate.Designer.cs"
Cohesion: 0.19
Nodes (4): RestoCore.Infrastructure.Persistence.Migrations, InitialCreate, AddCanvasLayoutAndMenuPublishJobs, ApplicationDbContextModelSnapshot

### Community 39 - "LayoutConfig"
Cohesion: 0.13
Nodes (12): CanvasElement, DishId, Height, Width, X, Y, ZIndex, LayoutConfig (+4 more)

### Community 40 - "RestoCore.Application.csproj"
Cohesion: 0.22
Nodes (7): FluentValidation (12.1.1), Microsoft.EntityFrameworkCore (10.0.11), Microsoft.Extensions.Caching.Abstractions (10.0.11), net10.0, Microsoft.NET.Sdk, net10.0, Microsoft.NET.Sdk

### Community 41 - "AuthEndpoints.cs"
Cohesion: 0.33
Nodes (5): AuthEndpoints, LoginRequest, LoginResponse, SeededUser, UserProfileResponse

### Community 42 - "GetTableQrQueryHandler"
Cohesion: 0.24
Nodes (6): IQrCodeService, GetTableQrQuery, GetTableQrQueryHandler, QrResult, QrCodeService, QrCodeServiceTests

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

### Community 47 - "IOrderRepository"
Cohesion: 0.19
Nodes (11): Constitution Check, Phase 1: Setup & Data Contracts, Implementation for User Story 2, Phase 4: User Story 2 - Soft Delete with 7-Day Retention & Friendly Response (Priority: P2), Tests for User Story 2, IOrderRepository, DeleteMenuItemCommand, DeleteMenuItemCommandHandler (+3 more)

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
Cohesion: 0.21
Nodes (12): RestoCore.Application.Features.Kitchen.Commands, Implementation for User Story 1, Phase 2: User Story 1 - Kitchen Availability Updates with Concurrency Resilience (Priority: P1) 🎯 MVP, Tests for User Story 1, KitchenEndpoints, ToggleAvailabilityRequest, IKitchenRepository, ItemAvailabilityResponse (+4 more)

### Community 53 - ".ProcessOrderMessageAsync"
Cohesion: 0.13
Nodes (16): Phase 2: User Story 1 - Client Order Ingestion (Priority: P1) 🎯 MVP, Phase 3: User Story 2 - Sequential FIFO Background Worker (Priority: P2), Phase 4: User Story 3 - Real-Time Kitchen Dispatch via WebSockets (Priority: P3), Phase 5: Verification, Regressions & Graphify, Tasks: Asynchronous Order Processing with FIFO Queue and WebSockets, IKitchenNotifier, OrderCreatedNotification, IOrderQueueService (+8 more)

### Community 54 - "MenuPublishJob"
Cohesion: 0.12
Nodes (14): MenuPublishJob, AssetsQueued, CdnPurgeRequested, CompletedAt, ErrorMessage, EstimatedDurationSeconds, Status, Tenant (+6 more)

### Community 55 - "Tasks: Menu Item CRUD, Soft Delete with 7-Day Retention, and Purge Worker"
Cohesion: 0.33
Nodes (5): Dependencies & Execution Order, Parallel Execution Opportunities, Phase 1: Setup (Shared Infrastructure), Phase 7: Polish & Cross-Cutting Concerns, Tasks: Menu Item CRUD, Soft Delete with 7-Day Retention, and Purge Worker

### Community 57 - "Speckit SDD Workflow"
Cohesion: 0.50
Nodes (4): Plan Template Structure, Spec Template Structure, Tasks Template Structure, Speckit SDD Workflow

### Community 59 - "1.2 `ICatalogRepository`"
Cohesion: 0.20
Nodes (9): 1.1 `IUnitOfWork`, 1.2 `ICatalogRepository`, 1.3 `IKitchenRepository`, 1.4 `IMenuLayoutRepository`, 1.5 `ITenantRepository`, 1. Domain Interface Contracts, 2. Infrastructure Entity Framework Implementations, 3. Concurrency Strategy: Retry with Exponential Backoff (+1 more)

### Community 60 - "2. Flujo de Ejecucion"
Cohesion: 0.25
Nodes (7): 1. Activacion y Entradas del Usuario, 2. Flujo de Ejecucion, 3. Ejemplo de Salida y Reporte, Paso 1: Deteccion y Comparacion de Artefactos, Paso 2: Determinacion de Gaps Prioritarios, Paso 3: Generacion de Especificacion SDD, Sync Docs to Backend Specs (`sync-docs-specs`)

### Community 70 - "3. Referential Integrity & Validation Rules"
Cohesion: 0.18
Nodes (10): 1.1 Category Entity (`categories` table), 1.2 Tenant Entity Extension (`tenants` table), 1. Domain Entities & Database Schema Changes, 3.1 Referential Integrity Guardrail on Category Deletion, 3.2 Category Field Validation Rules, 3.3 Tenant Profile Field Validation Rules, 3. Referential Integrity & Validation Rules, Data Model: Category ABMC & Restaurant Contact Profile Management (+2 more)

### Community 71 - "Fact"
Cohesion: 0.16
Nodes (6): CreateMenuItemCommand, CreateMenuItemValidator, AspireDashboardReadinessTests, ContainerReadinessTests, ItemAvailabilityStateTests, MenuValidationTests

### Community 72 - "ApplicationDbContext"
Cohesion: 0.09
Nodes (18): 1. Context and Problem Statement, 2. Research Decisions, 3. Best Practices Alignment, Decision 1: Scope and Granularity of Repositories, Decision 2: Unit of Work Implementation Pattern, Decision 3: Optimistic Concurrency Control & Retry Strategy, Decision 4: Querying (CQRS Query Handlers), Phase 0 Research: Repositories, Unit of Work, and Concurrency Control (+10 more)

### Community 74 - "BrandingConfig"
Cohesion: 0.08
Nodes (27): 1. Category Referential Integrity Guardrail on Deletion, 2. Restaurant Profile Extension & Multi-Tenant Isolation, 3. Public Menu Exposure & Latency Budget Compliance, 4. Category ABMC Partial Update (PATCH) Semantics, 5. DTO Naming & Serialization Strategy, Alternatives Considered, Alternatives Considered, Architectural Research: Category ABMC & Restaurant Contact Profile Management (+19 more)

### Community 75 - ".Handle"
Cohesion: 0.67
Nodes (5): AddModifierGroupCommand, AddModifierGroupCommandHandler, ModifierGroupResponse, ModifierOptionInput, ModifierOptionResponse

### Community 76 - "1. Resolved Clarifications & Key Technical Decisions"
Cohesion: 0.22
Nodes (8): 1. Resolved Clarifications & Key Technical Decisions, 2. Best Practices & Design Constraints, D-01: Soft Delete Implementation in EF Core, D-02: Active Kitchen Orders Verification Before Soft Deletion, D-03: Human-Friendly Response for Soft Delete (`MenuItemArchiveResponse`), D-04: Restoration within 7-Day Window (`POST /api/v1/admin/items/{id}/restore`), D-05: Scheduled Purge Worker (`PurgeArchivedDishesWorker`), Research & Architectural Findings: Menu Item CRUD, Soft Delete with 7-Day Retention, and Purge Worker

### Community 77 - "Table"
Cohesion: 0.22
Nodes (8): Table, IsActive, Label, TableNumber, Tenant, TenantId, Token, TableConfiguration

### Community 78 - "PublicBrandingDto"
Cohesion: 0.18
Nodes (10): PublicBrandingDto, BackgroundColor, CoverBannerUrl, FontFamily, LayoutMode, LogoUrl, PrimaryColor, SecondaryColor (+2 more)

### Community 79 - "ModifierOption"
Cohesion: 0.10
Nodes (14): RestoCore.Domain.Common, BaseEntity, CreatedAt, Id, UpdatedAt, ITenantScopedEntity, TenantId, ModifierOption (+6 more)

### Community 80 - "PublicMenuResponse"
Cohesion: 0.18
Nodes (10): PublicMenuResponse, Address, Branding, BusinessHours, Categories, LayoutConfig, PhoneNumber, TableNumber (+2 more)

### Community 82 - "PublicMenuItemDto"
Cohesion: 0.20
Nodes (10): PublicMenuItemDto, Allergens, BasePrice, Description, DietaryLabels, Id, ImageUrl, IsAvailable (+2 more)

### Community 83 - "AbstractValidator"
Cohesion: 0.19
Nodes (8): RestoCore.Application.Features.Categories.Validators, RestoCore.Application.Features.Tenants.Validators, RestoCore.Application.Features.MenuLayout.Validators, RestoCore.Application.Features.MenuItems.Validators, CreateCategoryValidator, CanvasElementValidator, UpdateMenuLayoutValidator, ProvisionTenantValidator

### Community 84 - ".AddApplicationMediator"
Cohesion: 0.33
Nodes (4): IMediator, ISender, Mediator, MediatorServiceCollectionExtensions

### Community 85 - "2. Research Decisions"
Cohesion: 0.25
Nodes (7): 1. Context and Problem Statement, 2. Research Decisions, Decision 1: Queue Technology: Redis Streams vs RabbitMQ vs PostgreSQL LISTEN/NOTIFY, Decision 2: Real-Time Protocol: Native ASP.NET Core WebSockets vs SignalR, Decision 3: Idempotency and Stream Acknowledgment, Decision 4: Entity Model and Comanda Snapshot Persistence, Phase 0 Research: Asynchronous Order Processing with Redis Streams and WebSockets

### Community 86 - "Summary"
Cohesion: 0.25
Nodes (7): Constitution Check, Documentation (this feature), Implementation Plan: Repositories, Unit of Work, and Concurrency Control, Project Structure, Source Code, Summary, Technical Context

### Community 87 - "Tasks: Category ABMC & Restaurant Contact Profile Management"
Cohesion: 0.25
Nodes (7): Dependencies & Execution Order, Implementation Strategy, Parallel Execution Opportunities, Phase 1: Setup (Shared Infrastructure), Phase 2: Foundational (Blocking Prerequisites), Phase 6: Polish & Cross-Cutting Concerns, Tasks: Category ABMC & Restaurant Contact Profile Management

### Community 88 - ".Send"
Cohesion: 0.21
Nodes (5): AdminMenuLayoutEndpoints, AdminTableEndpoints, AdminTenantEndpoints, HealthEndpoints, PublicMenuEndpoints

### Community 89 - "RestoreMenuItemCommand"
Cohesion: 0.44
Nodes (5): Implementation for User Story 3, Phase 5: User Story 3 - Administrative Dish Restoration (Priority: P3), Tests for User Story 3, RestoreMenuItemCommand, RestoreMenuItemCommandHandler

### Community 90 - "IStorageService"
Cohesion: 0.32
Nodes (6): IStorageService, PresignedUploadResponse, GeneratePresignedUploadCommand, GeneratePresignedUploadCommandHandler, GeneratePresignedUploadValidator, SeaweedStorageService

### Community 91 - "UpdateMenuItemCommand"
Cohesion: 0.39
Nodes (5): Implementation for User Story 1, Phase 3: User Story 1 - Dish Modification & Relational Validation (Priority: P1) 🎯 MVP, Tests for User Story 1, UpdateMenuItemCommand, UpdateMenuItemCommandHandler

### Community 94 - "Quickstart & Verification Guide: Category ABMC & Restaurant Profile"
Cohesion: 0.29
Nodes (6): 1. Prerequisites, 2. Running Unit Tests, 3.1 Category ABMC Verification, 3.2 Restaurant Profile Management Verification, 3. Manual / API Verification with cURL, Quickstart & Verification Guide: Category ABMC & Restaurant Profile

### Community 95 - ".Handle"
Cohesion: 0.52
Nodes (5): Implementation for User Story 3, Phase 5: User Story 3 - Public Menu Exposure of Contact Profile (Priority: P3), Tests for User Story 3, GetPublicMenuQuery, GetPublicMenuQueryHandler

### Community 96 - "ICatalogRepository"
Cohesion: 0.19
Nodes (18): 1. Dependency Injection in Command Handlers, 2. Using Explicit Transactions, 3. High-Concurrency Item Toggling, Developer Quickstart: Using Repositories & Unit of Work in RestoCore, Functional Requirements, Implementation for User Story 2, Phase 3: User Story 2 - Catalog Management via Isolated Catalog Repository & Unit of Work (Priority: P2), Tests for User Story 2 (+10 more)

### Community 97 - "ITenantContext"
Cohesion: 0.21
Nodes (11): 2. Technical Context, Decision, ICdnPurgeService, ITenantContext, HasTenant, TenantId, TenantSlug, DeleteCategoryCommand (+3 more)

### Community 99 - "IRequestHandler"
Cohesion: 0.33
Nodes (5): IRequestHandler, PublishMenuCommand, PublishMenuCommandHandler, UpdateMenuLayoutCommandHandler, LocalDevelopmentCdnPurgeService

### Community 100 - "IRequest"
Cohesion: 0.21
Nodes (17): 2.2 Restaurant Profile Operations, 2.3 Public Menu Response Model Extension, 2. CQRS Commands, Queries & DTOs, Commands:, Queries:, Implementation for User Story 2, Phase 4: User Story 2 - Restaurant Profile Management (Priority: P2), Tests for User Story 2 (+9 more)

### Community 101 - "GetPublicMenuQuery.cs"
Cohesion: 0.40
Nodes (3): RestoCore.Application.Features.PublicMenu.Queries, RestoCore.Application.Features.PublicMenu.DTOs, RestoCore.UnitTests.PublicMenu

### Community 102 - "MenuItemResponse"
Cohesion: 0.90
Nodes (3): MenuItemResponse, PatchMenuItemCommand, PatchMenuItemCommandHandler

### Community 103 - "MenuCompilationResult"
Cohesion: 0.50
Nodes (4): MenuCompilationResult, AssetsQueued, CompiledLayout, VersionHash

### Community 104 - "PublicCategoryDto"
Cohesion: 0.40
Nodes (5): PublicCategoryDto, Description, Id, Items, Name

## Knowledge Gaps
- **294 isolated node(s):** `net10.0`, `Microsoft.AspNetCore.Authentication.JwtBearer (10.0.11)`, `Microsoft.AspNetCore.OpenApi (10.0.11)`, `Microsoft.EntityFrameworkCore.Design (10.0.11)`, `Swashbuckle.AspNetCore (10.2.3)` (+289 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 524 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **22 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `ApplicationDbContext` connect `ApplicationDbContext` to `ContainerizedStackFixture`, `Order`, `UnitOfWork`, `IApplicationDbContext`, `ModifierGroup`, `MenuPublishJobResponse`, `ITenantRepository`, `MenuItem`, `IMenuLayoutRepository`, `Tenant`, `Task`, `LayoutConfig`, `Category`, `IOrderRepository`, `IKitchenRepository`, `.ProcessOrderMessageAsync`, `MenuPublishJob`, `1.2 `ICatalogRepository``, `1. Resolved Clarifications & Key Technical Decisions`, `Table`, `ModifierOption`, `PublicMenuResponse`, `microsoft_entityframeworkcore`, `Guid`, `ITenantContext`, `IRequestHandler`?**
  _High betweenness centrality (0.062) - this node is a cross-community bridge._
- **Are the 28 inferred relationships involving `MenuItem` (e.g. with `1. Mapa de Fuentes en `RestoCore-Docs`` and `1.2 `ICatalogRepository``) actually correct?**
  _`MenuItem` has 28 INFERRED edges - model-reasoned connections that need verification._
- **What connects `net10.0`, `Microsoft.AspNetCore.Authentication.JwtBearer (10.0.11)`, `Microsoft.AspNetCore.OpenApi (10.0.11)` to the rest of the system?**
  _294 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Structured Logging & Traces Specification` be split into smaller, more focused modules?**
  _Cohesion score 0.05102040816326531 - nodes in this community are weakly interconnected._
- **Why does `Order` connect `Order` to `Tenant`, `PurgeArchivedDishesWorker`, `ApplicationDbContext`, `system`, `Table`, `IOrderRepository`, `.ProcessOrderMessageAsync`, `1. Mapa de Fuentes en `RestoCore-Docs``, `Guid`?**
  _High betweenness centrality (0.049) - this node is a cross-community bridge._
- **Should `Tenant Entity` be split into smaller, more focused modules?**
  _Cohesion score 0.08465608465608465 - nodes in this community are weakly interconnected._
- **Why does `ITenantContext` connect `ITenantContext` to `PurgeArchivedDishesWorker`, `UnitOfWork`, `OpaInput`, `CategoryResponse`, `1. Category ABMC Endpoints (`Admin - Catalog`)`, `GetTableQrQueryHandler`, `IOrderRepository`, `ApplicationDbContext`, `BrandingConfig`, `.Handle`, `PublicMenuResponse`, `RestoreMenuItemCommand`, `IStorageService`, `UpdateMenuItemCommand`, `Guid`, `ICatalogRepository`, `IRequestHandler`, `IRequest`, `MenuItemResponse`?**
  _High betweenness centrality (0.048) - this node is a cross-community bridge._