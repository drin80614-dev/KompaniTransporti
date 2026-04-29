using System.Diagnostics;
using ArlianTrans.Web.Data;
using ArlianTrans.Web.Models;
using ArlianTrans.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArlianTrans.Web.Controllers;

public class HomeController(AppDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var model = new HomeViewModel
        {
            TotalTrips = await context.Trips.AsNoTracking().CountAsync(),
            ActiveReservations = await context.Reservations.AsNoTracking().CountAsync(x => x.Status != ReservationStatus.Cancelled),
            TicketsSold = await context.Tickets.AsNoTracking().CountAsync(),
            TotalRevenue = await context.Payments.AsNoTracking().Where(x => x.Status == PaymentStatus.Completed).SumAsync(x => (decimal?)x.Amount) ?? 0,
            FeaturedTrips = await context.Trips.AsNoTracking()
                .OrderByDescending(x => x.Id).Take(6).ToListAsync()
        };
        return View(model);
    }

    public IActionResult Contact() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
