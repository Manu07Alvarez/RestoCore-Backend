namespace RestoCore.UnitTests.Features.Analytics;

using FluentAssertions;
using Moq;
using RestoCore.Application.Common.Interfaces;
using RestoCore.Application.Common.Interfaces.Repositories;
using RestoCore.Application.Features.Analytics.Commands;
using RestoCore.Application.Features.Analytics.DTOs;
using RestoCore.Domain.Entities;

public class CollectAnalyticsEventCommandHandlerTests
{
    private readonly Mock<ITenantRepository> _tenantRepositoryMock = new();
    private readonly Mock<IAnalyticsEventQueue> _eventQueueMock = new();
    private readonly Guid _tenantId = Guid.NewGuid();
    private const string TenantSlug = "bistro-gourmet";

    public CollectAnalyticsEventCommandHandlerTests()
    {
        _tenantRepositoryMock
            .Setup(r => r.GetTenantBySlugAsync(TenantSlug, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Tenant
            {
                Id = _tenantId,
                Slug = TenantSlug,
                Name = "Bistro Gourmet",
                Status = "Active"
            });
    }

    [Fact]
    public async Task Handle_WhenValidMenuView_EnqueuesMessageWithoutPii()
    {
        AnalyticsEventMessage? captured = null;
        _eventQueueMock
            .Setup(q => q.EnqueueAsync(It.IsAny<AnalyticsEventMessage>(), It.IsAny<CancellationToken>()))
            .Callback<AnalyticsEventMessage, CancellationToken>((msg, _) => captured = msg)
            .Returns(ValueTask.CompletedTask);

        var handler = new CollectAnalyticsEventCommandHandler(_tenantRepositoryMock.Object, _eventQueueMock.Object);
        var result = await handler.Handle(new CollectAnalyticsEventCommand("menu_view", TenantSlug, null, null, "mobile"), CancellationToken.None);

        result.Should().BeTrue();
        captured.Should().NotBeNull();
        captured!.TenantId.Should().Be(_tenantId);
        captured.EventType.Should().Be("menu_view");
        captured.DeviceType.Should().Be("mobile");
    }

    [Fact]
    public async Task Handle_WhenItemViewWithoutItemId_ThrowsArgumentException()
    {
        var handler = new CollectAnalyticsEventCommandHandler(_tenantRepositoryMock.Object, _eventQueueMock.Object);
        var act = async () => await handler.Handle(new CollectAnalyticsEventCommand("item_view", TenantSlug, null, null, "mobile"), CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*item_id is required*");
    }

    [Fact]
    public async Task Handle_WhenCategoryClickWithoutCategoryId_ThrowsArgumentException()
    {
        var handler = new CollectAnalyticsEventCommandHandler(_tenantRepositoryMock.Object, _eventQueueMock.Object);
        var act = async () => await handler.Handle(new CollectAnalyticsEventCommand("category_click", TenantSlug, null, null, "desktop"), CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*category_id is required*");
    }

    [Fact]
    public async Task Handle_WhenInvalidEventType_ThrowsArgumentException()
    {
        var handler = new CollectAnalyticsEventCommandHandler(_tenantRepositoryMock.Object, _eventQueueMock.Object);
        var act = async () => await handler.Handle(new CollectAnalyticsEventCommand("table_turnover", TenantSlug), CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*event_type must be one of*");
    }

    [Fact]
    public async Task Handle_WhenUnknownTenant_ThrowsKeyNotFoundException()
    {
        var handler = new CollectAnalyticsEventCommandHandler(_tenantRepositoryMock.Object, _eventQueueMock.Object);
        var act = async () => await handler.Handle(new CollectAnalyticsEventCommand("menu_view", "non-existent-slug"), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
