using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RestoCore.Api.Endpoints;
using RestoCore.Api.Middlewares;
using RestoCore.Application.Common.Interfaces;
using RestoCore.Application.Features.PublicMenu.Queries;
using RestoCore.Infrastructure.Authorization;
using RestoCore.Infrastructure.Cdn;
using RestoCore.Infrastructure.MultiTenancy;
using RestoCore.Infrastructure.Persistence;
using RestoCore.Infrastructure.Qr;
using RestoCore.Infrastructure.Storage;
using RestoCore.Infrastructure.Telemetry;
using RestoCore.Application.Common.Mediator;

var builder = WebApplication.CreateBuilder(args);

// Add services to container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "RestoCore Backend API",
        Version = "v1",
        Description = "Multi-tenant restaurant digital menu and kitchen operations backend API."
    });

    var securityScheme = new Microsoft.OpenApi.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter JWT Bearer token format: Bearer {your token}",
        In = Microsoft.OpenApi.ParameterLocation.Header,
        Type = Microsoft.OpenApi.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    };

    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(document => new Microsoft.OpenApi.OpenApiSecurityRequirement
    {
        { new Microsoft.OpenApi.OpenApiSecuritySchemeReference("Bearer", document), new List<string>() }
    });
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

// Authentication & Authorization
builder.Services.AddAuthentication().AddJwtBearer(options =>
{
    // Local dev configuration
});

// CQRS Mediator (MIT Permissive Implementation)
builder.Services.AddApplicationMediator(typeof(GetPublicMenuQuery).Assembly);

// Multi-Tenancy
builder.Services.AddScoped<ITenantContext, TenantContext>();

// Services
builder.Services.AddSingleton<IQrCodeService, QrCodeService>();
builder.Services.AddSingleton<IStorageService, SeaweedStorageService>();
builder.Services.AddSingleton<ICdnPurgeService, LocalDevelopmentCdnPurgeService>();
builder.Services.AddScoped<RestoCore.Application.Features.MenuLayout.Services.IMenuCompilationService, RestoCore.Infrastructure.Services.Compilation.MenuCompilationService>();

// Caching (Redis)
var redisConfig = builder.Configuration["Redis:Configuration"] ?? "localhost:6379";
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConfig;
    options.InstanceName = "RestoCore:";
});

// Persistence (PostgreSQL with JSONB & GIN support)
builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
        npgsqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
    });
});
builder.Services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

// Authorization & OPA
builder.Services.AddHttpClient<IOpaClient, OpaClient>();
builder.Services.AddScoped<IAuthorizationHandler, OpaAuthorizationHandler>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("PublicRead", policy => policy.RequireAssertion(_ => true));
    options.AddPolicy("TenantAdminOnly", policy => policy.Requirements.Add(new OpaRequirement("manage_menu")));
    options.AddPolicy("KitchenOnly", policy => policy.Requirements.Add(new OpaRequirement("kitchen_stock")));
    options.AddPolicy("SuperAdminOnly", policy => policy.Requirements.Add(new OpaRequirement("manage_tenants")));
});

// Telemetry (OpenTelemetry Traces, Metrics & Structured Logging)
builder.Services.AddRestoCoreTelemetry(builder.Configuration);

// Exception Handling
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

// Configure HTTP request pipeline
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<TenantResolutionMiddleware>();

// Structured Execution Logging Middleware (correlates with Aspire Dashboard & OpenTelemetry traces)
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    var tenantContext = context.RequestServices.GetRequiredService<ITenantContext>();

    logger.LogInformation("HTTP {Method} {Path} received for Tenant '{TenantSlug}' (TenantId: {TenantId})",
        context.Request.Method,
        context.Request.Path,
        tenantContext.TenantSlug ?? "none",
        tenantContext.TenantId?.ToString() ?? "none");

    try
    {
        await next();
        stopwatch.Stop();

        logger.LogInformation("HTTP {Method} {Path} completed with Status {StatusCode} in {ElapsedMs}ms for Tenant '{TenantSlug}'",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds,
            tenantContext.TenantSlug ?? "none");
    }
    catch (Exception ex)
    {
        stopwatch.Stop();
        logger.LogError(ex, "HTTP {Method} {Path} failed after {ElapsedMs}ms for Tenant '{TenantSlug}'",
            context.Request.Method,
            context.Request.Path,
            stopwatch.ElapsedMilliseconds,
            tenantContext.TenantSlug ?? "none");
        throw;
    }
});

app.UseAuthentication();
app.UseAuthorization();

// Feature & System Endpoints
app.MapHealthEndpoints();
app.MapPublicMenuEndpoints();
app.MapAdminCatalogEndpoints();
app.MapAdminMenuLayoutEndpoints();
app.MapKitchenEndpoints();
app.MapAdminTenantEndpoints();
app.MapAdminTableEndpoints();
app.MapStorageEndpoints();

app.Run();

// Required for WebApplicationFactory in integration tests
public partial class Program { }
