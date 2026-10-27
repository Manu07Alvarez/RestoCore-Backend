namespace RestoCore.UnitTests.Features.Auth;

using FluentAssertions;
using Moq;
using RestoCore.Application.Common.Interfaces;
using RestoCore.Application.Common.Interfaces.Repositories;
using RestoCore.Application.Features.Auth.Commands;
using RestoCore.Domain.Entities;
using Xunit;

public class LoginCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();

    [Fact]
    public async Task Handle_WhenCredentialsAreValid_ShouldReturnAuthenticatedUserResult()
    {
        // Arrange
        var tenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "admin@bistrogourmet.com",
            Name = "Chef Marco",
            PasswordHash = "v1$100000$salt$hash",
            PrimaryRole = "owner",
            Roles = new[] { "owner", "tenant_admin", "kitchen_staff" },
            TenantId = tenantId,
            IsActive = true,
            Tenant = new Tenant
            {
                Id = tenantId,
                Name = "El Gato Negro",
                Slug = "el-gato-negro",
                Status = "Active"
            }
        };

        _userRepoMock.Setup(r => r.GetUserByEmailWithTenantAsync("admin@bistrogourmet.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock.Setup(h => h.VerifyPassword("admin123", user.PasswordHash))
            .Returns(true);

        var handler = new LoginCommandHandler(_userRepoMock.Object, _passwordHasherMock.Object);

        // Act
        var result = await handler.Handle(new LoginCommand("admin@bistrogourmet.com", "admin123"), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(userId.ToString());
        result.Email.Should().Be("admin@bistrogourmet.com");
        result.Name.Should().Be("Chef Marco");
        result.PrimaryRole.Should().Be("owner");
        result.TenantId.Should().Be(tenantId.ToString());
        result.TenantSlug.Should().Be("el-gato-negro");
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetUserByEmailWithTenantAsync("unknown@restocore.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var handler = new LoginCommandHandler(_userRepoMock.Object, _passwordHasherMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(new LoginCommand("unknown@restocore.com", "admin123"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenPasswordIsIncorrect_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@bistrogourmet.com",
            PasswordHash = "v1$100000$salt$hash",
            IsActive = true
        };

        _userRepoMock.Setup(r => r.GetUserByEmailWithTenantAsync("admin@bistrogourmet.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock.Setup(h => h.VerifyPassword("wrongpass", user.PasswordHash))
            .Returns(false);

        var handler = new LoginCommandHandler(_userRepoMock.Object, _passwordHasherMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(new LoginCommand("admin@bistrogourmet.com", "wrongpass"), CancellationToken.None));
    }
}
