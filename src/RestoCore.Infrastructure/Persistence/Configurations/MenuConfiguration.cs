namespace RestoCore.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestoCore.Domain.Entities;

public class MenuConfiguration : IEntityTypeConfiguration<Category>, IEntityTypeConfiguration<MenuItem>, IEntityTypeConfiguration<ModifierGroup>, IEntityTypeConfiguration<ModifierOption>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).HasMaxLength(80).IsRequired();
        builder.Property(c => c.Description).HasMaxLength(250);
        builder.Property(c => c.DisplayOrder).HasDefaultValue(0);

        builder.HasIndex(c => new { c.TenantId, c.DisplayOrder });
        builder.HasOne(c => c.Tenant).WithMany(t => t.Categories).HasForeignKey(c => c.TenantId).OnDelete(DeleteBehavior.Cascade);
    }

    public void Configure(EntityTypeBuilder<MenuItem> builder)
    {
        builder.ToTable("menu_items");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Name).HasMaxLength(100).IsRequired();
        builder.Property(m => m.BasePrice).HasPrecision(10, 2).IsRequired();
        builder.Property(m => m.ImageUrl).HasMaxLength(300);

        builder.HasIndex(m => new { m.TenantId, m.CategoryId, m.DisplayOrder });
        builder.HasIndex(m => new { m.TenantId, m.IsAvailable });

        builder.HasOne(m => m.Tenant).WithMany(t => t.MenuItems).HasForeignKey(m => m.TenantId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.Category).WithMany(c => c.Items).HasForeignKey(m => m.CategoryId).OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<ModifierGroup> builder)
    {
        builder.ToTable("modifier_groups");
        builder.HasKey(mg => mg.Id);

        builder.Property(mg => mg.Name).HasMaxLength(80).IsRequired();
        builder.HasOne(mg => mg.MenuItem).WithMany(m => m.ModifierGroups).HasForeignKey(mg => mg.MenuItemId).OnDelete(DeleteBehavior.Cascade);
    }

    public void Configure(EntityTypeBuilder<ModifierOption> builder)
    {
        builder.ToTable("modifier_options");
        builder.HasKey(mo => mo.Id);

        builder.Property(mo => mo.Name).HasMaxLength(80).IsRequired();
        builder.Property(mo => mo.PriceDelta).HasPrecision(10, 2).HasDefaultValue(0.00m);
        builder.HasOne(mo => mo.ModifierGroup).WithMany(mg => mg.Options).HasForeignKey(mo => mo.ModifierGroupId).OnDelete(DeleteBehavior.Cascade);
    }
}
