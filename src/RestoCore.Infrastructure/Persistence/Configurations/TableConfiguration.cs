namespace RestoCore.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestoCore.Domain.Entities;

public class TableConfiguration : IEntityTypeConfiguration<Table>
{
    public void Configure(EntityTypeBuilder<Table> builder)
    {
        builder.ToTable("tables");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.TableNumber).IsRequired();
        builder.Property(t => t.Token).HasMaxLength(64).IsRequired();
        builder.Property(t => t.Label).HasMaxLength(50);

        builder.HasIndex(t => new { t.TenantId, t.TableNumber }).IsUnique();
        builder.HasIndex(t => t.Token).IsUnique();

        builder.HasOne(t => t.Tenant).WithMany(tenant => tenant.Tables).HasForeignKey(t => t.TenantId).OnDelete(DeleteBehavior.Cascade);
    }
}
