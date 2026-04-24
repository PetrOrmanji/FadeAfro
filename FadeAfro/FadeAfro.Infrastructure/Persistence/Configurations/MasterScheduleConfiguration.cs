using FadeAfro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FadeAfro.Infrastructure.Persistence.Configurations;

public class MasterScheduleConfiguration : IEntityTypeConfiguration<MasterSchedule>
{
    public void Configure(EntityTypeBuilder<MasterSchedule> builder)
    {
        builder.ToTable("MasterSchedules");

        builder.HasKey(ms => ms.Id);

        builder.Property(ms => ms.DayOfWeek)
            .IsRequired();

        builder.Property(ms => ms.StartTime)
            .IsRequired();

        builder.Property(ms => ms.EndTime)
            .IsRequired();

        builder.Property(ms => ms.CreatedAt)
            .IsRequired();

        builder.HasOne(ms => ms.MasterProfile)
            .WithMany(mp => mp.Schedules)
            .HasForeignKey(ms => ms.MasterProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
