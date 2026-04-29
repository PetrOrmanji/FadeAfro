using FadeAfro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FadeAfro.Infrastructure.Persistence.Configurations;

public class MasterUnavailabilityConfiguration : IEntityTypeConfiguration<MasterUnavailability>
{
    public void Configure(EntityTypeBuilder<MasterUnavailability> builder)
    {
        builder.ToTable("MasterUnavailabilities");

        builder.HasKey(mu => mu.Id);

        builder.Property(mu => mu.Date)
            .IsRequired();

        builder.Property(mu => mu.CreatedAt)
            .IsRequired();
        
        builder.HasIndex(ms => new { ms.MasterProfileId, ms.Date })
            .IsUnique();

        builder.HasOne(mu => mu.MasterProfile)
            .WithMany(mp => mp.Unavailabilities)
            .HasForeignKey(mu => mu.MasterProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
