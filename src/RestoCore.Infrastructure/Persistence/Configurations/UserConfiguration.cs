namespace RestoCore.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestoCore.Domain.Entities;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email).HasMaxLength(150).IsRequired();
        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.Name).HasMaxLength(100).IsRequired();
        builder.Property(u => u.PasswordHash).HasMaxLength(256).IsRequired();
        builder.Property(u => u.PrimaryRole).HasMaxLength(50).IsRequired();
        builder.Property(u => u.Roles).IsRequired();
        builder.Property(u => u.IsActive).HasDefaultValue(true).IsRequired();

        builder.HasOne(u => u.Tenant)
            .WithMany()
            .HasForeignKey(u => u.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
