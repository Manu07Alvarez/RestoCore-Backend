namespace RestoCore.Application.Features.Tenants.DTOs;

using System.Text.Json.Serialization;
using RestoCore.Domain.ValueObjects;

public record TenantProfileResponse(
    Guid Id,
    string Name,
    string Slug,
    [property: JsonPropertyName("phone_number")] string? PhoneNumber,
    string? Address,
    [property: JsonPropertyName("business_hours")] string? BusinessHours,
    [property: JsonPropertyName("primary_color_rgb")] string? PrimaryColorRgb = null,
    [property: JsonPropertyName("logo_url")] string? LogoUrl = null,
    [property: JsonPropertyName("branding")] BrandingConfig? Branding = null
);
