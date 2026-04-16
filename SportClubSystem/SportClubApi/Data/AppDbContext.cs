using Microsoft.EntityFrameworkCore;
using SportClubApi.Models;

namespace SportClubApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Member> Members => Set<Member>();
    public DbSet<Workout> Workouts => Set<Workout>();
    public DbSet<Membership> Memberships => Set<Membership>();
    public DbSet<WorkoutRegistration> WorkoutRegistrations => Set<WorkoutRegistration>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Member>(entity =>
        {
            entity.HasIndex(m => m.Email).IsUnique();
        });

        modelBuilder.Entity<Membership>(entity =>
        {
            entity.HasOne(m => m.Member)
                  .WithMany(mb => mb.Memberships)
                  .HasForeignKey(m => m.MemberId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(m => m.Price).HasPrecision(18, 2);
        });

        modelBuilder.Entity<WorkoutRegistration>(entity =>
        {
            entity.HasOne(wr => wr.Member)
                  .WithMany(m => m.WorkoutRegistrations)
                  .HasForeignKey(wr => wr.MemberId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(wr => wr.Workout)
                  .WithMany(w => w.Registrations)
                  .HasForeignKey(wr => wr.WorkoutId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(wr => new { wr.MemberId, wr.WorkoutId }).IsUnique();
        });
    }
}