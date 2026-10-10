# Graph Report - resto-core-back  (2026-10-10)

## Corpus Check
- 212 files · ~81,603 words
- Verdict: corpus is large enough that graph structure adds value.
- Unclassified: 4 file(s) not represented in the graph (top: (none) 2, .example 1, .rego 1)

## Summary
- 1211 nodes · 2770 edges · 88 communities (70 shown, 18 thin omitted)
- Extraction: 87% EXTRACTED · 13% INFERRED · 0% AMBIGUOUS · INFERRED: 369 edges (avg confidence: 0.88)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `41738b1b`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- PublicMenuResponse
- Structured Logging & Traces Specification
- Tenant Entity
- ContainerizedStackFixture
- Order
- DateTimeOffset
- RestoCore Backend Constitution
- ITenantContext
- OpaInput
- Program.cs
- ApplicationDbContext
- fluentassertions
- RestoCore.Application.Common.Interfaces
- IApplicationDbContext
- PresignedUploadResponse
- ModifierOption
- ModifierGroup
- common.ps1
- http
- MenuPublishJobResponse
- ContainerizedStackFixture.cs
- .AddApplicationMediator
- RestoCore.Infrastructure.csproj
- app.py
- Task
- LayoutConfigDto
- MenuItem
- RestoCore.Domain.Entities
- CanvasElementDto
- MenuConfiguration
- CreateMenuItemCommand
- RestoCore.Application.Common.Mediator
- IOrderQueueService
- Tenant
- IRequestHandler
- IRequest
- RestoCore.IntegrationTests.csproj
- ICatalogRepository
- Table
- AuthEndpoints.cs
- RestoCore.Application.csproj
- GetTableQrQueryHandler
- ILogger
- RestoCore.SecurityTests.csproj
- RestoCore.UnitTests.csproj
- ref_k6
- Category
- Implementation Plan: Repositories, Unit of Work, and Concurrency Control
- create-new-feature.ps1
- LayoutConfig
- .Handle
- RestoCore.Api.csproj
- .ComputeVersionHash
- IKitchenRepository
- MenuPublishJob
- ICdnPurgeService
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
- OpenTelemetryExtensions
- RestoCore.Infrastructure.Persistence
- Guid
- BaseEntity
- OrderProcessorWorker
- ProvisionTenantCommand.cs
- IMenuLayoutRepository
- OrderItemSnapshot
- KitchenWebSocketManager
- Decision 1: Scope and Granularity of Repositories
- ContainerReadinessTests.cs
- 2. Contracts & Services
- 2. Research Decisions
- Tasks: Repositories, Unit of Work, and Concurrency Control
- Developer Quickstart: Using Repositories & Unit of Work in RestoCore

## God Nodes (most connected - your core abstractions)
1. `MenuItem` - 49 edges
2. `ApplicationDbContext` - 44 edges
3. `Tenant` - 43 edges
4. `RestoCore.Domain.Entities` - 41 edges
5. `RestoCore.Application.Common.Interfaces` - 40 edges
6. `Category` - 35 edges
7. `Order` - 35 edges
8. `IApplicationDbContext` - 32 edges
9. `ICatalogRepository` - 30 edges
10. `LayoutConfigDto` - 30 edges

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

## Communities (88 total, 18 thin omitted)

### Community 0 - "PublicMenuResponse"
Cohesion: 0.05
Nodes (46): PublicBrandingDto, BackgroundColor, CoverBannerUrl, FontFamily, LayoutMode, LogoUrl, PrimaryColor, SecondaryColor (+38 more)

### Community 1 - "Structured Logging & Traces Specification"
Cohesion: 0.05
Nodes (42): k6 Aspire Swagger Requirements Checklist, Observability and Swagger API Contract, Swagger OpenAPI Explorer Endpoints, k6 & Aspire Quickstart Guide, k6 Performance Testing Scenarios, k6 Aspire Swagger Specification, US1: Interactive API Exploration, US2: Real-time Telemetry Dashboard (+34 more)

### Community 2 - "Tenant Entity"
Cohesion: 0.08
Nodes (24): Digital Menu Quality Checklist, Digital Menu OpenAPI Specification, Category Entity, MenuItem Entity, ModifierGroup Entity, ModifierOption Entity, Table Entity, Tenant Entity (+16 more)

### Community 3 - "ContainerizedStackFixture"
Cohesion: 0.11
Nodes (10): Program, AdminMenuLayoutEndpointsTests, FullLifecycleE2ETests, MenuPublishEndpointsTests, PublicMenuCanvasEndpointsTests, SwaggerEndpointTests, ContainerizedStackFixture, TestAuthHandler (+2 more)

### Community 4 - "Order"
Cohesion: 0.12
Nodes (18): Constitution Check, Phase 1: Setup & Data Contracts, IOrderRepository, Order, CompletedAt, CreatedAt, CustomerNotes, Id (+10 more)

### Community 5 - "DateTimeOffset"
Cohesion: 0.19
Nodes (3): InitialCreate, AddCanvasLayoutAndMenuPublishJobs, ApplicationDbContextModelSnapshot

### Community 6 - "RestoCore Backend Constitution"
Cohesion: 0.09
Nodes (20): AI Agents Operating Guidelines, Skill: Git Commit, Skill: Review TODOs, Skill: Spec-Kit Analyze, Skill: Spec-Kit Clarify, Skill: Spec-Kit Constitution, Skill: Spec-Kit Git Commit, Skill: Spec-Kit Git Remote (+12 more)

### Community 7 - "ITenantContext"
Cohesion: 0.12
Nodes (10): ITenantContext, HasTenant, TenantId, TenantSlug, TenantContext, TenantId, TenantSlug, TenantResolutionMiddleware (+2 more)

### Community 8 - "OpaInput"
Cohesion: 0.10
Nodes (18): OpaAuthorizationHandler, OpaRequirement, Action, ResourceType, IOpaClient, OpaInput, Action, Method (+10 more)

### Community 9 - "Program.cs"
Cohesion: 0.07
Nodes (10): RestoCore.Application.Features.MenuLayout.Commands, RestoCore.Application.Features.PublicMenu.Queries, RestoCore.Infrastructure.MultiTenancy, RestoCore.Infrastructure.Qr, RestoCore.Infrastructure.Storage, RestoCore.Api.Middlewares, RestoCore.Infrastructure.Cdn, RestoCore.UnitTests.MultiTenancy (+2 more)

### Community 10 - "ApplicationDbContext"
Cohesion: 0.14
Nodes (10): ApplicationDbContext, Categories, CurrentTenantId, MenuItems, MenuPublishJobs, ModifierGroups, ModifierOptions, Orders (+2 more)

### Community 11 - "fluentassertions"
Cohesion: 0.17
Nodes (10): RestoCore.UnitTests.Features.MenuLayout, RestoCore.Infrastructure.Services.Compilation, RestoCore.UnitTests.Features.PublicMenu, RestoCore.Application.Features.PublicMenu.DTOs, RestoCore.IntegrationTests.Endpoints, RestoCore.UnitTests.PublicMenu, RestoCore.IntegrationTests.Fixtures, RestoCore.IntegrationTests.Storage (+2 more)

### Community 12 - "RestoCore.Application.Common.Interfaces"
Cohesion: 0.15
Nodes (7): RestoCore.Application.Common.Interfaces, RestoCore.UnitTests.Features.Orders, RestoCore.Application.Features.MenuLayout.Services, RestoCore.Infrastructure.WebSockets, RestoCore.Infrastructure.BackgroundServices, RestoCore.Application.Features.Orders.Commands, RestoCore.Infrastructure.Messaging

### Community 13 - "IApplicationDbContext"
Cohesion: 0.14
Nodes (15): Assumptions, Background & Architectural Drivers, Edge Cases, Feature Specification: Repositories and Unit of Work for Concurrency and Persistence Decoupling, Measurable Outcomes, Requirements *(mandatory)*, Success Criteria *(mandatory)*, IApplicationDbContext (+7 more)

### Community 14 - "PresignedUploadResponse"
Cohesion: 0.28
Nodes (7): RestoCore.Application.Features.Storage.Commands, IStorageService, PresignedUploadResponse, GeneratePresignedUploadCommand, GeneratePresignedUploadCommandHandler, GeneratePresignedUploadValidator, SeaweedStorageService

### Community 15 - "ModifierOption"
Cohesion: 0.12
Nodes (10): RestoCore.Domain.Common, ITenantScopedEntity, TenantId, ModifierOption, IsAvailable, ModifierGroup, ModifierGroupId, Name (+2 more)

### Community 16 - "ModifierGroup"
Cohesion: 0.16
Nodes (9): ModifierGroup, IsRequired, MaxSelection, MenuItem, MenuItemId, MinSelection, Name, Options (+1 more)

### Community 17 - "common.ps1"
Cohesion: 0.23
Nodes (11): Find-FeatureDirByPrefix(), Find-SpecifyRoot(), Get-CurrentBranch(), Get-FeatureDirFromBranchPrefixOrExit(), Get-FeaturePathsEnv(), Get-Python3Command(), Get-RepoRoot(), Get-SpecKitEffectiveBranchName() (+3 more)

### Community 18 - "http"
Cohesion: 0.13
Nodes (15): ASPNETCORE_ENVIRONMENT, applicationUrl, commandName, dotnetRunMessages, environmentVariables, launchBrowser, applicationUrl, commandName (+7 more)

### Community 19 - "MenuPublishJobResponse"
Cohesion: 0.13
Nodes (14): AdminMenuLayoutEndpoints, PublishMenuCommand, PublishMenuCommandHandler, MenuPublishJobResponse, AssetsQueued, CdnPurgeRequested, EstimatedDurationSeconds, JobId (+6 more)

### Community 21 - ".AddApplicationMediator"
Cohesion: 0.33
Nodes (4): IMediator, ISender, Mediator, MediatorServiceCollectionExtensions

### Community 22 - "RestoCore.Infrastructure.csproj"
Cohesion: 0.13
Nodes (14): AWSSDK.S3 (4.0.102.5), Microsoft.Extensions.Caching.StackExchangeRedis (10.0.12), Npgsql.EntityFrameworkCore.PostgreSQL (10.0.3), Npgsql.OpenTelemetry (10.0.3), OpenTelemetry.Exporter.OpenTelemetryProtocol (1.18.0), OpenTelemetry.Extensions.Hosting (1.18.0), OpenTelemetry.Instrumentation.AspNetCore (1.18.0), OpenTelemetry.Instrumentation.Http (1.18.0) (+6 more)

### Community 24 - "Task"
Cohesion: 0.12
Nodes (3): MenuItemResponse, CatalogRepository, UnitOfWork

### Community 25 - "LayoutConfigDto"
Cohesion: 0.19
Nodes (12): UpdateMenuLayoutCommand, UpdateMenuLayoutCommandHandler, LayoutConfigDto, BackgroundColor, BackgroundUrl, CanvasEnabled, Elements, MenuCompilationResult (+4 more)

### Community 26 - "MenuItem"
Cohesion: 0.11
Nodes (15): MenuItem, Allergens, BasePrice, Category, CategoryId, Description, DietaryLabels, DisplayOrder (+7 more)

### Community 27 - "RestoCore.Domain.Entities"
Cohesion: 0.21
Nodes (6): RestoCore.Domain.Entities, RestoCore.UnitTests.Features.Kitchen, RestoCore.SecurityTests.MultiTenancy, RestoCore.Application.Common.Interfaces.Repositories, RestoCore.Infrastructure.Persistence.Repositories, RestoCore.Application.Features.Kitchen.Commands

### Community 28 - "CanvasElementDto"
Cohesion: 0.18
Nodes (10): RestoCore.Application.Features.MenuLayout.Validators, CanvasElementDto, DishId, Height, Width, X, Y, ZIndex (+2 more)

### Community 29 - "MenuConfiguration"
Cohesion: 0.18
Nodes (6): RestoCore.Infrastructure.Persistence.Configurations, MenuConfiguration, MenuPublishJobConfiguration, OrderConfiguration, TableConfiguration, TenantConfiguration

### Community 30 - "CreateMenuItemCommand"
Cohesion: 0.13
Nodes (12): RestoCore.Application.Features.Categories.Validators, RestoCore.UnitTests.Domain, RestoCore.Application.Features.MenuItems.Commands, RestoCore.Application.Features.MenuItems.Validators, RestoCore.Application.Features.Categories.Commands, CategoryResponse, CreateCategoryCommand, CreateCategoryCommandHandler (+4 more)

### Community 31 - "RestoCore.Application.Common.Mediator"
Cohesion: 0.24
Nodes (5): RestoCore.Application.Common.Mediator, RestoCore.Api.Endpoints, RestoCore.Application.Features.QrCodes.Queries, KitchenEndpoints, ToggleAvailabilityRequest

### Community 32 - "IOrderQueueService"
Cohesion: 0.21
Nodes (8): Phase 2: User Story 1 - Client Order Ingestion (Priority: P1) 🎯 MVP, IOrderQueueService, OrderStreamMessage, SubmitOrderCommand, SubmitOrderCommandHandler, SubmitOrderResponse, RedisOrderQueueService, SubmitOrderCommandHandlerTests

### Community 33 - "Tenant"
Cohesion: 0.14
Nodes (12): Tenant, BrandingConfig, Categories, CurrentVersionHash, CustomDomain, LayoutConfig, MenuItems, MenuPublishJobs (+4 more)

### Community 34 - "IRequestHandler"
Cohesion: 0.18
Nodes (12): IRequestHandler, UpdateBrandingCommand, UpdateBrandingCommandHandler, BrandingConfig, BackgroundColor, CoverBannerUrl, FontFamily, LayoutMode (+4 more)

### Community 35 - "IRequest"
Cohesion: 0.17
Nodes (6): AdminTableEndpoints, AdminTenantEndpoints, OrderEndpoints, PublicMenuEndpoints, StorageEndpoints, IRequest

### Community 36 - "RestoCore.IntegrationTests.csproj"
Cohesion: 0.18
Nodes (10): Testcontainers.Redis (4.14.0), net10.0, coverlet.collector (6.0.4), FluentAssertions (8.10.0), Microsoft.AspNetCore.Mvc.Testing (10.0.11), Microsoft.NET.Test.Sdk (17.14.1), Testcontainers.PostgreSql (4.14.0), xunit (2.9.3) (+2 more)

### Community 37 - "ICatalogRepository"
Cohesion: 0.38
Nodes (8): Summary, Functional Requirements, Implementation for User Story 2, Phase 3: User Story 2 - Catalog Management via Isolated Catalog Repository & Unit of Work (Priority: P2), Tests for User Story 2, IUnitOfWork, ICatalogRepository, CreateMenuItemCommandHandler

### Community 38 - "Table"
Cohesion: 0.13
Nodes (9): ITenantRepository, Table, IsActive, Label, TableNumber, Tenant, TenantId, Token (+1 more)

### Community 39 - "AuthEndpoints.cs"
Cohesion: 0.33
Nodes (5): AuthEndpoints, LoginRequest, LoginResponse, SeededUser, UserProfileResponse

### Community 40 - "RestoCore.Application.csproj"
Cohesion: 0.22
Nodes (7): FluentValidation (12.1.1), Microsoft.EntityFrameworkCore (10.0.11), Microsoft.Extensions.Caching.Abstractions (10.0.11), net10.0, Microsoft.NET.Sdk, net10.0, Microsoft.NET.Sdk

### Community 41 - "GetTableQrQueryHandler"
Cohesion: 0.24
Nodes (6): IQrCodeService, GetTableQrQuery, GetTableQrQueryHandler, QrResult, QrCodeService, QrCodeServiceTests

### Community 42 - "ILogger"
Cohesion: 0.33
Nodes (4): GlobalExceptionHandler, OpaClient, OpaResponse, Result

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

### Community 47 - "Implementation Plan: Repositories, Unit of Work, and Concurrency Control"
Cohesion: 0.29
Nodes (6): Constitution Check, Documentation (this feature), Implementation Plan: Repositories, Unit of Work, and Concurrency Control, Project Structure, Source Code, Technical Context

### Community 48 - "create-new-feature.ps1"
Cohesion: 0.46
Nodes (7): ConvertTo-CleanBranchName(), Get-BranchName(), Get-HighestNumberFromBranches(), Get-HighestNumberFromNames(), Get-HighestNumberFromRemoteRefs(), Get-HighestNumberFromSpecs(), Get-NextBranchNumber()

### Community 49 - "LayoutConfig"
Cohesion: 0.15
Nodes (12): CanvasElement, DishId, Height, Width, X, Y, ZIndex, LayoutConfig (+4 more)

### Community 50 - ".Handle"
Cohesion: 0.67
Nodes (5): AddModifierGroupCommand, AddModifierGroupCommandHandler, ModifierGroupResponse, ModifierOptionInput, ModifierOptionResponse

### Community 51 - "RestoCore.Api.csproj"
Cohesion: 0.29
Nodes (6): Microsoft.AspNetCore.Authentication.JwtBearer (10.0.11), Microsoft.AspNetCore.OpenApi (10.0.11), Swashbuckle.AspNetCore (10.2.3), Microsoft.NET.Sdk.Web, net10.0, Microsoft.EntityFrameworkCore.Design (10.0.11)

### Community 52 - ".ComputeVersionHash"
Cohesion: 0.33
Nodes (3): IMenuCompilationService, MenuCompilationService, MenuCompilationServiceTests

### Community 53 - "IKitchenRepository"
Cohesion: 0.22
Nodes (9): Implementation for User Story 1, Phase 2: User Story 1 - Kitchen Availability Updates with Concurrency Resilience (Priority: P1) 🎯 MVP, Tests for User Story 1, IKitchenRepository, ItemAvailabilityResponse, ToggleItemAvailabilityCommand, ToggleItemAvailabilityCommandHandler, KitchenRepository (+1 more)

### Community 54 - "MenuPublishJob"
Cohesion: 0.12
Nodes (16): User Scenarios & Testing *(mandatory)*, User Story 1 - Kitchen Availability Updates with Concurrency Resilience (Priority: P1), User Story 2 - Catalog Management via Isolated Catalog Repository & Unit of Work (Priority: P2), User Story 3 - Canvas Layout Compilation & Job State Management (Priority: P3), MenuPublishJob, AssetsQueued, CdnPurgeRequested, CompletedAt (+8 more)

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
Cohesion: 0.22
Nodes (3): AspireDashboardReadinessTests, ContainerReadinessTests, PublicMenuCanvasTests

### Community 75 - "Guid"
Cohesion: 0.27
Nodes (3): OrderIngestionPayload, OrderItemInput, SubmitOrderItemDto

### Community 76 - "BaseEntity"
Cohesion: 0.40
Nodes (4): BaseEntity, CreatedAt, Id, UpdatedAt

### Community 77 - "OrderProcessorWorker"
Cohesion: 0.17
Nodes (8): Implementation Plan: Asynchronous Order Processing with FIFO Queue and WebSockets, Project Structure, Summary, Technical Context, Phase 3: User Story 2 - Sequential FIFO Background Worker (Priority: P2), Phase 5: Verification, Regressions & Graphify, Tasks: Asynchronous Order Processing with FIFO Queue and WebSockets, OrderProcessorWorker

### Community 78 - "ProvisionTenantCommand.cs"
Cohesion: 0.25
Nodes (7): RestoCore.Application.Features.Tenants.Validators, RestoCore.Application.Features.Tenants.Commands, AdminCatalogEndpoints, ProvisionTenantCommand, ProvisionTenantCommandHandler, TenantSummaryResponse, ProvisionTenantValidator

### Community 79 - "IMenuLayoutRepository"
Cohesion: 0.29
Nodes (4): Implementation for User Story 3, Phase 4: User Story 3 - Canvas Layout Compilation & Tenant Repositories (Priority: P3), IMenuLayoutRepository, MenuLayoutRepository

### Community 80 - "OrderItemSnapshot"
Cohesion: 0.18
Nodes (11): OrderItemSnapshot, Instructions, MenuItemId, Modifiers, Name, Quantity, UnitPrice, OrderModifierSnapshot (+3 more)

### Community 81 - "KitchenWebSocketManager"
Cohesion: 0.31
Nodes (4): Phase 4: User Story 3 - Real-Time Kitchen Dispatch via WebSockets (Priority: P3), IKitchenNotifier, OrderCreatedNotification, KitchenWebSocketManager

### Community 82 - "Decision 1: Scope and Granularity of Repositories"
Cohesion: 0.22
Nodes (8): 1. Context and Problem Statement, 2. Research Decisions, 3. Best Practices Alignment, Decision 1: Scope and Granularity of Repositories, Decision 2: Unit of Work Implementation Pattern, Decision 3: Optimistic Concurrency Control & Retry Strategy, Decision 4: Querying (CQRS Query Handlers), Phase 0 Research: Repositories, Unit of Work, and Concurrency Control

### Community 84 - "2. Contracts & Services"
Cohesion: 0.25
Nodes (7): 1.1 `Order` Entity, 1. Domain Entities & Value Objects, 2.1 `IOrderQueueService`, 2.2 `IKitchenNotifier` (WebSocket Connection Registry), 2.3 `IOrderRepository`, 2. Contracts & Services, Data Model & Interface Contracts: Asynchronous Order Processing & WebSockets

### Community 85 - "2. Research Decisions"
Cohesion: 0.25
Nodes (7): 1. Context and Problem Statement, 2. Research Decisions, Decision 1: Queue Technology: Redis Streams vs RabbitMQ vs PostgreSQL LISTEN/NOTIFY, Decision 2: Real-Time Protocol: Native ASP.NET Core WebSockets vs SignalR, Decision 3: Idempotency and Stream Acknowledgment, Decision 4: Entity Model and Comanda Snapshot Persistence, Phase 0 Research: Asynchronous Order Processing with Redis Streams and WebSockets

### Community 86 - "Tasks: Repositories, Unit of Work, and Concurrency Control"
Cohesion: 0.33
Nodes (5): Dependencies & Execution Order, Format: `[ID] [P?] [Story] Description`, Phase 1: Setup & Foundational Contracts, Phase 5: Verification, Regressions & Graphify Re-evaluation, Tasks: Repositories, Unit of Work, and Concurrency Control

### Community 87 - "Developer Quickstart: Using Repositories & Unit of Work in RestoCore"
Cohesion: 0.40
Nodes (4): 1. Dependency Injection in Command Handlers, 2. Using Explicit Transactions, 3. High-Concurrency Item Toggling, Developer Quickstart: Using Repositories & Unit of Work in RestoCore

## Knowledge Gaps
- **286 isolated node(s):** `net10.0`, `Microsoft.AspNetCore.Authentication.JwtBearer (10.0.11)`, `Microsoft.AspNetCore.OpenApi (10.0.11)`, `Microsoft.EntityFrameworkCore.Design (10.0.11)`, `Swashbuckle.AspNetCore (10.2.3)` (+281 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 435 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **18 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `ApplicationDbContext` connect `ApplicationDbContext` to `Order`, `DateTimeOffset`, `ITenantContext`, `RestoCore.Application.Common.Interfaces`, `IApplicationDbContext`, `ModifierOption`, `ModifierGroup`, `MenuPublishJobResponse`, `Task`, `MenuItem`, `Tenant`, `ICatalogRepository`, `Table`, `ILogger`, `Category`, `LayoutConfig`, `IKitchenRepository`, `MenuPublishJob`, `1.2 `ICatalogRepository``, `Guid`, `IMenuLayoutRepository`, `Decision 1: Scope and Granularity of Repositories`, `Tasks: Repositories, Unit of Work, and Concurrency Control`?**
  _High betweenness centrality (0.077) - this node is a cross-community bridge._
- **Are the 12 inferred relationships involving `MenuItem` (e.g. with `1.2 `ICatalogRepository`` and `Decision 1: Scope and Granularity of Repositories`) actually correct?**
  _`MenuItem` has 12 INFERRED edges - model-reasoned connections that need verification._
- **What connects `net10.0`, `Microsoft.AspNetCore.Authentication.JwtBearer (10.0.11)`, `Microsoft.AspNetCore.OpenApi (10.0.11)` to the rest of the system?**
  _286 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `PublicMenuResponse` be split into smaller, more focused modules?**
  _Cohesion score 0.05272108843537415 - nodes in this community are weakly interconnected._
- **Why does `RestoCore.Application.Common.Interfaces` connect `RestoCore.Application.Common.Interfaces` to `IRequestHandler`, `ICatalogRepository`, `ITenantContext`, `Program.cs`, `GetTableQrQueryHandler`, `RestoCore.Infrastructure.Persistence`, `fluentassertions`, `PresignedUploadResponse`, `ProvisionTenantCommand.cs`, `.Handle`, `ContainerReadinessTests.cs`, `ContainerizedStackFixture.cs`, `RestoCore.Domain.Entities`, `CreateMenuItemCommand`, `RestoCore.Application.Common.Mediator`?**
  _High betweenness centrality (0.053) - this node is a cross-community bridge._
- **Should `Structured Logging & Traces Specification` be split into smaller, more focused modules?**
  _Cohesion score 0.05102040816326531 - nodes in this community are weakly interconnected._
- **Why does `Order` connect `Order` to `Tenant`, `DateTimeOffset`, `Feature Specification: Asynchronous Order Processing with FIFO Queue and WebSockets`, `Table`, `ICatalogRepository`, `ApplicationDbContext`, `Guid`, `RestoCore.Application.Common.Interfaces`, `OrderItemSnapshot`, `2. Contracts & Services`, `MenuConfiguration`?**
  _High betweenness centrality (0.041) - this node is a cross-community bridge._