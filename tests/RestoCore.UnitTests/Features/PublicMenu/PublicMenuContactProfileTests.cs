namespace RestoCore.UnitTests.Features.PublicMenu;

using FluentAssertions;
using Moq;
using RestoCore.Application.Common.Interfaces.Repositories;
using RestoCore.Application.Features.PublicMenu.Queries;
using RestoCore.Domain.Entities;
using Xunit;

public class PublicMenuContactProfileTests
{
    private readonly Mock<ICatalogRepository> _catalogRepoMock = new();

    [Fact]
    public async Task Handle_WhenTenantHasContactProfile_ShouldIncludeContactFieldsInPublicMenuResponse()
    {
        // Arrange
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = "Parrilla Don Pepe",
            Slug = "don-pepe",
            Status = "Active",
            PhoneNumber = "+54 11 4567-8901",
            Address = "Av. Corrientes 1234, CABA",
            BusinessHours = "Lunes a Domingo de 12:00 a 00:00 hs",
            Categories = new List<Category>()
        };

        _catalogRepoMock.Setup(r => r.GetTenantWithCatalogAsync("don-pepe", It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        var handler = new GetPublicMenuQueryHandler(_catalogRepoMock.Object);

        // Act
        var (response, etag) = await handler.Handle(new GetPublicMenuQuery("don-pepe"), CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.TenantName.Should().Be("Parrilla Don Pepe");
        response.TenantSlug.Should().Be("don-pepe");
        response.PhoneNumber.Should().Be("+54 11 4567-8901");
        response.Address.Should().Be("Av. Corrientes 1234, CABA");
        response.BusinessHours.Should().Be("Lunes a Domingo de 12:00 a 00:00 hs");
        etag.Should().NotBeNullOrWhiteSpace();
    }
}
