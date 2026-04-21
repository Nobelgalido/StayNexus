using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StayNexus.Core.Models;

namespace StayNexus.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Property> Properties { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Payment> Payments { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Property>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Address).IsRequired().HasMaxLength(500);
            entity.Property(p => p.City).IsRequired().HasMaxLength(100);
            entity.Property(p => p.Province).IsRequired().HasMaxLength(100);

            entity.HasOne(p => p.Owner)
                  .WithMany(u => u.Properties)
                  .HasForeignKey(p => p.OwnerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Room>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Name).IsRequired().HasMaxLength(200);
            entity.Property(r => r.PricePerNight).HasColumnType("decimal(18,2)");

            entity.HasOne(r => r.Property)
                  .WithMany(p => p.Rooms)
                  .HasForeignKey(r => r.PropertyId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Booking>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.Property(b => b.TotalPrice).HasColumnType("decimal(18,2)");

            entity.HasOne(b => b.Guest)
                  .WithMany(u => u.Bookings)
                  .HasForeignKey(b => b.GuestId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(b => b.Room)
                  .WithMany(r => r.Bookings)
                  .HasForeignKey(b => b.RoomId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Payment>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Amount).HasColumnType("decimal(18,2)");
            entity.Property(p => p.GatewayReferenceId).HasMaxLength(500);

            entity.HasOne(p => p.Booking)
                  .WithOne(b => b.Payment)
                  .HasForeignKey<Payment>(p => p.BookingId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
