namespace RestoCore.Application.Features.Tenants.DTOs;

using System.Text.Json.Serialization;

public record TenantProfileResponse(
    Guid Id,
    string Name,
    string Slug,
    [property: JsonPropertyName("phone_number")] string? PhoneNumber,
    string? Address,
    [property: JsonPropertyName("business_hours")] string? BusinessHours
);
