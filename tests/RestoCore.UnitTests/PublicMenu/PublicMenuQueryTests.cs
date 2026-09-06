namespace RestoCore.UnitTests.PublicMenu;

using FluentAssertions;
using RestoCore.Application.Features.PublicMenu.DTOs;
using System.Security.Cryptography;
using System.Text.Json;
using Xunit;

public class PublicMenuDtoTests
{
    [Fact]
    public void Should_Generate_Consistent_ETag_For_Identical_Menu()
    {
        // Arrange
        var menu1 = new PublicMenuResponse
        {
            TenantName = "Pizzeria",
            TenantSlug = "pizzeria",
            Branding = new PublicBrandingDto { PrimaryColor = "#FF0000" }
        };

        var menu2 = new PublicMenuResponse
        {
            TenantName = "Pizzeria",
            TenantSlug = "pizzeria",
            Branding = new PublicBrandingDto { PrimaryColor = "#FF0000" }
        };

        // Act
        var hash1 = Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(menu1)));
        var hash2 = Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(menu2)));

        // Assert
        hash1.Should().Be(hash2);
    }
}
