using ArlianTrans.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace ArlianTrans.Web.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Trip> Trips => Set<Trip>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Seat> Seats => Set<Seat>();
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();
    public DbSet<EmailLog> EmailLogs => Set<EmailLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Trip>(entity =>
        {
            entity.Property(x => x.Price).HasColumnType("decimal(10,2)");
            entity.HasMany(x => x.Seats).WithOne(x => x.Trip).HasForeignKey(x => x.TripId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.Reservations).WithOne(x => x.Trip).HasForeignKey(x => x.TripId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasIndex(x => x.Email);
            entity.HasIndex(x => x.PhoneNumber);
        });

        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.Property(x => x.TotalAmount).HasColumnType("decimal(10,2)");
            entity.HasOne(x => x.Customer).WithMany(x => x.Reservations).HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(x => x.Tickets).WithOne(x => x.Reservation).HasForeignKey(x => x.ReservationId).OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(x => x.Payments).WithOne(x => x.Reservation).HasForeignKey(x => x.ReservationId).OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(x => x.EmailLogs).WithOne(x => x.Reservation).HasForeignKey(x => x.ReservationId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Ticket>(entity => entity.HasIndex(x => x.TicketNumber).IsUnique());
        modelBuilder.Entity<Payment>(entity => entity.Property(x => x.Amount).HasColumnType("decimal(10,2)"));
        modelBuilder.Entity<Seat>(entity => entity.HasIndex(x => new { x.TripId, x.SeatNumber }).IsUnique());
        modelBuilder.Entity<EmailLog>(entity =>
        {
            entity.HasIndex(x => x.CreatedAt);
            entity.HasOne(x => x.Ticket).WithMany().HasForeignKey(x => x.TicketId).OnDelete(DeleteBehavior.SetNull);
        });
        modelBuilder.Entity<AdminUser>(entity =>
        {
            entity.HasIndex(x => x.Username).IsUnique();
        });
    }
}
