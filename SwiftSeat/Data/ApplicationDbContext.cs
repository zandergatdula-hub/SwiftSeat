using Microsoft.EntityFrameworkCore;
using SwiftSeat.Models;
using System.Reflection;
using System.Reflection.Emit;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Shows> Shows { get; set; }

    public DbSet<Categories> Categories { get; set; }

    public DbSet<Purchases> Purchases { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Categories>().HasData(
            new Categories { Id = 1, Name = "Music" },
            new Categories { Id = 2, Name = "Sports" },
            new Categories { Id = 3, Name = "Theater" }
            );

        modelBuilder.Entity<Purchases>()
           .ToTable("Purchases")
           .HasKey(p => p.PurchaseId);

        // Configure the relationship
        modelBuilder.Entity<Purchases>()
            .HasOne(p => p.Event)
            .WithMany()
            .HasForeignKey(p => p.EventId);
    }

}

