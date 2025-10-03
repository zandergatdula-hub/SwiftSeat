using Microsoft.EntityFrameworkCore;
using SwiftSeat.Models;
using System.Reflection;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

  
    public DbSet<Customer> Customers { get; set; }
   
    public DbSet<Shows> Shows { get; set; }
}
