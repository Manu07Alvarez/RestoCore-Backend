namespace RestoCore.UnitTests.Features.Tenants;

using FluentAssertions;
using Moq;
using RestoCore.Application.Common.Interfaces;
using RestoCore.Application.Common.Interfaces.Repositories;
using RestoCore.Application.Features.Tenants.Commands;
using RestoCore.Application.Features.Tenants.Queries;
using RestoCore.Domain.Entities;
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
    public async Task GetTenantProfile_ShouldReturnCurrentTenantProfile()
    {
        // Arrange
        var tenant = new Tenant
        {
            Id = _tenantId,
            Name = "Parrilla Don Pepe",
            Slug = "don-pepe",
            PhoneNumber = "+54 11 4567-8901",
            Address = "Av. Corrientes 1234",
            BusinessHours = "Lunes a Domingo 12:00 a 00:00"
        };

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
            "Martes a Domingo 12:00 a 01:00");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Name.Should().Be("Parrilla Don Pepe & Asador");
        result.PhoneNumber.Should().Be("+54 11 5555-6666");
        result.Address.Should().Be("Av. Santa Fe 4321");
        result.BusinessHours.Should().Be("Martes a Domingo 12:00 a 01:00");

        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cdnPurgeMock.Verify(c => c.PurgeTenantCacheAsync("don-pepe", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PatchTenantProfile_ShouldOnlyUpdateSuppliedFields()
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
            null);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Name.Should().Be("Parrilla Don Pepe");
        result.PhoneNumber.Should().Be("+54 11 9999-8888");
        result.Address.Should().Be("Av. Corrientes 1234");
        result.BusinessHours.Should().Be("Lunes a Domingo 12:00 a 00:00");

        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cdnPurgeMock.Verify(c => c.PurgeTenantCacheAsync("don-pepe", It.IsAny<CancellationToken>()), Times.Once);
    }
}
