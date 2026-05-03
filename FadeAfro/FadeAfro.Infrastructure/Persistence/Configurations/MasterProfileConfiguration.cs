using FadeAfro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FadeAfro.Infrastructure.Persistence.Configurations;

public class MasterProfileConfiguration : IEntityTypeConfiguration<MasterProfile>
{
    public void Configure(EntityTypeBuilder<MasterProfile> builder)
    {
        builder.ToTable("MasterProfiles");

        builder.HasKey(mp => mp.Id);

        builder.Property(mp => mp.PhotoUrl)
            .HasMaxLength(500);

        builder.Property(mp => mp.CreatedAt)
            .IsRequired();

        builder.HasOne(mp => mp.Master)
            .WithOne()
            .HasForeignKey<MasterProfile>(mp => mp.MasterId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
