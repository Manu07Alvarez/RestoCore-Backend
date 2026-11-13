namespace RestoCore.Infrastructure.Authorization;

using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
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
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("tenant_id")]
    public string TenantId { get; set; } = string.Empty;

    [JsonPropertyName("roles")]
    public string[] Roles { get; set; } = Array.Empty<string>();

    [JsonPropertyName("permissions")]
    public string[] Permissions { get; set; } = Array.Empty<string>();
}

public class OpaResource
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("tenant_id")]
    public string TenantId { get; set; } = string.Empty;
}

public class OpaClient : IOpaClient
{
    private static readonly TimeSpan DecisionCacheTtl = TimeSpan.FromSeconds(30);

    private readonly HttpClient _httpClient;
    private readonly ILogger<OpaClient> _logger;
    private readonly IMemoryCache? _memoryCache;

    public OpaClient(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<OpaClient> logger,
        IMemoryCache? memoryCache = null)
    {
        _httpClient = httpClient;
        _logger = logger;
        _memoryCache = memoryCache;
        var baseUrl = configuration["Opa:BaseUrl"] ?? "http://localhost:8181";
        if (_httpClient.BaseAddress == null)
        {
            _httpClient.BaseAddress = new Uri(baseUrl);
        }
    }

    public async Task<bool> EvaluatePolicyAsync(OpaInput input, CancellationToken cancellationToken = default)
    {
        var cacheKey = BuildDecisionCacheKey(input);
        if (_memoryCache != null && _memoryCache.TryGetValue(cacheKey, out bool cachedDecision))
        {
            return cachedDecision;
        }

        try
        {
            var response = await _httpClient.PostAsJsonAsync("/v1/data/restocore/authz/allow", new { input }, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("OPA evaluation failed with status {StatusCode}", response.StatusCode);
                return false;
            }

            var result = await response.Content.ReadFromJsonAsync<OpaResponse>(cancellationToken);
            var allowed = result?.Result ?? false;

            _memoryCache?.Set(cacheKey, allowed, DecisionCacheTtl);
            return allowed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error communicating with OPA policy server");
            return false;
        }
    }

    private static string BuildDecisionCacheKey(OpaInput input)
    {
        var roles = string.Join(",", input.User.Roles);
        var perms = string.Join(",", input.User.Permissions);
        return $"opa:decision:{input.User.Id}:{input.User.TenantId}:{roles}:{perms}:{input.Action}:{input.Resource.Type}:{input.Resource.TenantId}";
    }

    private class OpaResponse
    {
        public bool Result { get; set; }
    }
}
