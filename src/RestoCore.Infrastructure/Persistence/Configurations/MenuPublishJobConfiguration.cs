namespace RestoCore.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestoCore.Domain.Entities;

public class MenuPublishJobConfiguration : IEntityTypeConfiguration<MenuPublishJob>
{
    public void Configure(EntityTypeBuilder<MenuPublishJob> builder)
    {
        builder.ToTable("menu_publish_jobs");
        builder.HasKey(j => j.Id);

        builder.Property(j => j.TenantSlug).HasMaxLength(50).IsRequired();
        builder.Property(j => j.Status).HasMaxLength(20).IsRequired();
        builder.Property(j => j.VersionHash).HasMaxLength(64).IsRequired();
        builder.Property(j => j.ErrorMessage).HasMaxLength(500);

        builder.HasOne(j => j.Tenant)
            .WithMany(t => t.MenuPublishJobs)
            .HasForeignKey(j => j.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(j => new { j.TenantId, j.TriggeredAt });
    }
}