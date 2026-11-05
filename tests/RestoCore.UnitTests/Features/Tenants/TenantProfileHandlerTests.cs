namespace RestoCore.UnitTests.Features.Tenants;

using FluentAssertions;
using Moq;
using RestoCore.Application.Common.Interfaces;
using RestoCore.Application.Common.Interfaces.Repositories;
using RestoCore.Application.Features.Tenants.Commands;
using RestoCore.Application.Features.Tenants.Queries;
using RestoCore.Domain.Entities;
using RestoCore.Domain.ValueObjects;
using Xunit;

public class TenantProfileHandlerTests
{
    private readonly Mock<ITenantRepository> _tenantRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ITenantContext> _tenantContextMock = new();
    private readonly Mock<ICdnPurgeService> _cdnPurgeMock = new();
    private readonly Guid _tenantId = Guid.NewGuid();

    public TenantProfileHandlerTests()
    {
        _tenantContextMock.Setup(t => t.TenantId).Returns(_tenantId);
        _tenantContextMock.Setup(t => t.TenantSlug).Returns("don-pepe");
    }

    [Fact]
    public async Task GetTenantProfile_ShouldReturnCurrentTenantProfileWithBranding()
    {
        // Arrange
        var tenant = new Tenant
        {
            Id = _tenantId,
            Name = "Parrilla Don Pepe",
            Slug = "don-pepe",
            PhoneNumber = "+54 11 4567-8901",
            Address = "Av. Corrientes 1234",
            BusinessHours = "Lunes a Domingo 12:00 a 00:00",
            BrandingConfig = new BrandingConfig
            {
                LogoUrl = "https://cdn.restocore.local/logos/don-pepe.webp"
            }
        };
        tenant.BrandingConfig.ApplyRgbColor("rgb(200, 40, 50)");

        _tenantRepoMock.Setup(r => r.GetTenantByIdAsync(_tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        var handler = new GetTenantProfileQueryHandler(_tenantRepoMock.Object, _tenantContextMock.Object);

        // Act
        var result = await handler.Handle(new GetTenantProfileQuery(), CancellationToken.None);

        // Assert
        result.Id.Should().Be(_tenantId);
        result.Name.Should().Be("Parrilla Don Pepe");
        result.Slug.Should().Be("don-pepe");
        result.PhoneNumber.Should().Be("+54 11 4567-8901");
        result.Address.Should().Be("Av. Corrientes 1234");
        result.BusinessHours.Should().Be("Lunes a Domingo 12:00 a 00:00");
        result.PrimaryColorRgb.Should().Be("rgb(200, 40, 50)");
        result.LogoUrl.Should().Be("https://cdn.restocore.local/logos/don-pepe.webp");
        result.Branding.Should().NotBeNull();
        result.Branding!.PrimaryColor.Should().Be("#C82832");
    }

    [Fact]
    public async Task UpdateTenantProfile_ShouldUpdateAllFieldsAndInvalidateCache()
    {
        // Arrange
        var tenant = new Tenant
        {
            Id = _tenantId,
            Name = "Parrilla Don Pepe",
            Slug = "don-pepe"
        };

        _tenantRepoMock.Setup(r => r.GetTenantByIdAsync(_tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        var handler = new UpdateTenantProfileCommandHandler(
            _tenantRepoMock.Object,
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _cdnPurgeMock.Object);

        var command = new UpdateTenantProfileCommand(
            "Parrilla Don Pepe & Asador",
            "+54 11 5555-6666",
            "Av. Santa Fe 4321",
            "Martes a Domingo 12:00 a 01:00",
            "rgb(25, 135, 84)",
            "https://cdn.restocore.local/logos/don-pepe-new.webp");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Name.Should().Be("Parrilla Don Pepe & Asador");
        result.PhoneNumber.Should().Be("+54 11 5555-6666");
        result.Address.Should().Be("Av. Santa Fe 4321");
        result.BusinessHours.Should().Be("Martes a Domingo 12:00 a 01:00");
        result.PrimaryColorRgb.Should().Be("rgb(25, 135, 84)");
        result.LogoUrl.Should().Be("https://cdn.restocore.local/logos/don-pepe-new.webp");
        result.Branding!.PrimaryColor.Should().Be("#198754");

        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cdnPurgeMock.Verify(c => c.PurgeTenantCacheAsync("don-pepe", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PatchTenantProfile_ShouldOnlyUpdateSuppliedFieldsIncludingRgbAndLogo()
    {
        // Arrange
        var tenant = new Tenant
        {
            Id = _tenantId,
            Name = "Parrilla Don Pepe",
            Slug = "don-pepe",
            PhoneNumber = "+54 11 1111-2222",
            Address = "Av. Corrientes 1234",
            BusinessHours = "Lunes a Domingo 12:00 a 00:00"
        };

        _tenantRepoMock.Setup(r => r.GetTenantByIdAsync(_tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        var handler = new PatchTenantProfileCommandHandler(
            _tenantRepoMock.Object,
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _cdnPurgeMock.Object);

        var command = new PatchTenantProfileCommand(
            null,
            "+54 11 9999-8888",
            null,
            null,
            "12, 110, 253",
            "https://cdn.restocore.local/logos/logo-empresa.png");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Name.Should().Be("Parrilla Don Pepe");
        result.PhoneNumber.Should().Be("+54 11 9999-8888");
        result.Address.Should().Be("Av. Corrientes 1234");
        result.BusinessHours.Should().Be("Lunes a Domingo 12:00 a 00:00");
        result.PrimaryColorRgb.Should().Be("rgb(12, 110, 253)");
        result.LogoUrl.Should().Be("https://cdn.restocore.local/logos/logo-empresa.png");
        result.Branding!.PrimaryColor.Should().Be("#0C6EFD");

        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cdnPurgeMock.Verify(c => c.PurgeTenantCacheAsync("don-pepe", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAndPatchBranding_ShouldModifyRgbColorAndCompanyLogo()
    {
        // Arrange
        var tenant = new Tenant
        {
            Id = _tenantId,
            Name = "Parrilla Don Pepe",
            Slug = "don-pepe"
        };

        _tenantRepoMock.Setup(r => r.GetTenantByIdAsync(_tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        var patchHandler = new PatchBrandingCommandHandler(
            _tenantRepoMock.Object,
            _unitOfWorkMock.Object,
            _tenantContextMock.Object,
            _cdnPurgeMock.Object);

        var getHandler = new GetBrandingQueryHandler(_tenantRepoMock.Object, _tenantContextMock.Object);

        // Act
        await patchHandler.Handle(
            new PatchBrandingCommand(
                PrimaryColorRgb: "rgb(255, 87, 51)",
                LogoUrl: "https://cdn.restocore.local/logos/brand-logo.webp"),
            CancellationToken.None);

        var branding = await getHandler.Handle(new GetBrandingQuery(), CancellationToken.None);

        // Assert
        branding.PrimaryColorRgb.Should().Be("rgb(255, 87, 51)");
        branding.PrimaryColor.Should().Be("#FF5733");
        branding.LogoUrl.Should().Be("https://cdn.restocore.local/logos/brand-logo.webp");
        _cdnPurgeMock.Verify(c => c.PurgeTenantCacheAsync("don-pepe", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("rgb(300, 10, 10)")]
    [InlineData("invalid-color")]
    [InlineData("")]
    public void BrandingConfig_ApplyRgbColor_ShouldThrowOnInvalidColor(string invalidColor)
    {
        var config = new BrandingConfig();
        var act = () => config.ApplyRgbColor(invalidColor);
        act.Should().Throw<ArgumentException>();
    }
}
