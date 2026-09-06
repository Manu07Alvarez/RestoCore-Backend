namespace RestoCore.Infrastructure.Authorization;

using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public interface IOpaClient
{
    Task<bool> EvaluatePolicyAsync(OpaInput input, CancellationToken cancellationToken = default);
}

public class OpaInput
{
    public string Method { get; set; } = string.Empty;
    public string[] Path { get; set; } = Array.Empty<string>();
    public OpaUser User { get; set; } = new();
    public OpaResource Resource { get; set; } = new();
    public string Action { get; set; } = string.Empty;
}

public class OpaUser
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string[] Roles { get; set; } = Array.Empty<string>();
}

public class OpaResource
{
    public string Type { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
}

public class OpaClient : IOpaClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpaClient> _logger;

    public OpaClient(HttpClient httpClient, IConfiguration configuration, ILogger<OpaClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        var baseUrl = configuration["Opa:BaseUrl"] ?? "http://localhost:8181";
        if (_httpClient.BaseAddress == null)
        {
            _httpClient.BaseAddress = new Uri(baseUrl);
        }
    }

    public async Task<bool> EvaluatePolicyAsync(OpaInput input, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/v1/data/restocore/authz/allow", new { input }, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("OPA evaluation failed with status {StatusCode}", response.StatusCode);
                return false;
            }

            var result = await response.Content.ReadFromJsonAsync<OpaResponse>(cancellationToken);
            return result?.Result ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error communicating with OPA policy server");
            return false;
        }
    }

    private class OpaResponse
    {
        public bool Result { get; set; }
    }
}
