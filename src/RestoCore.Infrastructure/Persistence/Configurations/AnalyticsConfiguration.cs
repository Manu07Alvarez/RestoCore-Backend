namespace RestoCore.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestoCore.Domain.Entities;

public class AnalyticsConfiguration :
    IEntityTypeConfiguration<DailyMenuTrafficSummary>,
    IEntityTypeConfiguration<DailyItemViewSummary>,
    IEntityTypeConfiguration<DailyCategoryInteractionSummary>,
    IEntityTypeConfiguration<HourlyMenuTrafficSummary>
{
    public void Configure(EntityTypeBuilder<DailyMenuTrafficSummary> builder)
    {
        builder.ToTable("daily_menu_traffic_summaries");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.TenantId, x.DateUtc }).IsUnique();
        builder.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public void Configure(EntityTypeBuilder<DailyItemViewSummary> builder)
    {
        builder.ToTable("daily_item_view_summaries");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.TenantId, x.ItemId, x.DateUtc }).IsUnique();
        builder.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Item)
            .WithMany()
            .HasForeignKey(x => x.ItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public void Configure(EntityTypeBuilder<DailyCategoryInteractionSummary> builder)
    {
        builder.ToTable("daily_category_interaction_summaries");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.TenantId, x.CategoryId, x.DateUtc }).IsUnique();
        builder.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Category)
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public void Configure(EntityTypeBuilder<HourlyMenuTrafficSummary> builder)
    {
        builder.ToTable("hourly_menu_traffic_summaries");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.TenantId, x.DateUtc, x.HourOfDay }).IsUnique();
        builder.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
