using ArlianTrans.Web.Data;
using ArlianTrans.Web.Models;
using ArlianTrans.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArlianTrans.Web.Controllers;

public class TripsController(AppDbContext context, IWebHostEnvironment environment) : Controller
{
    public async Task<IActionResult> Index(string? search, string? country, TransportType? transportType)
    {
        var query = context.Trips.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(x => x.Destination.Contains(search) || x.DepartureCity.Contains(search));
        if (!string.IsNullOrWhiteSpace(country)) query = query.Where(x => x.Country.Contains(country));
        if (transportType.HasValue) query = query.Where(x => x.TransportType == transportType.Value);

        var model = new TripListViewModel
        {
            Search = search,
            Country = country,
            TransportType = transportType,
            Trips = await query.OrderByDescending(x => x.Id).ToListAsync()
        };
        return View(model);
    }

    public async Task<IActionResult> Details(int id)
    {
        var trip = await context.Trips.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return trip is null ? NotFound() : View(trip);
    }

    public async Task<IActionResult> DbStatus(string? term)
    {
        term = string.IsNullOrWhiteSpace(term) ? "tokyooo" : term.Trim();
        var dbPath = Path.Combine(environment.ContentRootPath, "database", "arlian_trans.db");
        var latest = await context.Trips.AsNoTracking()
            .OrderByDescending(x => x.Id)
            .Take(12)
            .ToListAsync();
        var matches = await context.Trips.AsNoTracking()
            .Where(x => x.Destination.Contains(term) || x.Country.Contains(term) || x.DepartureCity.Contains(term))
            .OrderByDescending(x => x.Id)
            .ToListAsync();

        var lines = new List<string>
        {
            $"DB path: {dbPath}",
            $"DB exists: {System.IO.File.Exists(dbPath)}",
            $"Trips count: {await context.Trips.AsNoTracking().CountAsync()}",
            $"Search term: {term}",
            "",
            "Matches:"
        };
        lines.AddRange(matches.Select(x => $"{x.Id} | {x.DepartureCity} - {x.Destination} | {x.Country} | {x.DepartureDate:yyyy-MM-dd} | Status={x.Status}"));
        lines.Add("");
        lines.Add("Latest trips:");
        lines.AddRange(latest.Select(x => $"{x.Id} | {x.DepartureCity} - {x.Destination} | {x.Country} | {x.DepartureDate:yyyy-MM-dd} | Status={x.Status}"));

        return Content(string.Join(Environment.NewLine, lines), "text/plain");
    }
}
