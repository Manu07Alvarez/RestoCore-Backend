# Graph Report - resto-core-back  (2026-10-10)

## Corpus Check
- 196 files · ~78,064 words
- Verdict: corpus is large enough that graph structure adds value.
- Unclassified: 4 file(s) not represented in the graph (top: (none) 2, .example 1, .rego 1)

## Summary
- 1089 nodes · 2405 edges · 77 communities (58 shown, 19 thin omitted)
- Extraction: 87% EXTRACTED · 13% INFERRED · 0% AMBIGUOUS · INFERRED: 317 edges (avg confidence: 0.88)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `41738b1b`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- PublicMenuItemDto
- Structured Logging & Traces Specification
- Tenant Entity
- Task
- OpaClient.cs
- DateTimeOffset
- RestoCore Backend Constitution
- Guid
- OpaInput
- RestoCore.Application.Common.Interfaces
- ApplicationDbContext
- fluentassertions
- MenuPublishEndpointsTests.cs
- IApplicationDbContext
- PresignedUploadResponse
- ModifierOption
- ModifierGroup
- common.ps1
- http
- MenuPublishJobResponse
- ContainerizedStackFixture.cs
- IRequest
- RestoCore.Infrastructure.csproj
- app.py
- CancellationToken
- UpdateMenuLayoutCommand
- MenuItem
- RestoCore.Domain.Entities
- CanvasElementDto
- MenuConfiguration
- CreateMenuItemCommand
- RestoCore.Application.Common.Mediator
- PublicModifierGroupDto
- Tenant
- BrandingConfig
- .Send
- RestoCore.IntegrationTests.csproj
- IUnitOfWork
- Table
- AuthEndpoints.cs
- RestoCore.Application.csproj
- GetTableQrQueryHandler
- .TryHandleAsync
- RestoCore.SecurityTests.csproj
- RestoCore.UnitTests.csproj
- ref_k6
- Category
- Implementation Plan: Repositories, Unit of Work, and Concurrency Control
- create-new-feature.ps1
- LayoutConfig
- .Handle
- RestoCore.Api.csproj
- LayoutConfigDto
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
- PublicBrandingDto
- OpenTelemetryExtensions
- StorageEndpoints.cs
- BaseEntity

## God Nodes (most connected - your core abstractions)
1. `MenuItem` - 49 edges
2. `Tenant` - 41 edges
3. `ApplicationDbContext` - 40 edges
4. `RestoCore.Domain.Entities` - 35 edges
5. `Category` - 35 edges
6. `RestoCore.Application.Common.Interfaces` - 33 edges
7. `IApplicationDbContext` - 32 edges
8. `LayoutConfigDto` - 30 edges
9. `MenuPublishJob` - 29 edges
10. `ModifierGroup` - 28 edges

## Surprising Connections (you probably didn't know these)
- `Phase 5: Verification, Regressions & Graphify Re-evaluation` --references--> `IApplicationDbContext`  [INFERRED]
  specs/006-repository-unitofwork-concurrency/tasks.md → src/RestoCore.Application/Common/Interfaces/IApplicationDbContext.cs
- `1.1 `IUnitOfWork`` --references--> `IUnitOfWork`  [INFERRED]
  specs/006-repository-unitofwork-concurrency/data-model.md → src/RestoCore.Application/Common/Interfaces/IUnitOfWork.cs
- `1.3 `IKitchenRepository`` --references--> `IKitchenRepository`  [INFERRED]
  specs/006-repository-unitofwork-concurrency/data-model.md → src/RestoCore.Application/Common/Interfaces/Repositories/IKitchenRepository.cs
- `Decision 3: Optimistic Concurrency Control & Retry Strategy` --references--> `IKitchenRepository`  [INFERRED]
  specs/006-repository-unitofwork-concurrency/research.md → src/RestoCore.Application/Common/Interfaces/Repositories/IKitchenRepository.cs
- `2. Infrastructure Entity Framework Implementations` --references--> `ApplicationDbContext`  [INFERRED]
  specs/006-repository-unitofwork-concurrency/data-model.md → src/RestoCore.Infrastructure/Persistence/ApplicationDbContext.cs

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

## Communities (77 total, 19 thin omitted)

### Community 0 - "PublicMenuItemDto"
Cohesion: 0.13
Nodes (15): PublicCategoryDto, Description, Id, Items, Name, PublicMenuItemDto, Allergens, BasePrice (+7 more)

### Community 1 - "Structured Logging & Traces Specification"
Cohesion: 0.05
Nodes (42): k6 Aspire Swagger Requirements Checklist, Observability and Swagger API Contract, Swagger OpenAPI Explorer Endpoints, k6 & Aspire Quickstart Guide, k6 Performance Testing Scenarios, k6 Aspire Swagger Specification, US1: Interactive API Exploration, US2: Real-time Telemetry Dashboard (+34 more)

### Community 2 - "Tenant Entity"
Cohesion: 0.08
Nodes (24): Digital Menu Quality Checklist, Digital Menu OpenAPI Specification, Category Entity, MenuItem Entity, ModifierGroup Entity, ModifierOption Entity, Table Entity, Tenant Entity (+16 more)

### Community 3 - "Task"
Cohesion: 0.06
Nodes (30): Program, PublicMenuResponse, Branding, Categories, LayoutConfig, TableNumber, TenantName, TenantSlug (+22 more)

### Community 5 - "DateTimeOffset"
Cohesion: 0.14
Nodes (4): RestoCore.Infrastructure.Persistence.Migrations, InitialCreate, AddCanvasLayoutAndMenuPublishJobs, ApplicationDbContextModelSnapshot

### Community 6 - "RestoCore Backend Constitution"
Cohesion: 0.09
Nodes (20): AI Agents Operating Guidelines, Skill: Git Commit, Skill: Review TODOs, Skill: Spec-Kit Analyze, Skill: Spec-Kit Clarify, Skill: Spec-Kit Constitution, Skill: Spec-Kit Git Commit, Skill: Spec-Kit Git Remote (+12 more)

### Community 7 - "Guid"
Cohesion: 0.23
Nodes (4): ITenantContext, HasTenant, TenantId, TenantSlug

### Community 8 - "OpaInput"
Cohesion: 0.10
Nodes (18): OpaAuthorizationHandler, OpaRequirement, Action, ResourceType, IOpaClient, OpaInput, Action, Method (+10 more)

### Community 9 - "RestoCore.Application.Common.Interfaces"
Cohesion: 0.14
Nodes (8): RestoCore.Application.Common.Interfaces, RestoCore.SecurityTests.MultiTenancy, RestoCore.Infrastructure.MultiTenancy, RestoCore.Infrastructure.Qr, RestoCore.Api.Middlewares, RestoCore.Infrastructure.Cdn, RestoCore.UnitTests.MultiTenancy, RestoCore.Infrastructure.Authorization

### Community 10 - "ApplicationDbContext"
Cohesion: 0.11
Nodes (14): Dependencies & Execution Order, Format: `[ID] [P?] [Story] Description`, Phase 1: Setup & Foundational Contracts, Phase 5: Verification, Regressions & Graphify Re-evaluation, Tasks: Repositories, Unit of Work, and Concurrency Control, ApplicationDbContext, Categories, CurrentTenantId (+6 more)

### Community 11 - "fluentassertions"
Cohesion: 0.19
Nodes (5): RestoCore.IntegrationTests.Infrastructure, RestoCore.UnitTests.Features.PublicMenu, RestoCore.Application.Features.PublicMenu.DTOs, RestoCore.UnitTests.PublicMenu, RestoCore.UnitTests.Qr

### Community 12 - "MenuPublishEndpointsTests.cs"
Cohesion: 0.21
Nodes (7): RestoCore.UnitTests.Features.MenuLayout, RestoCore.Application.Features.MenuLayout.Commands, RestoCore.Infrastructure.Services.Compilation, RestoCore.Application.Features.MenuLayout.Services, RestoCore.IntegrationTests.Endpoints, RestoCore.IntegrationTests.Fixtures, RestoCore.Application.Features.MenuLayout.DTOs

### Community 13 - "IApplicationDbContext"
Cohesion: 0.11
Nodes (18): Background & Architectural Drivers, Edge Cases, Feature Specification: Repositories and Unit of Work for Concurrency and Persistence Decoupling, Measurable Outcomes, Requirements *(mandatory)*, Success Criteria *(mandatory)*, User Scenarios & Testing *(mandatory)*, User Story 1 - Kitchen Availability Updates with Concurrency Resilience (Priority: P1) (+10 more)

### Community 14 - "PresignedUploadResponse"
Cohesion: 0.26
Nodes (6): IStorageService, PresignedUploadResponse, GeneratePresignedUploadCommand, GeneratePresignedUploadCommandHandler, GeneratePresignedUploadValidator, SeaweedStorageService

### Community 15 - "ModifierOption"
Cohesion: 0.18
Nodes (9): ITenantScopedEntity, TenantId, ModifierOption, IsAvailable, ModifierGroup, ModifierGroupId, Name, PriceDelta (+1 more)

### Community 16 - "ModifierGroup"
Cohesion: 0.15
Nodes (9): ModifierGroup, IsRequired, MaxSelection, MenuItem, MenuItemId, MinSelection, Name, Options (+1 more)

### Community 17 - "common.ps1"
Cohesion: 0.23
Nodes (11): Find-FeatureDirByPrefix(), Find-SpecifyRoot(), Get-CurrentBranch(), Get-FeatureDirFromBranchPrefixOrExit(), Get-FeaturePathsEnv(), Get-Python3Command(), Get-RepoRoot(), Get-SpecKitEffectiveBranchName() (+3 more)

### Community 18 - "http"
Cohesion: 0.13
Nodes (15): ASPNETCORE_ENVIRONMENT, applicationUrl, commandName, dotnetRunMessages, environmentVariables, launchBrowser, applicationUrl, commandName (+7 more)

### Community 19 - "MenuPublishJobResponse"
Cohesion: 0.15
Nodes (14): AdminMenuLayoutEndpoints, PublishMenuCommand, PublishMenuCommandHandler, MenuPublishJobResponse, AssetsQueued, CdnPurgeRequested, EstimatedDurationSeconds, JobId (+6 more)

### Community 21 - "IRequest"
Cohesion: 0.22
Nodes (6): IMediator, IRequest, ISender, Mediator, MediatorServiceCollectionExtensions, GetPublicMenuQuery

### Community 22 - "RestoCore.Infrastructure.csproj"
Cohesion: 0.13
Nodes (14): AWSSDK.S3 (4.0.102.5), Microsoft.Extensions.Caching.StackExchangeRedis (10.0.12), Npgsql.EntityFrameworkCore.PostgreSQL (10.0.3), Npgsql.OpenTelemetry (10.0.3), OpenTelemetry.Exporter.OpenTelemetryProtocol (1.18.0), OpenTelemetry.Extensions.Hosting (1.18.0), OpenTelemetry.Instrumentation.AspNetCore (1.18.0), OpenTelemetry.Instrumentation.Http (1.18.0) (+6 more)

### Community 25 - "UpdateMenuLayoutCommand"
Cohesion: 0.33
Nodes (3): UpdateMenuLayoutCommand, UpdateMenuLayoutCommandHandler, UpdateMenuLayoutValidatorTests

### Community 26 - "MenuItem"
Cohesion: 0.11
Nodes (15): MenuItem, Allergens, BasePrice, Category, CategoryId, Description, DietaryLabels, DisplayOrder (+7 more)

### Community 27 - "RestoCore.Domain.Entities"
Cohesion: 0.15
Nodes (5): RestoCore.Domain.Entities, RestoCore.UnitTests.Features.Kitchen, RestoCore.Domain.Common, RestoCore.Application.Common.Interfaces.Repositories, RestoCore.Infrastructure.Persistence.Repositories

### Community 28 - "CanvasElementDto"
Cohesion: 0.18
Nodes (10): RestoCore.Application.Features.MenuLayout.Validators, CanvasElementDto, DishId, Height, Width, X, Y, ZIndex (+2 more)

### Community 29 - "MenuConfiguration"
Cohesion: 0.20
Nodes (5): RestoCore.Infrastructure.Persistence.Configurations, MenuConfiguration, MenuPublishJobConfiguration, TableConfiguration, TenantConfiguration

### Community 30 - "CreateMenuItemCommand"
Cohesion: 0.11
Nodes (11): RestoCore.Application.Features.Tenants.Validators, RestoCore.Application.Features.MenuItems.Validators, AdminCatalogEndpoints, CategoryResponse, CreateCategoryCommand, CreateCategoryValidator, CreateMenuItemCommand, MenuItemResponse (+3 more)

### Community 31 - "RestoCore.Application.Common.Mediator"
Cohesion: 0.13
Nodes (9): RestoCore.Application.Features.Categories.Validators, RestoCore.Domain.ValueObjects, RestoCore.UnitTests.Domain, RestoCore.Application.Common.Mediator, RestoCore.Api.Endpoints, RestoCore.Application.Features.Tenants.Commands, RestoCore.Application.Features.PublicMenu.Queries, RestoCore.Application.Features.MenuItems.Commands (+1 more)

### Community 32 - "PublicModifierGroupDto"
Cohesion: 0.17
Nodes (12): PublicModifierGroupDto, Id, IsRequired, MaxSelection, MinSelection, Name, Options, PublicModifierOptionDto (+4 more)

### Community 33 - "Tenant"
Cohesion: 0.10
Nodes (21): Implementation for User Story 3, Phase 4: User Story 3 - Canvas Layout Compilation & Tenant Repositories (Priority: P3), IMenuLayoutRepository, ITenantRepository, ProvisionTenantCommand, ProvisionTenantCommandHandler, TenantSummaryResponse, Tenant (+13 more)

### Community 34 - "BrandingConfig"
Cohesion: 0.19
Nodes (11): UpdateBrandingCommand, UpdateBrandingCommandHandler, BrandingConfig, BackgroundColor, CoverBannerUrl, FontFamily, LayoutMode, LogoUrl (+3 more)

### Community 35 - ".Send"
Cohesion: 0.23
Nodes (4): AdminTenantEndpoints, HealthEndpoints, PublicMenuEndpoints, StorageEndpoints

### Community 36 - "RestoCore.IntegrationTests.csproj"
Cohesion: 0.18
Nodes (10): Testcontainers.Redis (4.14.0), net10.0, coverlet.collector (6.0.4), FluentAssertions (8.10.0), Microsoft.AspNetCore.Mvc.Testing (10.0.11), Microsoft.NET.Test.Sdk (17.14.1), Testcontainers.PostgreSql (4.14.0), xunit (2.9.3) (+2 more)

### Community 37 - "IUnitOfWork"
Cohesion: 0.13
Nodes (23): Summary, 1. Dependency Injection in Command Handlers, 2. Using Explicit Transactions, 3. High-Concurrency Item Toggling, Developer Quickstart: Using Repositories & Unit of Work in RestoCore, 1. Context and Problem Statement, 2. Research Decisions, 3. Best Practices Alignment (+15 more)

### Community 38 - "Table"
Cohesion: 0.17
Nodes (7): Table, IsActive, Label, TableNumber, Tenant, TenantId, Token

### Community 39 - "AuthEndpoints.cs"
Cohesion: 0.33
Nodes (5): AuthEndpoints, LoginRequest, LoginResponse, SeededUser, UserProfileResponse

### Community 40 - "RestoCore.Application.csproj"
Cohesion: 0.22
Nodes (7): FluentValidation (12.1.1), Microsoft.EntityFrameworkCore (10.0.11), Microsoft.Extensions.Caching.Abstractions (10.0.11), net10.0, Microsoft.NET.Sdk, net10.0, Microsoft.NET.Sdk

### Community 41 - "GetTableQrQueryHandler"
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

### Community 47 - "Implementation Plan: Repositories, Unit of Work, and Concurrency Control"
Cohesion: 0.29
Nodes (6): Constitution Check, Documentation (this feature), Implementation Plan: Repositories, Unit of Work, and Concurrency Control, Project Structure, Source Code, Technical Context

### Community 48 - "create-new-feature.ps1"
Cohesion: 0.46
Nodes (7): ConvertTo-CleanBranchName(), Get-BranchName(), Get-HighestNumberFromBranches(), Get-HighestNumberFromNames(), Get-HighestNumberFromRemoteRefs(), Get-HighestNumberFromSpecs(), Get-NextBranchNumber()

### Community 49 - "LayoutConfig"
Cohesion: 0.14
Nodes (12): CanvasElement, DishId, Height, Width, X, Y, ZIndex, LayoutConfig (+4 more)

### Community 50 - ".Handle"
Cohesion: 0.67
Nodes (5): AddModifierGroupCommand, AddModifierGroupCommandHandler, ModifierGroupResponse, ModifierOptionInput, ModifierOptionResponse

### Community 51 - "RestoCore.Api.csproj"
Cohesion: 0.29
Nodes (6): Microsoft.AspNetCore.Authentication.JwtBearer (10.0.11), Microsoft.AspNetCore.OpenApi (10.0.11), Swashbuckle.AspNetCore (10.2.3), Microsoft.NET.Sdk.Web, net10.0, Microsoft.EntityFrameworkCore.Design (10.0.11)

### Community 52 - "LayoutConfigDto"
Cohesion: 0.17
Nodes (12): LayoutConfigDto, BackgroundColor, BackgroundUrl, CanvasEnabled, Elements, IMenuCompilationService, MenuCompilationResult, AssetsQueued (+4 more)

### Community 53 - "IKitchenRepository"
Cohesion: 0.15
Nodes (12): RestoCore.Application.Features.Kitchen.Commands, Implementation for User Story 1, Phase 2: User Story 1 - Kitchen Availability Updates with Concurrency Resilience (Priority: P1) 🎯 MVP, Tests for User Story 1, KitchenEndpoints, ToggleAvailabilityRequest, IKitchenRepository, ItemAvailabilityResponse (+4 more)

### Community 54 - "MenuPublishJob"
Cohesion: 0.14
Nodes (12): MenuPublishJob, AssetsQueued, CdnPurgeRequested, CompletedAt, ErrorMessage, EstimatedDurationSeconds, Status, Tenant (+4 more)

### Community 55 - "ICdnPurgeService"
Cohesion: 0.27
Nodes (3): Assumptions, ICdnPurgeService, LocalDevelopmentCdnPurgeService

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

### Community 71 - "PublicBrandingDto"
Cohesion: 0.22
Nodes (9): PublicBrandingDto, BackgroundColor, CoverBannerUrl, FontFamily, LayoutMode, LogoUrl, PrimaryColor, SecondaryColor (+1 more)

### Community 74 - "StorageEndpoints.cs"
Cohesion: 0.33
Nodes (3): RestoCore.Application.Features.Storage.Commands, RestoCore.Application.Features.QrCodes.Queries, AdminTableEndpoints

### Community 76 - "BaseEntity"
Cohesion: 0.40
Nodes (4): BaseEntity, CreatedAt, Id, UpdatedAt

## Knowledge Gaps
- **263 isolated node(s):** `net10.0`, `Microsoft.AspNetCore.Authentication.JwtBearer (10.0.11)`, `Microsoft.AspNetCore.OpenApi (10.0.11)`, `Microsoft.EntityFrameworkCore.Design (10.0.11)`, `Swashbuckle.AspNetCore (10.2.3)` (+258 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 396 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **19 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `Tenant` connect `Tenant` to `BrandingConfig`, `Task`, `Table`, `RestoCore.Domain.Entities`, `ApplicationDbContext`, `BaseEntity`, `IApplicationDbContext`, `Category`, `ModifierGroup`, `LayoutConfig`, `MenuPublishJobResponse`, `MenuPublishJob`, `CancellationToken`, `MenuItem`, `1.2 `ICatalogRepository``, `MenuConfiguration`?**
  _High betweenness centrality (0.059) - this node is a cross-community bridge._
- **Are the 12 inferred relationships involving `MenuItem` (e.g. with `1.2 `ICatalogRepository`` and `Decision 1: Scope and Granularity of Repositories`) actually correct?**
  _`MenuItem` has 12 INFERRED edges - model-reasoned connections that need verification._
- **What connects `net10.0`, `Microsoft.AspNetCore.Authentication.JwtBearer (10.0.11)`, `Microsoft.AspNetCore.OpenApi (10.0.11)` to the rest of the system?**
  _263 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `PublicMenuItemDto` be split into smaller, more focused modules?**
  _Cohesion score 0.13333333333333333 - nodes in this community are weakly interconnected._
- **Why does `ApplicationDbContext` connect `ApplicationDbContext` to `Task`, `DateTimeOffset`, `Guid`, `IApplicationDbContext`, `ModifierOption`, `ModifierGroup`, `MenuPublishJobResponse`, `CancellationToken`, `UpdateMenuLayoutCommand`, `MenuItem`, `RestoCore.Domain.Entities`, `Tenant`, `IUnitOfWork`, `Table`, `Category`, `IKitchenRepository`, `MenuPublishJob`, `ICdnPurgeService`, `1.2 `ICatalogRepository``?**
  _High betweenness centrality (0.055) - this node is a cross-community bridge._
- **Should `Structured Logging & Traces Specification` be split into smaller, more focused modules?**
  _Cohesion score 0.05102040816326531 - nodes in this community are weakly interconnected._
- **Why does `MenuItem` connect `MenuItem` to `Tenant`, `Task`, `IUnitOfWork`, `Guid`, `RestoCore.Domain.Entities`, `ApplicationDbContext`, `List`, `BaseEntity`, `IApplicationDbContext`, `Category`, `ModifierOption`, `ModifierGroup`, `MenuPublishJobResponse`, `IKitchenRepository`, `1.2 `ICatalogRepository``, `MenuConfiguration`?**
  _High betweenness centrality (0.042) - this node is a cross-community bridge._