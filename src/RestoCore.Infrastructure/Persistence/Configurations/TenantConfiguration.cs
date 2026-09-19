namespace RestoCore.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestoCore.Domain.Entities;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("tenants");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name).HasMaxLength(100).IsRequired();
        builder.Property(t => t.Slug).HasMaxLength(50).IsRequired();
        builder.HasIndex(t => t.Slug).IsUnique();

        builder.Property(t => t.CustomDomain).HasMaxLength(150);
        builder.HasIndex(t => t.CustomDomain).IsUnique();

        builder.Property(t => t.Status).HasMaxLength(20).IsRequired();

        builder.OwnsOne(t => t.BrandingConfig, branding =>
        {
            branding.ToJson();
        });

        builder.OwnsOne(t => t.LayoutConfig, layout =>
        {
            layout.ToJson();
            layout.OwnsMany(l => l.Elements);
        });

        builder.Property(t => t.CurrentVersionHash).HasMaxLength(64);
    }
}
