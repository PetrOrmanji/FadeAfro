using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FadeAfro.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.TelegramId)
            .IsRequired();

        builder.HasIndex(u => u.TelegramId)
            .IsUnique();

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .HasMaxLength(100);

        builder.Property(u => u.Username)
            .HasMaxLength(100);

        var rolesComparer = new ValueComparer<List<Role>>(
            (a, b) => a != null && b != null && a.SequenceEqual(b),
            roles => roles.Aggregate(0, (acc, r) => HashCode.Combine(acc, r.GetHashCode())),
            roles => roles.ToList());

        builder.Property(u => u.Roles)
            .HasConversion(
                roles => string.Join(',', roles.Select(r => r.ToString())),
                value => value.Split(',', StringSplitOptions.RemoveEmptyEntries)
                              .Select(Enum.Parse<Role>)
                              .ToList())
            .Metadata.SetValueComparer(rolesComparer);

        builder.Property(u => u.CreatedAt)
            .IsRequired();
    }
}
