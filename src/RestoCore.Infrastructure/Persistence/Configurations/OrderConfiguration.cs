namespace RestoCore.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestoCore.Domain.Entities;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(o => o.TotalAmount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(o => o.ItemsSnapshot)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.HasOne(o => o.Tenant)
            .WithMany()
            .HasForeignKey(o => o.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(o => o.Table)
            .WithMany()
            .HasForeignKey(o => o.TableId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(o => new { o.TenantId, o.CreatedAt });
        builder.HasIndex(o => o.Status);

        // ADR-0014: GIN index on JSONB items_snapshot for fast @> containment queries
        builder.HasIndex(o => o.ItemsSnapshot)
            .HasMethod("gin")
            .HasDatabaseName("ix_orders_items_snapshot_gin");

        // ADR-0014: Partial B-Tree index for active kitchen orders per tenant
        builder.HasIndex(o => o.TenantId)
            .HasDatabaseName("ix_orders_active_tenant")
            .HasFilter("status NOT IN ('delivered', 'cancelled')");
    }
}
