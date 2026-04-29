using ArlianTrans.Web.Data;
using ArlianTrans.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace ArlianTrans.Web.Services;

public class DatabaseRefreshService(AppDbContext context)
{
    public async Task SyncManualDatabaseChangesAsync()
    {
        var trips = await context.Trips.ToListAsync();
        var seatsByTrip = (await context.Seats
                .Select(x => new { x.TripId, x.SeatNumber })
                .ToListAsync())
            .GroupBy(x => x.TripId)
            .ToDictionary(x => x.Key, x => x.Select(y => y.SeatNumber).ToHashSet());

        foreach (var trip in trips)
        {
            NormalizeTrip(trip);
            seatsByTrip.TryGetValue(trip.Id, out var existingSet);
            existingSet ??= new HashSet<int>();

            if (existingSet.Count >= trip.TotalSeats)
            {
                continue;
            }

            var missingSeats = Enumerable.Range(1, trip.TotalSeats)
                .Where(number => !existingSet.Contains(number))
                .Select(number => new Seat
                {
                    TripId = trip.Id,
                    SeatNumber = number,
                    Status = number <= trip.OccupiedSeats ? SeatStatus.Booked : SeatStatus.Free
                })
                .ToList();

            if (missingSeats.Count > 0)
            {
                context.Seats.AddRange(missingSeats);
            }
        }

        await context.SaveChangesAsync();
    }

    private static void NormalizeTrip(Trip trip)
    {
        if (string.IsNullOrWhiteSpace(trip.DepartureCity))
        {
            trip.DepartureCity = "Prishtinë";
        }

        if (trip.TransportType == 0)
        {
            trip.TransportType = TransportType.Autobus;
        }

        if (trip.Status == 0)
        {
            trip.Status = TripStatus.Active;
        }

        if (trip.TotalSeats <= 0)
        {
            trip.TotalSeats = 60;
        }

        if (trip.OccupiedSeats < 0)
        {
            trip.OccupiedSeats = 0;
        }

        if (trip.AvailableSeats <= 0 && trip.OccupiedSeats == 0)
        {
            trip.AvailableSeats = trip.TotalSeats;
        }

        if (trip.AvailableSeats < 0 || trip.AvailableSeats + trip.OccupiedSeats != trip.TotalSeats)
        {
            trip.AvailableSeats = Math.Max(0, trip.TotalSeats - trip.OccupiedSeats);
        }
    }
}
