namespace RestoCore.UnitTests.Features.Analytics;

using FluentAssertions;
using Moq;
using RestoCore.Application.Common.Interfaces;
using RestoCore.Application.Common.Interfaces.Repositories;
using RestoCore.Application.Features.Analytics.Queries;
using RestoCore.Domain.Entities;

public class GetAnalyticsDashboardQueryHandlerTests
{
    private readonly Mock<IAnalyticsRepository> _analyticsRepositoryMock = new();
    private readonly Mock<ICatalogRepository> _catalogRepositoryMock = new();
    private readonly Mock<ITenantRepository> _tenantRepositoryMock = new();
    private readonly Mock<ITenantContext> _tenantContextMock = new();
    private readonly Guid _tenantId = Guid.NewGuid();

    public GetAnalyticsDashboardQueryHandlerTests()
    {
        _tenantContextMock.Setup(t => t.TenantId).Returns(_tenantId);
        _analyticsRepositoryMock
            .Setup(r => r.GetDailyTrafficAsync(_tenantId, It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DailyMenuTrafficSummary>());
        _analyticsRepositoryMock
            .Setup(r => r.GetItemViewsAsync(_tenantId, It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DailyItemViewSummary>());
        _analyticsRepositoryMock
            .Setup(r => r.GetCategoryInteractionsAsync(_tenantId, It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DailyCategoryInteractionSummary>());
        _analyticsRepositoryMock
            .Setup(r => r.GetHourlyTrafficAsync(_tenantId, It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<HourlyMenuTrafficSummary>());
        _analyticsRepositoryMock
            .Setup(r => r.GetAllTenantItemsWithDeletedAsync(_tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MenuItem>());
        _catalogRepositoryMock
            .Setup(r => r.GetCategoriesByTenantAsync(_tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Category>());
    }

    [Fact]
    public async Task Handle_ClassifiesMenuEngineeringMatrixIntoFourQuadrants_With30PercentCostFallback()
    {
        _tenantRepositoryMock
            .Setup(r => r.GetTenantByIdAsync(_tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Tenant { Id = _tenantId, Name = "Test Tenant", Slug = "test", PlanTier = "Standard" });

        var starId = Guid.NewGuid();       // High views (100), High margin (Base 200, null cost -> 140 margin)
        var plowhorseId = Guid.NewGuid();  // High views (90), Low margin (Base 100, Cost 80 -> 20 margin)
        var puzzleId = Guid.NewGuid();     // Low views (10), High margin (Base 200, Cost 50 -> 150 margin)
        var dogId = Guid.NewGuid();        // Low views (5), Low margin (Base 80, Cost 60 -> 20 margin)
        var deletedId = Guid.NewGuid();    // Soft-deleted item: appears in top_viewed_items, excluded from matrix

        var items = new List<MenuItem>
        {
            new() { Id = starId, TenantId = _tenantId, Name = "Star Dish", BasePrice = 200m, EstimatedCost = null, IsDeleted = false },
            new() { Id = plowhorseId, TenantId = _tenantId, Name = "Plowhorse Dish", BasePrice = 100m, EstimatedCost = 80m, IsDeleted = false },
            new() { Id = puzzleId, TenantId = _tenantId, Name = "Puzzle Dish", BasePrice = 200m, EstimatedCost = 50m, IsDeleted = false },
            new() { Id = dogId, TenantId = _tenantId, Name = "Dog Dish", BasePrice = 80m, EstimatedCost = 60m, IsDeleted = false },
            new() { Id = deletedId, TenantId = _tenantId, Name = "Archived Dish", BasePrice = 300m, EstimatedCost = 50m, IsDeleted = true }
        };

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var itemViews = new List<DailyItemViewSummary>
        {
            new() { TenantId = _tenantId, ItemId = starId, DateUtc = today, ViewsCount = 100 },
            new() { TenantId = _tenantId, ItemId = plowhorseId, DateUtc = today, ViewsCount = 90 },
            new() { TenantId = _tenantId, ItemId = puzzleId, DateUtc = today, ViewsCount = 10 },
            new() { TenantId = _tenantId, ItemId = dogId, DateUtc = today, ViewsCount = 5 },
            new() { TenantId = _tenantId, ItemId = deletedId, DateUtc = today, ViewsCount = 50 }
        };

        _analyticsRepositoryMock
            .Setup(r => r.GetAllTenantItemsWithDeletedAsync(_tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(items);
        _analyticsRepositoryMock
            .Setup(r => r.GetItemViewsAsync(_tenantId, It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(itemViews);

        var handler = new GetAnalyticsDashboardQueryHandler(
            _analyticsRepositoryMock.Object,
            _catalogRepositoryMock.Object,
            _tenantRepositoryMock.Object,
            _tenantContextMock.Object);

        var response = await handler.Handle(new GetAnalyticsDashboardQuery("2026-10-01", "2026-11-04"), CancellationToken.None);

        var matrix = response.PriorityHighMetrics.MenuEngineeringMatrix;
        matrix.Stars.Should().ContainSingle().Which.Should().Be(starId);
        matrix.Plowhorses.Should().ContainSingle().Which.Should().Be(plowhorseId);
        matrix.Puzzles.Should().ContainSingle().Which.Should().Be(puzzleId);
        matrix.Dogs.Should().ContainSingle().Which.Should().Be(dogId);

        // Soft-deleted dish is preserved in historical top_viewed_items but excluded from active matrix
        response.PriorityHighMetrics.TopViewedItems.Should().Contain(x => x.ItemId == deletedId && x.ViewsCount == 50);
        response.PriorityMediumMetrics.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenZeroTotalViews_ReturnsEmptyArraysForAllQuadrants()
    {
        _tenantRepositoryMock
            .Setup(r => r.GetTenantByIdAsync(_tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Tenant { Id = _tenantId, Name = "Zero Traffic Tenant", Slug = "zero", PlanTier = "Standard" });

        _analyticsRepositoryMock
            .Setup(r => r.GetAllTenantItemsWithDeletedAsync(_tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MenuItem>
            {
                new() { Id = Guid.NewGuid(), TenantId = _tenantId, Name = "Unviewed Dish", BasePrice = 100m, IsDeleted = false }
            });

        var handler = new GetAnalyticsDashboardQueryHandler(
            _analyticsRepositoryMock.Object,
            _catalogRepositoryMock.Object,
            _tenantRepositoryMock.Object,
            _tenantContextMock.Object);

        var response = await handler.Handle(new GetAnalyticsDashboardQuery(), CancellationToken.None);

        var matrix = response.PriorityHighMetrics.MenuEngineeringMatrix;
        matrix.Stars.Should().BeEmpty();
        matrix.Plowhorses.Should().BeEmpty();
        matrix.Puzzles.Should().BeEmpty();
        matrix.Dogs.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenUniformVarianceAndPositiveMargin_ClassifiesIntoStars()
    {
        _tenantRepositoryMock
            .Setup(r => r.GetTenantByIdAsync(_tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Tenant { Id = _tenantId, Name = "Uniform Tenant", Slug = "uniform", PlanTier = "Standard" });

        var item1 = Guid.NewGuid();
        var item2 = Guid.NewGuid();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        _analyticsRepositoryMock
            .Setup(r => r.GetAllTenantItemsWithDeletedAsync(_tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MenuItem>
            {
                new() { Id = item1, TenantId = _tenantId, Name = "Dish 1", BasePrice = 100m, EstimatedCost = 30m },
                new() { Id = item2, TenantId = _tenantId, Name = "Dish 2", BasePrice = 100m, EstimatedCost = 30m }
            });
        _analyticsRepositoryMock
            .Setup(r => r.GetItemViewsAsync(_tenantId, It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DailyItemViewSummary>
            {
                new() { TenantId = _tenantId, ItemId = item1, DateUtc = today, ViewsCount = 25 },
                new() { TenantId = _tenantId, ItemId = item2, DateUtc = today, ViewsCount = 25 }
            });

        var handler = new GetAnalyticsDashboardQueryHandler(
            _analyticsRepositoryMock.Object,
            _catalogRepositoryMock.Object,
            _tenantRepositoryMock.Object,
            _tenantContextMock.Object);

        var response = await handler.Handle(new GetAnalyticsDashboardQuery(), CancellationToken.None);

        response.PriorityHighMetrics.MenuEngineeringMatrix.Stars.Should().BeEquivalentTo(new[] { item1, item2 });
    }

    [Fact]
    public async Task Handle_WhenProTierTenant_PopulatesPriorityMediumMetricsWithoutTableTurnover()
    {
        _tenantRepositoryMock
            .Setup(r => r.GetTenantByIdAsync(_tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Tenant { Id = _tenantId, Name = "Pro Tenant", Slug = "pro", PlanTier = "Pro" });

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        _analyticsRepositoryMock
            .Setup(r => r.GetDailyTrafficAsync(_tenantId, It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DailyMenuTrafficSummary>
            {
                new() { TenantId = _tenantId, DateUtc = today, QrVisitsCount = 200, MobileCount = 175, TabletCount = 10, DesktopCount = 15 }
            });
        _analyticsRepositoryMock
            .Setup(r => r.GetHourlyTrafficAsync(_tenantId, It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<HourlyMenuTrafficSummary>
            {
                new() { TenantId = _tenantId, DateUtc = today, HourOfDay = 21, VisitsCount = 85 }
            });

        var handler = new GetAnalyticsDashboardQueryHandler(
            _analyticsRepositoryMock.Object,
            _catalogRepositoryMock.Object,
            _tenantRepositoryMock.Object,
            _tenantContextMock.Object);

        var response = await handler.Handle(new GetAnalyticsDashboardQuery(), CancellationToken.None);

        response.PriorityMediumMetrics.Should().NotBeNull();
        response.PriorityMediumMetrics!.PeakHours.Should().HaveCount(24);
        response.PriorityMediumMetrics.PeakHours.Single(h => h.Hour == 21).VisitsCount.Should().Be(85);
        response.PriorityMediumMetrics.DeviceBreakdown.MobilePercentage.Should().Be(92.5m);
        response.PriorityMediumMetrics.DeviceBreakdown.DesktopPercentage.Should().Be(7.5m);
    }
}
