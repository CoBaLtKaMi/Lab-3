using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Data;

public class AppDbContext : DbContext
{
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Training> Trainings => Set<Training>();

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Subscription>()
            .HasOne(s => s.Member)
            .WithMany(m => m.Subscriptions)
            .HasForeignKey(s => s.MemberId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Training>()
            .HasOne(t => t.Member)
            .WithMany(m => m.Trainings)
            .HasForeignKey(t => t.MemberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}