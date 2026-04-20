using Microsoft.EntityFrameworkCore;
using SportClubApi.Models;

namespace SportClubApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Каждое DbSet<T> = одна таблица в базе данных
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Workout> Workouts => Set<Workout>();
    public DbSet<Membership> Memberships => Set<Membership>();
    public DbSet<WorkoutRegistration> WorkoutRegistrations => Set<WorkoutRegistration>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Email должен быть уникальным
        modelBuilder.Entity<Member>(e => {
            e.HasIndex(m => m.Email).IsUnique();
        });

        // Абонемент -> Член клуба (каскадное удаление)
        modelBuilder.Entity<Membership>(e => {
            e.HasOne(m => m.Member)
             .WithMany(mb => mb.Memberships)
             .HasForeignKey(m => m.MemberId)
             .OnDelete(DeleteBehavior.Cascade);
            e.Property(m => m.Price).HasPrecision(18, 2);
        });

        // Запись на тренировку -> Member и Workout
        modelBuilder.Entity<WorkoutRegistration>(e => {
            e.HasOne(wr => wr.Member)
             .WithMany(m => m.WorkoutRegistrations)
             .HasForeignKey(wr => wr.MemberId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(wr => wr.Workout)
             .WithMany(w => w.Registrations)
             .HasForeignKey(wr => wr.WorkoutId)
             .OnDelete(DeleteBehavior.Cascade);

            // Один член — один раз на тренировку
            e.HasIndex(wr => new { wr.MemberId, wr.WorkoutId }).IsUnique();
        });
    }
}
