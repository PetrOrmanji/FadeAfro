using FadeAfro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FadeAfro.Infrastructure.Persistence.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("Appointments");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.StartTime)
            .IsRequired();

        builder.Property(a => a.EndTime)
            .IsRequired();

        builder.Property(a => a.Comment)
            .HasMaxLength(500);

        builder.Property(a => a.CreatedAt)
            .IsRequired();

        builder.HasOne(a => a.Client)
            .WithMany(u => u.Appointments)
            .HasForeignKey(a => a.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.MasterProfile)
            .WithMany(mp => mp.Appointments)
            .HasForeignKey(a => a.MasterProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.Services)
            .WithOne(s => s.Appointment)
            .HasForeignKey(s => s.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(a => a.Services).HasField("_services");
    }
}
