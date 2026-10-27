namespace RestoCore.Infrastructure.Persistence.Seed;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RestoCore.Application.Common.Interfaces;
using RestoCore.Domain.Entities;
using RestoCore.Domain.ValueObjects;

public static class DatabaseSeeder
{
    public static readonly Guid DefaultTenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public const string DefaultTenantSlug = "el-gato-negro";

    public static async Task SeedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseSeeder");

        logger.LogInformation("Starting on-demand database seeding...");

        await context.Database.EnsureCreatedAsync(cancellationToken);

        // Ensure schema compatibility for pre-existing local PostgreSQL volumes
        const string ensureSchemaSql = """
            ALTER TABLE tenants ADD COLUMN IF NOT EXISTS "PhoneNumber" character varying(50);
            ALTER TABLE tenants ADD COLUMN IF NOT EXISTS "Address" character varying(255);
            ALTER TABLE tenants ADD COLUMN IF NOT EXISTS "BusinessHours" character varying(255);
            ALTER TABLE menu_items ADD COLUMN IF NOT EXISTS "IsDeleted" boolean NOT NULL DEFAULT false;
            ALTER TABLE menu_items ADD COLUMN IF NOT EXISTS "DeletedAt" timestamp with time zone;

            CREATE TABLE IF NOT EXISTS users (
                "Id" uuid NOT NULL,
                "Email" character varying(150) NOT NULL,
                "Name" character varying(100) NOT NULL,
                "PasswordHash" character varying(256) NOT NULL,
                "PrimaryRole" character varying(50) NOT NULL,
                "Roles" text[] NOT NULL,
                "TenantId" uuid NULL,
                "IsActive" boolean NOT NULL DEFAULT true,
                "CreatedAt" timestamp with time zone NOT NULL,
                "UpdatedAt" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_users" PRIMARY KEY ("Id"),
                CONSTRAINT "FK_users_tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES tenants ("Id") ON DELETE CASCADE
            );
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_users_Email" ON users ("Email");
            """;

        await context.Database.ExecuteSqlRawAsync(ensureSchemaSql, cancellationToken);

        // 1. Seed Base Tenant (El Gato Negro)
        var tenant = await context.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == DefaultTenantId || t.Slug == DefaultTenantSlug, cancellationToken);

        if (tenant == null)
        {
            tenant = new Tenant
            {
                Id = DefaultTenantId,
                Name = "El Gato Negro",
                Slug = DefaultTenantSlug,
                Status = "Active",
                PhoneNumber = "+54 11 4567-8901",
                Address = "Av. Corrientes 1669, CABA",
                BusinessHours = "Lunes a Domingo de 08:00 a 00:00 hs",
                BrandingConfig = new BrandingConfig
                {
                    PrimaryColor = "#C9A227",
                    SecondaryColor = "#1C1C1C",
                    BackgroundColor = "#FAF8F5",
                    TextColor = "#2B2D42",
                    FontFamily = "Playfair Display, serif",
                    LayoutMode = "GridWithImages"
                }
            };
            context.Tenants.Add(tenant);
            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Seeded tenant '{TenantName}' ({TenantId}).", tenant.Name, tenant.Id);
        }

        // 2. Seed Users (with PBKDF2-HMAC-SHA256 hashed passwords)
        var seedUsers = new[]
        {
            new
            {
                Email = "admin@bistrogourmet.com",
                Password = "admin123",
                Name = "Chef Marco",
                PrimaryRole = "owner",
                Roles = new[] { "owner", "tenant_admin", "kitchen_staff" },
                TenantId = (Guid?)tenant.Id
            },
            new
            {
                Email = "admin@restocore.com",
                Password = "admin123",
                Name = "Administrador Local",
                PrimaryRole = "owner",
                Roles = new[] { "owner", "tenant_admin", "kitchen_staff" },
                TenantId = (Guid?)tenant.Id
            },
            new
            {
                Email = "cocina@restocore.com",
                Password = "cocina123",
                Name = "Cocinero Pablo",
                PrimaryRole = "cook",
                Roles = new[] { "cook", "kitchen_staff" },
                TenantId = (Guid?)tenant.Id
            },
            new
            {
                Email = "superadmin@restocore.com",
                Password = "admin123",
                Name = "Super Administrador",
                PrimaryRole = "super_admin",
                Roles = new[] { "super_admin" },
                TenantId = (Guid?)null
            }
        };

        foreach (var u in seedUsers)
        {
            var exists = await context.Users
                .IgnoreQueryFilters()
                .AnyAsync(x => x.Email.ToLower() == u.Email.ToLower(), cancellationToken);

            if (!exists)
            {
                context.Users.Add(new User
                {
                    Email = u.Email,
                    Name = u.Name,
                    PasswordHash = passwordHasher.HashPassword(u.Password),
                    PrimaryRole = u.PrimaryRole,
                    Roles = u.Roles,
                    TenantId = u.TenantId,
                    IsActive = true
                });
                logger.LogInformation("Seeded user '{Email}' with role '{Role}'.", u.Email, u.PrimaryRole);
            }
        }
        await context.SaveChangesAsync(cancellationToken);

        // 3. Seed Categories and Sample Menu Items
        var hasCategories = await context.Categories
            .IgnoreQueryFilters()
            .AnyAsync(c => c.TenantId == tenant.Id, cancellationToken);

        if (!hasCategories)
        {
            var catEntradas = new Category
            {
                TenantId = tenant.Id,
                Name = "Entradas y Tapas",
                Description = "Especialidades artesanales para compartir",
                DisplayOrder = 1,
                IsActive = true
            };

            var catPrincipales = new Category
            {
                TenantId = tenant.Id,
                Name = "Platos Principales",
                Description = "Cortes seleccionados a las brasas y pastas de autor",
                DisplayOrder = 2,
                IsActive = true
            };

            var catPostres = new Category
            {
                TenantId = tenant.Id,
                Name = "Postres y Cafetería",
                Description = "Clásicos porteños y café de especialidad tostado en casa",
                DisplayOrder = 3,
                IsActive = true
            };

            context.Categories.AddRange(catEntradas, catPrincipales, catPostres);
            await context.SaveChangesAsync(cancellationToken);

            var items = new List<MenuItem>
            {
                new()
                {
                    TenantId = tenant.Id,
                    CategoryId = catEntradas.Id,
                    Name = "Empanadas Salteñas Cortadas a Cuchillo",
                    Description = "Dúo de empanadas horneadas al barro con salsa yasgua.",
                    BasePrice = 4500.00m,
                    DisplayOrder = 1,
                    IsAvailable = true,
                    Allergens = new[] { "gluten" },
                    DietaryLabels = new[] { "artisanal" }
                },
                new()
                {
                    TenantId = tenant.Id,
                    CategoryId = catEntradas.Id,
                    Name = "Provoleta Ahumada con Tomates Confitados",
                    Description = "Queso provolone estacionado a la chapa con rúcula fresca y reducción de aceto.",
                    BasePrice = 8900.00m,
                    DisplayOrder = 2,
                    IsAvailable = true,
                    Allergens = new[] { "dairy" },
                    DietaryLabels = new[] { "vegetarian", "gluten_free", "celiac_safe" }
                },
                new()
                {
                    TenantId = tenant.Id,
                    CategoryId = catPrincipales.Id,
                    Name = "Ojo de Bife Madurado 400g",
                    Description = "Corte Angus certificado con papas rústicas al romero y manteca de hierbas.",
                    BasePrice = 22500.00m,
                    DisplayOrder = 1,
                    IsAvailable = true,
                    Allergens = new[] { "dairy" },
                    DietaryLabels = new[] { "gluten_free", "high_protein" },
                    ModifierGroups = new List<ModifierGroup>
                    {
                        new()
                        {
                            Name = "Punto de Cocción",
                            MinSelection = 1,
                            MaxSelection = 1,
                            IsRequired = true,
                            Options = new List<ModifierOption>
                            {
                                new() { Name = "Jugoso", PriceDelta = 0m, IsAvailable = true },
                                new() { Name = "A Punto", PriceDelta = 0m, IsAvailable = true },
                                new() { Name = "Bien Cocido", PriceDelta = 0m, IsAvailable = true }
                            }
                        }
                    }
                },
                new()
                {
                    TenantId = tenant.Id,
                    CategoryId = catPrincipales.Id,
                    Name = "Sorrentinos de Calabaza Asada y Parmesano",
                    Description = "Pasta fresca artesanal con salsa suave de salvia y nueces tostadas.",
                    BasePrice = 14800.00m,
                    DisplayOrder = 2,
                    IsAvailable = true,
                    Allergens = new[] { "gluten", "dairy", "nuts", "eggs" },
                    DietaryLabels = new[] { "vegetarian" }
                },
                new()
                {
                    TenantId = tenant.Id,
                    CategoryId = catPostres.Id,
                    Name = "Flan Casero Mixto de Campo",
                    Description = "Acompañado con dulce de leche repostero y crema chantilly.",
                    BasePrice = 6200.00m,
                    DisplayOrder = 1,
                    IsAvailable = true,
                    Allergens = new[] { "dairy", "eggs" },
                    DietaryLabels = new[] { "vegetarian", "gluten_free" }
                }
            };

            context.MenuItems.AddRange(items);
            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Seeded 3 categories and {Count} menu items for '{TenantName}'.", items.Count, tenant.Name);
        }

        // 4. Seed QR Tables
        for (var tableNumber = 1; tableNumber <= 5; tableNumber++)
        {
            var tableExists = await context.Tables
                .IgnoreQueryFilters()
                .AnyAsync(t => t.TenantId == tenant.Id && t.TableNumber == tableNumber, cancellationToken);

            if (!tableExists)
            {
                context.Tables.Add(new Table
                {
                    TenantId = tenant.Id,
                    TableNumber = tableNumber,
                    Token = $"mesa-{tableNumber}-{tenant.Slug}",
                    IsActive = true
                });
            }
        }
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Database seeding completed successfully.");
    }
}
