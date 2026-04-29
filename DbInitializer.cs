using ArlianTrans.Web.Models;
using ArlianTrans.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace ArlianTrans.Web.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var dbDirectory = Path.Combine(Directory.GetCurrentDirectory(), "database");
        Directory.CreateDirectory(dbDirectory);

        await context.Database.EnsureCreatedAsync();
        await EnsureSchemaUpdatesAsync(context);

        if (!await context.AdminUsers.AnyAsync())
        {
            context.AdminUsers.Add(new AdminUser
            {
                FullName = "Demo Admin",
                Username = "admin",
                PasswordHash = AdminAuthService.HashPassword("Admin123!"),
                Role = AdminRole.Manager,
                CreatedAt = DateTime.UtcNow
            });

            context.AdminUsers.Add(new AdminUser
            {
                FullName = "Punetor Zyre",
                Username = "zyra",
                PasswordHash = AdminAuthService.HashPassword("Zyra123!"),
                Role = AdminRole.OfficeStaff,
                CreatedAt = DateTime.UtcNow
            });
        }

        if (!await context.Trips.AnyAsync())
        {
            var trips = SeedDataFactory.CreateTrips();
            context.Trips.AddRange(trips);
            await context.SaveChangesAsync();

            var seats = new List<Seat>();
            foreach (var trip in trips)
            {
                for (var i = 1; i <= trip.TotalSeats; i++)
                {
                    seats.Add(new Seat
                    {
                        TripId = trip.Id,
                        SeatNumber = i,
                        Status = SeatStatus.Free
                    });
                }
            }

            context.Seats.AddRange(seats);
        }

        await context.SaveChangesAsync();
    }

    private static async Task EnsureSchemaUpdatesAsync(AppDbContext context)
    {
        await EnsureColumnAsync(context, "AdminUsers", nameof(AdminUser.Role), "INTEGER NOT NULL DEFAULT 1");
        await EnsureColumnAsync(context, "Reservations", nameof(Reservation.EmailSent), "INTEGER NOT NULL DEFAULT 0");
        await EnsureColumnAsync(context, "Reservations", nameof(Reservation.EmailSentAt), "TEXT NULL");
        await EnsureColumnAsync(context, "Reservations", nameof(Reservation.EmailErrorMessage), "TEXT NULL");

        await context.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS EmailLogs (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                RecipientEmail TEXT NOT NULL,
                Subject TEXT NOT NULL,
                Body TEXT NOT NULL,
                Status TEXT NOT NULL,
                ErrorMessage TEXT NULL,
                CreatedAt TEXT NOT NULL,
                SentAt TEXT NULL,
                ReservationId INTEGER NULL,
                TicketId INTEGER NULL,
                FOREIGN KEY (ReservationId) REFERENCES Reservations(Id) ON DELETE SET NULL,
                FOREIGN KEY (TicketId) REFERENCES Tickets(Id) ON DELETE SET NULL
            )
            """);
    }

    private static async Task EnsureColumnAsync(AppDbContext context, string tableName, string columnName, string definition)
    {
        var columnsSql = "SELECT name AS Value FROM pragma_table_info('" + tableName + "')";
        var columns = await context.Database.SqlQueryRaw<string>(columnsSql).ToListAsync();
        if (!columns.Contains(columnName))
        {
            var alterSql = "ALTER TABLE " + tableName + " ADD COLUMN " + columnName + " " + definition;
            await context.Database.ExecuteSqlRawAsync(alterSql);
        }
    }
}
