# Graph Report - resto-core-back  (2026-10-10)

## Corpus Check
- 184 files · ~71,479 words
- Verdict: corpus is large enough that graph structure adds value.
- Unclassified: 4 file(s) not represented in the graph (top: (none) 2, .example 1, .rego 1)

## Summary
- 907 nodes · 1854 edges · 70 communities (54 shown, 16 thin omitted)
- Extraction: 91% EXTRACTED · 9% INFERRED · 0% AMBIGUOUS · INFERRED: 162 edges (avg confidence: 0.85)
- Token cost: 0 input · 0 output

## Community Hubs (Navigation)
- Menu Item Modifiers
- Observability & Performance Testing
- Observability & Performance Testing
- Observability & Performance Testing
- Multi-Tenant Domain Model
- Canvas Menu Layout
- AI Agent Directives & Skills
- Tenant Administration
- OPA Policy Authorization
- OPA Policy Authorization
- Menu Item Modifiers
- Observability & Performance Testing
- Canvas Menu Layout
- Multi-Tenant Domain Model
- Observability & Performance Testing
- Menu Item Modifiers
- Menu Item Modifiers
- Test Fixtures & Readiness
- Applicationurl Commandname
- Multi-Tenant Domain Model
- Media Storage & Uploads
- Observability & Performance Testing
- Telemetry & Tracing
- Distributed Cache & Redis
- Canvas Menu Layout
- Menu Item Modifiers
- Multi-Tenant Domain Model
- Canvas Menu Layout
- Telemetry & Tracing
- Test Fixtures & Readiness
- Canvas Menu Layout
- Multi-Tenant Domain Model
- Digital Menu Catalog
- Purgetagsasync Datetime
- Media Storage & Uploads
- Test Fixtures & Readiness
- Test Fixtures & Readiness
- Category Management
- JWT Authentication & Security
- 10011 Net100
- Test Fixtures & Readiness
- Multi-Tenant Domain Model
- Test Fixtures & Readiness
- Test Fixtures & Readiness
- Observability & Performance Testing
- Multi-Tenant Domain Model
- Canvas Menu Layout
- Createnewfeatureps1 Converttocleanbranchname
- Canvas Menu Layout
- OPA Policy Authorization
- JWT Authentication & Security
- Test Fixtures & Readiness
- Kitchen Operations
- Gettableqrquerycs Gettableqrquery
- Baseentitycs Baseentity
- Template Structure
- Telemetry & Tracing
- Canvas Menu Layout
- Media Storage & Uploads
- AI Agent Directives & Skills
- AI Agent Directives & Skills
- Admin Streamlit
- AI Agent Directives & Skills
- Vector Raster

## God Nodes (most connected - your core abstractions)
1. `RestoCore.Application.Common.Interfaces` - 33 edges
2. `MenuItem` - 33 edges
3. `ApplicationDbContext` - 31 edges
4. `IApplicationDbContext` - 30 edges
5. `LayoutConfigDto` - 30 edges
6. `Tenant` - 30 edges
7. `RestoCore.Domain.Entities` - 26 edges
8. `Category` - 25 edges
9. `MenuPublishJob` - 22 edges
10. `ITenantContext` - 21 edges

## Surprising Connections (you probably didn't know these)
- `Backend Architecture Inviolables` --conceptually_related_to--> `RestoCore Backend Constitution`  [INFERRED]
  AGENTS.md → .specify/memory/constitution.md
- `MenuValidationTests` --references--> `CreateCategoryValidator`  [EXTRACTED]
  tests/RestoCore.UnitTests/Domain/MenuValidationTests.cs → src/RestoCore.Application/Features/Categories/Validators/CreateCategoryValidator.cs
- `UpdateMenuLayoutValidatorTests` --references--> `UpdateMenuLayoutValidator`  [EXTRACTED]
  tests/RestoCore.UnitTests/Features/MenuLayout/UpdateMenuLayoutValidatorTests.cs → src/RestoCore.Application/Features/MenuLayout/Validators/UpdateMenuLayoutValidator.cs
- `Skill: Spec-Kit Constitution` --references--> `RestoCore Backend Constitution`  [EXTRACTED]
  .agents/skills/speckit-constitution/SKILL.md → .specify/memory/constitution.md
- `SwaggerEndpointTests` --references--> `Program`  [EXTRACTED]
  tests/RestoCore.IntegrationTests/Endpoints/SwaggerEndpointTests.cs → src/RestoCore.Api/Program.cs

## Import Cycles
- None detected.

## Hyperedges (group relationships)
- **** — agents_spec_driven_development, specify_memory_constitution, agents_skills_speckit_specify, agents_skills_speckit_plan, agents_skills_speckit_tasks, agents_skills_speckit_implement [INFERRED]
- **** — agents_skills_speckit_git_initialize, agents_skills_speckit_git_feature, agents_skills_speckit_git_validate, agents_skills_speckit_git_commit, agents_skills_speckit_git_remote [INFERRED]
- **Spec-Driven Development Artifacts Pipeline** — specify_workflows_speckit_workflow_speckit, specify_templates_spec_template_structure, specify_templates_plan_template_structure, specify_templates_tasks_template_structure [EXTRACTED 1.00]
- **Multi-Tenant Catalog Data Model Structure** — specs_001_multi_tenant_digital_menu_data_model_tenant, specs_001_multi_tenant_digital_menu_data_model_category, specs_001_multi_tenant_digital_menu_data_model_menuitem, specs_001_multi_tenant_digital_menu_data_model_table [EXTRACTED 1.00]
- **Load Testing and Real-Time Telemetry Feedback Loop** — specs_003_k6_aspire_swagger_spec_us2_aspire_telemetry, specs_003_k6_aspire_swagger_spec_us3_k6_stress_tests, specs_003_k6_aspire_swagger_research_aspire_dashboard, specs_003_k6_aspire_swagger_research_k6_testing [EXTRACTED 1.00]
- **OpenTelemetry Logging and Trace Correlation Framework** — specs_004_structured_logging_traces_data_model_telemetry_event, specs_004_structured_logging_traces_data_model_span_attributes, specs_004_structured_logging_traces_plan_anti_redundancy, specs_004_structured_logging_traces_plan_w3c_propagation, specs_004_structured_logging_traces_contracts_telemetry_contract_otlp_exporter [EXTRACTED 1.00]
- **Decoupled Canvas Layout AOT Publishing Pipeline** — specs_005_canvas_layout_aot_compilation_data_model_layout_config, specs_005_canvas_layout_aot_compilation_data_model_menu_publish_job, specs_005_canvas_layout_aot_compilation_research_jsonb_storage, specs_005_canvas_layout_aot_compilation_research_aot_compilation, specs_005_canvas_layout_aot_compilation_contracts_openapi_canvas_publish_endpoint [EXTRACTED 1.00]

## Communities (70 total, 16 thin omitted)

### Community 0 - "Menu Item Modifiers"
Cohesion: 0.05
Nodes (51): AddModifierGroupCommand, AddModifierGroupCommandHandler, ModifierGroupResponse, ModifierOptionInput, ModifierOptionResponse, PublicBrandingDto, BackgroundColor, CoverBannerUrl (+43 more)

### Community 1 - "Observability & Performance Testing"
Cohesion: 0.05
Nodes (42): k6 Aspire Swagger Requirements Checklist, Observability and Swagger API Contract, Swagger OpenAPI Explorer Endpoints, k6 & Aspire Quickstart Guide, k6 Performance Testing Scenarios, k6 Aspire Swagger Specification, US1: Interactive API Exploration, US2: Real-time Telemetry Dashboard (+34 more)

### Community 2 - "Observability & Performance Testing"
Cohesion: 0.08
Nodes (24): Digital Menu Quality Checklist, Digital Menu OpenAPI Specification, Category Entity, MenuItem Entity, ModifierGroup Entity, ModifierOption Entity, Table Entity, Tenant Entity (+16 more)

### Community 3 - "Observability & Performance Testing"
Cohesion: 0.11
Nodes (10): Program, AdminMenuLayoutEndpointsTests, FullLifecycleE2ETests, MenuPublishEndpointsTests, PublicMenuCanvasEndpointsTests, SwaggerEndpointTests, ContainerizedStackFixture, TestAuthHandler (+2 more)

### Community 4 - "Multi-Tenant Domain Model"
Cohesion: 0.12
Nodes (11): GlobalExceptionHandler, ITenantContext, HasTenant, TenantId, TenantSlug, TenantContext, TenantId, TenantSlug (+3 more)

### Community 5 - "Canvas Menu Layout"
Cohesion: 0.15
Nodes (4): RestoCore.Infrastructure.Persistence.Migrations, InitialCreate, AddCanvasLayoutAndMenuPublishJobs, ApplicationDbContextModelSnapshot

### Community 6 - "AI Agent Directives & Skills"
Cohesion: 0.09
Nodes (20): AI Agents Operating Guidelines, Skill: Git Commit, Skill: Review TODOs, Skill: Spec-Kit Analyze, Skill: Spec-Kit Clarify, Skill: Spec-Kit Constitution, Skill: Spec-Kit Git Commit, Skill: Spec-Kit Git Remote (+12 more)

### Community 7 - "Tenant Administration"
Cohesion: 0.11
Nodes (18): RestoCore.Application.Features.Tenants.Validators, RestoCore.Application.Features.Tenants.Commands, AdminTenantEndpoints, ProvisionTenantCommand, ProvisionTenantCommandHandler, TenantSummaryResponse, UpdateBrandingCommand, UpdateBrandingCommandHandler (+10 more)

### Community 8 - "OPA Policy Authorization"
Cohesion: 0.12
Nodes (17): IOpaClient, OpaClient, OpaInput, Action, Method, Path, Resource, User (+9 more)

### Community 9 - "OPA Policy Authorization"
Cohesion: 0.14
Nodes (8): RestoCore.Application.Common.Interfaces, RestoCore.SecurityTests.MultiTenancy, RestoCore.Infrastructure.MultiTenancy, RestoCore.Infrastructure.Qr, RestoCore.Api.Middlewares, RestoCore.Infrastructure.Cdn, RestoCore.UnitTests.MultiTenancy, RestoCore.Infrastructure.Authorization

### Community 10 - "Menu Item Modifiers"
Cohesion: 0.09
Nodes (17): IApplicationDbContext, Categories, MenuItems, MenuPublishJobs, ModifierGroups, ModifierOptions, Tables, Tenants (+9 more)

### Community 11 - "Observability & Performance Testing"
Cohesion: 0.24
Nodes (4): RestoCore.Domain.ValueObjects, RestoCore.IntegrationTests.Endpoints, RestoCore.IntegrationTests.Fixtures, RestoCore.Infrastructure.Persistence

### Community 12 - "Canvas Menu Layout"
Cohesion: 0.24
Nodes (5): RestoCore.UnitTests.Features.MenuLayout, RestoCore.Application.Features.MenuLayout.Commands, RestoCore.Infrastructure.Services.Compilation, RestoCore.Application.Features.MenuLayout.Services, RestoCore.Application.Features.MenuLayout.DTOs

### Community 13 - "Multi-Tenant Domain Model"
Cohesion: 0.11
Nodes (17): Tenant, BrandingConfig, Categories, CurrentVersionHash, CustomDomain, LayoutConfig, MenuItems, MenuPublishJobs (+9 more)

### Community 14 - "Observability & Performance Testing"
Cohesion: 0.17
Nodes (4): AspireDashboardReadinessTests, ContainerReadinessTests, ItemAvailabilityStateTests, PublicMenuCanvasTests

### Community 15 - "Menu Item Modifiers"
Cohesion: 0.12
Nodes (10): RestoCore.Domain.Common, ITenantScopedEntity, TenantId, ModifierOption, IsAvailable, ModifierGroup, ModifierGroupId, Name (+2 more)

### Community 16 - "Menu Item Modifiers"
Cohesion: 0.14
Nodes (11): ModifierGroup, IsRequired, MaxSelection, MenuItem, MenuItemId, MinSelection, Name, Options (+3 more)

### Community 17 - "Test Fixtures & Readiness"
Cohesion: 0.23
Nodes (11): Find-FeatureDirByPrefix(), Find-SpecifyRoot(), Get-CurrentBranch(), Get-FeatureDirFromBranchPrefixOrExit(), Get-FeaturePathsEnv(), Get-Python3Command(), Get-RepoRoot(), Get-SpecKitEffectiveBranchName() (+3 more)

### Community 18 - "Applicationurl Commandname"
Cohesion: 0.13
Nodes (15): ASPNETCORE_ENVIRONMENT, applicationUrl, commandName, dotnetRunMessages, environmentVariables, launchBrowser, applicationUrl, commandName (+7 more)

### Community 19 - "Multi-Tenant Domain Model"
Cohesion: 0.14
Nodes (13): IRequest, PublishMenuCommand, MenuPublishJobResponse, AssetsQueued, CdnPurgeRequested, EstimatedDurationSeconds, JobId, Status (+5 more)

### Community 20 - "Media Storage & Uploads"
Cohesion: 0.13
Nodes (3): RestoCore.Infrastructure.Storage, RestoCore.Infrastructure.Telemetry, OpenTelemetryExtensions

### Community 21 - "Observability & Performance Testing"
Cohesion: 0.19
Nodes (3): RestoCore.IntegrationTests.Infrastructure, RestoCore.IntegrationTests.Storage, RestoCore.UnitTests.Qr

### Community 22 - "Telemetry & Tracing"
Cohesion: 0.13
Nodes (14): AWSSDK.S3 (4.0.102.5), Microsoft.Extensions.Caching.StackExchangeRedis (10.0.12), Npgsql.EntityFrameworkCore.PostgreSQL (10.0.3), Npgsql.OpenTelemetry (10.0.3), OpenTelemetry.Exporter.OpenTelemetryProtocol (1.18.0), OpenTelemetry.Extensions.Hosting (1.18.0), OpenTelemetry.Instrumentation.AspNetCore (1.18.0), OpenTelemetry.Instrumentation.Http (1.18.0) (+6 more)

### Community 25 - "Canvas Menu Layout"
Cohesion: 0.26
Nodes (8): UpdateMenuLayoutCommand, UpdateMenuLayoutCommandHandler, LayoutConfigDto, BackgroundColor, BackgroundUrl, CanvasEnabled, Elements, UpdateMenuLayoutValidatorTests

### Community 26 - "Menu Item Modifiers"
Cohesion: 0.14
Nodes (14): MenuItem, Allergens, BasePrice, Category, CategoryId, Description, DietaryLabels, DisplayOrder (+6 more)

### Community 27 - "Multi-Tenant Domain Model"
Cohesion: 0.15
Nodes (13): MenuPublishJob, AssetsQueued, CdnPurgeRequested, CompletedAt, ErrorMessage, EstimatedDurationSeconds, Status, Tenant (+5 more)

### Community 28 - "Canvas Menu Layout"
Cohesion: 0.18
Nodes (10): RestoCore.Application.Features.MenuLayout.Validators, CanvasElementDto, DishId, Height, Width, X, Y, ZIndex (+2 more)

### Community 29 - "Telemetry & Tracing"
Cohesion: 0.21
Nodes (4): IMediator, ISender, Mediator, MediatorServiceCollectionExtensions

### Community 30 - "Test Fixtures & Readiness"
Cohesion: 0.21
Nodes (7): RestoCore.Application.Features.Categories.Validators, RestoCore.UnitTests.Domain, RestoCore.Application.Features.MenuItems.Commands, RestoCore.Application.Features.MenuItems.Validators, RestoCore.Application.Features.Categories.Commands, AdminCatalogEndpoints, CreateCategoryValidator

### Community 31 - "Canvas Menu Layout"
Cohesion: 0.22
Nodes (8): RestoCore.Application.Features.Storage.Commands, RestoCore.Application.Common.Mediator, RestoCore.Api.Endpoints, RestoCore.Application.Features.Kitchen.Commands, RestoCore.Application.Features.QrCodes.Queries, AdminMenuLayoutEndpoints, KitchenEndpoints, ToggleAvailabilityRequest

### Community 33 - "Digital Menu Catalog"
Cohesion: 0.18
Nodes (4): RestoCore.Application.Features.PublicMenu.Queries, AdminTableEndpoints, HealthEndpoints, PublicMenuEndpoints

### Community 34 - "Purgetagsasync Datetime"
Cohesion: 0.23
Nodes (5): ICdnPurgeService, IRequestHandler, PublishMenuCommandHandler, IMenuCompilationService, LocalDevelopmentCdnPurgeService

### Community 35 - "Media Storage & Uploads"
Cohesion: 0.32
Nodes (6): IStorageService, PresignedUploadResponse, GeneratePresignedUploadCommand, GeneratePresignedUploadCommandHandler, GeneratePresignedUploadValidator, SeaweedStorageService

### Community 36 - "Test Fixtures & Readiness"
Cohesion: 0.18
Nodes (10): Testcontainers.Redis (4.14.0), net10.0, coverlet.collector (6.0.4), FluentAssertions (8.10.0), Microsoft.AspNetCore.Mvc.Testing (10.0.11), Microsoft.NET.Test.Sdk (17.14.1), Testcontainers.PostgreSql (4.14.0), xunit (2.9.3) (+2 more)

### Community 37 - "Test Fixtures & Readiness"
Cohesion: 0.35
Nodes (5): CreateMenuItemCommand, CreateMenuItemCommandHandler, MenuItemResponse, CreateMenuItemValidator, MenuValidationTests

### Community 38 - "Category Management"
Cohesion: 0.31
Nodes (3): CategoryResponse, CreateCategoryCommand, CreateCategoryCommandHandler

### Community 39 - "JWT Authentication & Security"
Cohesion: 0.33
Nodes (5): AuthEndpoints, LoginRequest, LoginResponse, SeededUser, UserProfileResponse

### Community 40 - "10011 Net100"
Cohesion: 0.22
Nodes (7): FluentValidation (12.1.1), Microsoft.EntityFrameworkCore (10.0.11), Microsoft.Extensions.Caching.Abstractions (10.0.11), net10.0, Microsoft.NET.Sdk, net10.0, Microsoft.NET.Sdk

### Community 41 - "Test Fixtures & Readiness"
Cohesion: 0.29
Nodes (3): IQrCodeService, QrCodeService, QrCodeServiceTests

### Community 42 - "Multi-Tenant Domain Model"
Cohesion: 0.22
Nodes (8): Table, IsActive, Label, TableNumber, Tenant, TenantId, Token, TableConfiguration

### Community 43 - "Test Fixtures & Readiness"
Cohesion: 0.20
Nodes (9): net10.0, coverlet.collector (6.0.4), FluentAssertions (8.10.0), Microsoft.AspNetCore.Mvc.Testing (10.0.11), Microsoft.NET.Test.Sdk (17.14.1), Testcontainers.PostgreSql (4.14.0), xunit (2.9.3), xunit.runner.visualstudio (3.1.4) (+1 more)

### Community 44 - "Test Fixtures & Readiness"
Cohesion: 0.22
Nodes (8): Moq (4.20.72), net10.0, coverlet.collector (6.0.4), FluentAssertions (8.10.0), Microsoft.NET.Test.Sdk (17.14.1), xunit (2.9.3), xunit.runner.visualstudio (3.1.4), Microsoft.NET.Sdk

### Community 45 - "Observability & Performance Testing"
Cohesion: 0.22
Nodes (4): options, options, options, options

### Community 46 - "Multi-Tenant Domain Model"
Cohesion: 0.22
Nodes (8): Category, Description, DisplayOrder, IsActive, Items, Name, Tenant, TenantId

### Community 47 - "Canvas Menu Layout"
Cohesion: 0.39
Nodes (3): RestoCore.UnitTests.Features.PublicMenu, RestoCore.Application.Features.PublicMenu.DTOs, RestoCore.UnitTests.PublicMenu

### Community 48 - "Createnewfeatureps1 Converttocleanbranchname"
Cohesion: 0.46
Nodes (7): ConvertTo-CleanBranchName(), Get-BranchName(), Get-HighestNumberFromBranches(), Get-HighestNumberFromNames(), Get-HighestNumberFromRemoteRefs(), Get-HighestNumberFromSpecs(), Get-NextBranchNumber()

### Community 49 - "Canvas Menu Layout"
Cohesion: 0.25
Nodes (7): CanvasElement, DishId, Height, Width, X, Y, ZIndex

### Community 50 - "OPA Policy Authorization"
Cohesion: 0.29
Nodes (4): OpaAuthorizationHandler, OpaRequirement, Action, ResourceType

### Community 51 - "JWT Authentication & Security"
Cohesion: 0.29
Nodes (6): Microsoft.AspNetCore.Authentication.JwtBearer (10.0.11), Microsoft.AspNetCore.OpenApi (10.0.11), Swashbuckle.AspNetCore (10.2.3), Microsoft.NET.Sdk.Web, net10.0, Microsoft.EntityFrameworkCore.Design (10.0.11)

### Community 53 - "Kitchen Operations"
Cohesion: 0.73
Nodes (3): ItemAvailabilityResponse, ToggleItemAvailabilityCommand, ToggleItemAvailabilityCommandHandler

### Community 54 - "Gettableqrquerycs Gettableqrquery"
Cohesion: 0.90
Nodes (3): GetTableQrQuery, GetTableQrQueryHandler, QrResult

### Community 55 - "Baseentitycs Baseentity"
Cohesion: 0.40
Nodes (4): BaseEntity, CreatedAt, Id, UpdatedAt

### Community 57 - "Template Structure"
Cohesion: 0.50
Nodes (4): Plan Template Structure, Spec Template Structure, Tasks Template Structure, Speckit SDD Workflow

### Community 59 - "Canvas Menu Layout"
Cohesion: 0.50
Nodes (4): MenuCompilationResult, AssetsQueued, CompiledLayout, VersionHash

## Knowledge Gaps
- **263 isolated node(s):** `net10.0`, `Microsoft.AspNetCore.Authentication.JwtBearer (10.0.11)`, `Microsoft.AspNetCore.OpenApi (10.0.11)`, `Microsoft.EntityFrameworkCore.Design (10.0.11)`, `Swashbuckle.AspNetCore (10.2.3)` (+258 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 359 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **16 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `RestoCore.Application.Common.Interfaces` connect `OPA Policy Authorization` to `Multi-Tenant Domain Model`, `Menu Item Modifiers`, `Purgetagsasync Datetime`, `Media Storage & Uploads`, `Multi-Tenant Domain Model`, `Test Fixtures & Readiness`, `Category Management`, `Tenant Administration`, `Test Fixtures & Readiness`, `Observability & Performance Testing`, `Canvas Menu Layout`, `Canvas Menu Layout`, `Media Storage & Uploads`, `Observability & Performance Testing`, `Kitchen Operations`, `Gettableqrquerycs Gettableqrquery`, `Canvas Menu Layout`?**
  _High betweenness centrality (0.044) - this node is a cross-community bridge._
- **Are the 8 inferred relationships involving `MenuItem` (e.g. with `.Handle()` and `.PutLayout_WithValidCoordinates_PersistsAndReturns200()`) actually correct?**
  _`MenuItem` has 8 INFERRED edges - model-reasoned connections that need verification._
- **What connects `net10.0`, `Microsoft.AspNetCore.Authentication.JwtBearer (10.0.11)`, `Microsoft.AspNetCore.OpenApi (10.0.11)` to the rest of the system?**
  _263 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Menu Item Modifiers` be split into smaller, more focused modules?**
  _Cohesion score 0.05137844611528822 - nodes in this community are weakly interconnected._
- **Why does `ApplicationDbContext` connect `Menu Item Modifiers` to `Multi-Tenant Domain Model`, `Purgetagsasync Datetime`, `Multi-Tenant Domain Model`, `Canvas Menu Layout`, `Multi-Tenant Domain Model`, `Multi-Tenant Domain Model`, `Multi-Tenant Domain Model`, `Menu Item Modifiers`, `Menu Item Modifiers`, `Multi-Tenant Domain Model`, `Multi-Tenant Domain Model`, `Menu Item Modifiers`, `Multi-Tenant Domain Model`?**
  _High betweenness centrality (0.040) - this node is a cross-community bridge._
- **Should `Observability & Performance Testing` be split into smaller, more focused modules?**
  _Cohesion score 0.05102040816326531 - nodes in this community are weakly interconnected._
- **Why does `IApplicationDbContext` connect `Menu Item Modifiers` to `Multi-Tenant Domain Model`, `Menu Item Modifiers`, `Purgetagsasync Datetime`, `Test Fixtures & Readiness`, `Category Management`, `Tenant Administration`, `Multi-Tenant Domain Model`, `Multi-Tenant Domain Model`, `Multi-Tenant Domain Model`, `Menu Item Modifiers`, `Menu Item Modifiers`, `Test Fixtures & Readiness`, `Kitchen Operations`, `Gettableqrquerycs Gettableqrquery`, `Multi-Tenant Domain Model`, `Canvas Menu Layout`, `Menu Item Modifiers`, `Multi-Tenant Domain Model`?**
  _High betweenness centrality (0.033) - this node is a cross-community bridge._