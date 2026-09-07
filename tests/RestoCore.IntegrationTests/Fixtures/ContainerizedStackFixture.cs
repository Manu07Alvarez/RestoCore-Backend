namespace RestoCore.IntegrationTests.Fixtures;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestoCore.Application.Common.Interfaces;
using RestoCore.Infrastructure.Persistence;

public class ContainerizedStackFixture : WebApplicationFactory<Program>
{
    public const string TenantIdHeader = "X-Tenant-Id";
    public const string TenantRoleHeader = "X-User-Role";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((ctx, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Port=5432;Database=restocore_dev;Username=postgres;Password=postgres_dev_password",
                ["Redis:Configuration"] = "localhost:6379",
                ["Opa:BaseUrl"] = "http://localhost:8181",
                ["Storage:ServiceUrl"] = "http://localhost:8333",
                ["Storage:PublicBaseUrl"] = "http://localhost:8333",
                ["Storage:BucketName"] = "restocore-images"
            });
        });
    }
}
