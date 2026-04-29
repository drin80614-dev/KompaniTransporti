using ArlianTrans.Web.Data;
using ArlianTrans.Web.Models;
using ArlianTrans.Web.Services;
using ArlianTrans.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArlianTrans.Web.Controllers;

public class TicketsController(AppDbContext context, BookingService bookingService) : Controller
{
    public async Task<IActionResult> Create() => View(await BuildViewModelAsync(new TicketPurchaseViewModel()));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TicketPurchaseViewModel model)
    {
        if (!ModelState.IsValid) return View(await BuildViewModelAsync(model));

        var result = await bookingService.PurchaseTicketAsync(model);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(await BuildViewModelAsync(model));
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction(nameof(Success), new { id = result.ReservationId });
    }

    public async Task<IActionResult> Success(int id)
    {
        var reservation = await context.Reservations
            .AsNoTracking()
            .Include(x => x.Customer).Include(x => x.Trip).Include(x => x.Tickets).Include(x => x.Payments)
            .FirstOrDefaultAsync(x => x.Id == id);
        return reservation is null ? NotFound() : View(reservation);
    }

    private async Task<TicketPurchaseViewModel> BuildViewModelAsync(TicketPurchaseViewModel model)
    {
        model.Trips = await context.Trips
            .AsNoTracking()
            .Where(x => x.Status == TripStatus.Active && x.AvailableSeats > 0)
            .OrderBy(x => x.DepartureDate)
            .ThenBy(x => x.DepartureTime)
            .ToListAsync();
        model.SeatsByTrip = await LoadSeatsByTripAsync(model.Trips.Select(x => x.Id).ToList());
        return model;
    }

    private async Task<Dictionary<int, List<Seat>>> LoadSeatsByTripAsync(List<int> tripIds)
    {
        var seats = await context.Seats
            .AsNoTracking()
            .Where(x => tripIds.Contains(x.TripId))
            .OrderBy(x => x.TripId)
            .ThenBy(x => x.SeatNumber)
            .ToListAsync();
        return seats.GroupBy(x => x.TripId).ToDictionary(x => x.Key, x => x.ToList());
    }
}
