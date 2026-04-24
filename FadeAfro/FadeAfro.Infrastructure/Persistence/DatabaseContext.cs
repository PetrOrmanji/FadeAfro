using FadeAfro.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FadeAfro.Infrastructure.Persistence;

public class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<MasterProfile> MasterProfiles => Set<MasterProfile>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<MasterSchedule> MasterSchedules => Set<MasterSchedule>();
    public DbSet<MasterUnavailability> MasterUnavailabilities => Set<MasterUnavailability>();
    public DbSet<Appointment> Appointments => Set<Appointment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DatabaseContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
