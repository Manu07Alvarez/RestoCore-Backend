namespace RestoCore.IntegrationTests.Fixtures;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RestoCore.Application.Common.Interfaces;
using RestoCore.Domain.Entities;
using RestoCore.Domain.ValueObjects;
using RestoCore.Infrastructure.Persistence;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registration
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            var dbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IApplicationDbContext));
            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            // In local/test environments where Docker/PostgreSQL is mocked or running
            var dbName = Guid.NewGuid().ToString("N");
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql($"Host=localhost;Database=restocore_test_{dbName};Username=postgres;Password=postgres");
            });
            services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        });
    }
}
