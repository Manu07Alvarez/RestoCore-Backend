namespace RestoCore.UnitTests.Features.Auth;

using FluentAssertions;
using RestoCore.Infrastructure.Identity;
using Xunit;

public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void HashPassword_ShouldGenerateFormattedHashAndVerifyCorrectPassword()
    {
        // Act
        var hash = _hasher.HashPassword("admin123");

        // Assert
        hash.Should().StartWith("v1$100000$");
        _hasher.VerifyPassword("admin123", hash).Should().BeTrue();
        _hasher.VerifyPassword("wrong-password", hash).Should().BeFalse();
    }

    [Fact]
    public void HashPassword_SameInputTwice_ShouldProduceDifferentSalts()
    {
        var hash1 = _hasher.HashPassword("secret123");
        var hash2 = _hasher.HashPassword("secret123");

        hash1.Should().NotBe(hash2);
        _hasher.VerifyPassword("secret123", hash1).Should().BeTrue();
        _hasher.VerifyPassword("secret123", hash2).Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WithMalformedHash_ShouldReturnFalse()
    {
        _hasher.VerifyPassword("admin123", "invalid-hash").Should().BeFalse();
        _hasher.VerifyPassword("admin123", "").Should().BeFalse();
    }
}
