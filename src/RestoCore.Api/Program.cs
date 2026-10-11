using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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

// Load .env file if present in repository root or execution directory
var envCandidates = new[]
{
    Path.Combine(Directory.GetCurrentDirectory(), ".env"),
    Path.Combine(AppContext.BaseDirectory, ".env"),
    Path.Combine(Directory.GetCurrentDirectory(), "..", ".env"),
    Path.Combine(Directory.GetCurrentDirectory(), "..", "..", ".env")
};

foreach (var envPath in envCandidates)
{
    if (File.Exists(envPath))
    {
        foreach (var line in File.ReadAllLines(envPath))
        {
            var trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith('#')) continue;
            var separatorIndex = trimmed.IndexOf('=');
            if (separatorIndex > 0)
            {
                var key = trimmed[..separatorIndex].Trim();
                var value = trimmed[(separatorIndex + 1)..].Trim().Trim('"').Trim('\'');
                Environment.SetEnvironmentVariable(key, value);
            }
        }
        break;
    }
}

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

// Strict CORS Configuration from .env or appsettings (Not overly permissive)
var allowedOriginsSetting = Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS")
    ?? builder.Configuration["Cors:AllowedOrigins"]
    ?? builder.Configuration["CORS_ALLOWED_ORIGINS"]
    ?? "http://localhost:3000";

var allowedOrigins = allowedOriginsSetting
    .Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    .ToArray();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .WithExposedHeaders("ETag");
    });
});

// Authentication & Authorization
var jwtKey = builder.Configuration["Jwt:Key"] ?? "RestoCoreSuperSecretDevelopmentKey2026!#*";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "RestoCore";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "RestoCore";
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = signingKey,
        RoleClaimType = ClaimTypes.Role,
        NameClaimType = ClaimTypes.NameIdentifier
    };
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
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<RestoCore.Application.Common.Interfaces.Repositories.IKitchenRepository, RestoCore.Infrastructure.Persistence.Repositories.KitchenRepository>();
builder.Services.AddScoped<RestoCore.Application.Common.Interfaces.Repositories.ICatalogRepository, RestoCore.Infrastructure.Persistence.Repositories.CatalogRepository>();
builder.Services.AddScoped<RestoCore.Application.Common.Interfaces.Repositories.IMenuLayoutRepository, RestoCore.Infrastructure.Persistence.Repositories.MenuLayoutRepository>();
builder.Services.AddScoped<RestoCore.Application.Common.Interfaces.Repositories.ITenantRepository, RestoCore.Infrastructure.Persistence.Repositories.TenantRepository>();
builder.Services.AddScoped<RestoCore.Application.Common.Interfaces.Repositories.IOrderRepository, RestoCore.Infrastructure.Persistence.Repositories.OrderRepository>();

// Redis Streams & Order Queue
var redisMuxer = StackExchange.Redis.ConnectionMultiplexer.Connect(redisConfig);
builder.Services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(redisMuxer);
builder.Services.AddScoped<IOrderQueueService, RestoCore.Infrastructure.Messaging.RedisOrderQueueService>();

// WebSockets & Notifications
builder.Services.AddSingleton<RestoCore.Infrastructure.WebSockets.KitchenWebSocketManager>();
builder.Services.AddSingleton<IKitchenNotifier>(sp => sp.GetRequiredService<RestoCore.Infrastructure.WebSockets.KitchenWebSocketManager>());

// Background Workers
builder.Services.AddHostedService<RestoCore.Infrastructure.BackgroundServices.OrderProcessorWorker>();
builder.Services.AddHostedService<RestoCore.Infrastructure.Workers.PurgeArchivedDishesWorker>();

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
app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
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

app.UseAuthorization();
app.UseWebSockets();

// Kitchen WebSocket stream for real-time dispatch (ADR-0007)
app.Map("/ws/kitchen", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var tenantSlug = context.Request.Query["tenant"].ToString();
        if (string.IsNullOrWhiteSpace(tenantSlug))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        var wsManager = context.RequestServices.GetRequiredService<RestoCore.Infrastructure.WebSockets.KitchenWebSocketManager>();
        wsManager.RegisterSocket(tenantSlug, webSocket);

        var buffer = new byte[1024 * 4];
        while (webSocket.State == System.Net.WebSockets.WebSocketState.Open)
        {
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == System.Net.WebSockets.WebSocketMessageType.Close)
            {
                await webSocket.CloseAsync(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None);
            }
        }
    }
    else
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
    }
});

// Feature & System Endpoints
app.MapHealthEndpoints();
app.MapAuthEndpoints();
app.MapPublicMenuEndpoints();
app.MapAdminCatalogEndpoints();
app.MapAdminMenuLayoutEndpoints();
app.MapKitchenEndpoints();
app.MapAdminTenantEndpoints();
app.MapAdminTableEndpoints();
app.MapStorageEndpoints();
app.MapOrderEndpoints();

app.Run();

// Required for WebApplicationFactory in integration tests
public partial class Program { }
