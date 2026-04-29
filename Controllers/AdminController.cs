using ArlianTrans.Web.Data;
using ArlianTrans.Web.Models;
using ArlianTrans.Web.Services;
using ArlianTrans.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArlianTrans.Web.Controllers;

public class AdminController(AppDbContext context, AdminAuthService authService, BookingService bookingService, IEmailService emailService, DatabaseRefreshService refreshService) : Controller
{
    public IActionResult Index() => IsLoggedIn() ? RedirectToAction(nameof(Dashboard)) : RedirectToAction(nameof(Login));
    public IActionResult Login() => View(new AdminLoginViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(AdminLoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var admin = await authService.ValidateAsync(model.Username, model.Password);
        if (admin is null)
        {
            ModelState.AddModelError(string.Empty, "Kredencialet janë të pasakta.");
            return View(model);
        }

        HttpContext.Session.SetString(AdminAuthService.SessionKey, admin.Username);
        HttpContext.Session.SetString(AdminAuthService.NameSessionKey, admin.FullName);
        HttpContext.Session.SetString(AdminAuthService.RoleSessionKey, admin.Role.ToString());
        return RedirectToAction(nameof(Dashboard));
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Remove(AdminAuthService.SessionKey);
        HttpContext.Session.Remove(AdminAuthService.NameSessionKey);
        HttpContext.Session.Remove(AdminAuthService.RoleSessionKey);
        return RedirectToAction(nameof(Login));
    }

    public async Task<IActionResult> Dashboard()
    {
        var gate = EnsureAdmin();
        if (gate is not null) return gate;
        SetAdminViewData();

        var model = new AdminDashboardViewModel
        {
            TotalTrips = await context.Trips.AsNoTracking().CountAsync(),
            TotalReservations = await context.Reservations.AsNoTracking().CountAsync(),
            TotalTickets = await context.Tickets.AsNoTracking().CountAsync(),
            TotalRevenue = await context.Payments.AsNoTracking().Where(x => x.Status == PaymentStatus.Completed).SumAsync(x => (decimal?)x.Amount) ?? 0,
            LatestReservations = await context.Reservations.AsNoTracking().Include(x => x.Customer).Include(x => x.Trip).OrderByDescending(x => x.CreatedAt).Take(8).ToListAsync(),
            LowSeatTrips = await context.Trips.AsNoTracking().Where(x => x.AvailableSeats <= 10 && x.Status == TripStatus.Active).OrderBy(x => x.AvailableSeats).Take(8).ToListAsync()
        };

        return View(model);
    }

    public async Task<IActionResult> Trips()
    {
        var gate = EnsureManager();
        if (gate is not null) return gate;
        SetAdminViewData();
        return View(await context.Trips.AsNoTracking().OrderByDescending(x => x.Id).ToListAsync());
    }

    public IActionResult CreateTrip()
    {
        var gate = EnsureManager();
        if (gate is not null) return gate;
        SetAdminViewData();
        return View("TripForm", new Trip
        {
            DepartureCity = "Prishtinë",
            DepartureDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            ReturnDate = DateOnly.FromDateTime(DateTime.Today.AddDays(3)),
            DepartureTime = new TimeOnly(8, 0),
            ReturnTime = new TimeOnly(17, 0),
            TotalSeats = 60,
            AvailableSeats = 60,
            Status = TripStatus.Active
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTrip(Trip trip)
    {
        var gate = EnsureManager();
        if (gate is not null) return gate;
        ValidateTripSeatBalance(trip);
        if (!ModelState.IsValid) return View("TripForm", trip);

        context.Trips.Add(trip);
        await context.SaveChangesAsync();

        context.Seats.AddRange(Enumerable.Range(1, trip.TotalSeats).Select(number => new Seat
        {
            TripId = trip.Id,
            SeatNumber = number,
            Status = number <= trip.OccupiedSeats ? SeatStatus.Booked : SeatStatus.Free
        }));
        await context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Udhëtimi u shtua me sukses.";
        return RedirectToAction(nameof(Trips));
    }

    public async Task<IActionResult> EditTrip(int id)
    {
        var gate = EnsureManager();
        if (gate is not null) return gate;
        SetAdminViewData();
        var trip = await context.Trips.FindAsync(id);
        return trip is null ? NotFound() : View("TripForm", trip);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditTrip(Trip trip)
    {
        var gate = EnsureManager();
        if (gate is not null) return gate;
        ValidateTripSeatBalance(trip);
        if (!ModelState.IsValid) return View("TripForm", trip);

        context.Trips.Update(trip);
        await context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Udhëtimi u përditësua me sukses.";
        return RedirectToAction(nameof(Trips));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTrip(int id)
    {
        var gate = EnsureManager();
        if (gate is not null) return gate;

        var trip = await context.Trips.Include(x => x.Reservations).FirstOrDefaultAsync(x => x.Id == id);
        if (trip is null) return NotFound();

        if (trip.Reservations.Any(x => x.Status != ReservationStatus.Cancelled))
        {
            TempData["ErrorMessage"] = "Nuk lejohet fshirja e një udhëtimi me rezervime aktive.";
            return RedirectToAction(nameof(Trips));
        }

        context.Trips.Remove(trip);
        await context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Udhëtimi u fshi me sukses.";
        return RedirectToAction(nameof(Trips));
    }

    public async Task<IActionResult> Reservations()
    {
        var gate = EnsureAdmin();
        if (gate is not null) return gate;
        SetAdminViewData();

        var reservations = await context.Reservations
            .AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.Trip)
            .Include(x => x.Payments)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
        return View(reservations);
    }

    public async Task<IActionResult> CreateReservation()
    {
        var gate = EnsureAdmin();
        if (gate is not null) return gate;
        SetAdminViewData();

        return View(await BuildReservationViewModelAsync(new ReservationCreateViewModel
        {
            PaymentMethod = PaymentMethod.CashOffice
        }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateReservation(ReservationCreateViewModel model)
    {
        var gate = EnsureAdmin();
        if (gate is not null) return gate;

        if (!ModelState.IsValid)
        {
            return View(await BuildReservationViewModelAsync(model));
        }

        var result = await bookingService.CreateReservationAsync(model);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(await BuildReservationViewModelAsync(model));
        }

        TempData["SuccessMessage"] = "Rezervimi manual u ruajt në SQLite database file.";
        return RedirectToAction(nameof(Reservations));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmCashPayment(int id)
    {
        var gate = EnsureAdmin();
        if (gate is not null) return gate;
        TempData["SuccessMessage"] = await bookingService.ConfirmCashPaymentAsync(id) ? "Pagesa CASH u konfirmua." : "Konfirmimi dështoi.";
        return RedirectToAction(nameof(Reservations));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelReservation(int id)
    {
        var gate = EnsureAdmin();
        if (gate is not null) return gate;
        TempData["SuccessMessage"] = await bookingService.CancelReservationAsync(id) ? "Rezervimi u anulua." : "Anulimi dështoi.";
        return RedirectToAction(nameof(Reservations));
    }

    public async Task<IActionResult> Tickets()
    {
        var gate = EnsureAdmin();
        if (gate is not null) return gate;
        SetAdminViewData();
        var tickets = await context.Tickets.AsNoTracking().Include(x => x.Reservation)!.ThenInclude(x => x!.Trip).Include(x => x.Reservation)!.ThenInclude(x => x!.Customer).OrderByDescending(x => x.IssuedAt).ToListAsync();
        return View(tickets);
    }

    public async Task<IActionResult> Payments()
    {
        var gate = EnsureAdmin();
        if (gate is not null) return gate;
        SetAdminViewData();

        var payments = await context.Payments
            .AsNoTracking()
            .Include(x => x.Reservation)!.ThenInclude(x => x!.Customer)
            .Include(x => x.Reservation)!.ThenInclude(x => x!.Trip)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return View(payments);
    }

    public async Task<IActionResult> EmailLogs()
    {
        var gate = EnsureAdmin();
        if (gate is not null) return gate;
        SetAdminViewData();

        var logs = await context.EmailLogs.AsNoTracking()
            .Include(x => x.Reservation)!.ThenInclude(x => x!.Customer)
            .Include(x => x.Ticket)
            .OrderByDescending(x => x.CreatedAt)
            .Take(200)
            .ToListAsync();

        return View(logs);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendEmail(int id)
    {
        var gate = EnsureAdmin();
        if (gate is not null) return gate;

        var result = await emailService.ResendEmailAsync(id);
        if (result.Success)
        {
            TempData["SuccessMessage"] = "Email u ridergua me sukses.";
        }
        else
        {
            TempData["ErrorMessage"] = $"Email deshtoi: {result.ErrorMessage}";
        }

        return RedirectToAction(nameof(EmailLogs));
    }

    public IActionResult SendTestEmail()
    {
        var gate = EnsureAdmin();
        if (gate is not null) return gate;
        SetAdminViewData();
        return View(new TestEmailViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendTestEmail(TestEmailViewModel model)
    {
        var gate = EnsureAdmin();
        if (gate is not null) return gate;
        SetAdminViewData();

        if (!ModelState.IsValid) return View(model);

        var result = await emailService.SendTestEmailAsync(model.RecipientEmail);
        if (result.Success)
        {
            TempData["SuccessMessage"] = "SMTP OK - email testues u dergua.";
            return RedirectToAction(nameof(EmailLogs));
        }

        ModelState.AddModelError(string.Empty, $"SMTP gabim: {result.ErrorMessage}");
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RefreshData()
    {
        var gate = EnsureAdmin();
        if (gate is not null) return gate;

        await refreshService.SyncManualDatabaseChangesAsync();
        TempData["SuccessMessage"] = "Te dhenat u rilexuan nga SQLite.";
        var referer = Request.Headers.Referer.ToString();
        return string.IsNullOrWhiteSpace(referer) ? RedirectToAction(nameof(Dashboard)) : Redirect(referer);
    }

    public async Task<IActionResult> Reports()
    {
        var gate = EnsureManager();
        if (gate is not null) return gate;
        SetAdminViewData();

        var completedPayments = await context.Payments.AsNoTracking().Where(x => x.Status == PaymentStatus.Completed).ToListAsync();
        var reservations = await context.Reservations.AsNoTracking().ToListAsync();

        var model = new ReportViewModel
        {
            TotalReservations = reservations.Count,
            TotalTicketsSold = await context.Tickets.AsNoTracking().CountAsync(),
            TotalRevenue = completedPayments.Sum(x => x.Amount),
            CardPayments = completedPayments.Where(x => x.PaymentMethod == PaymentMethod.Card).Sum(x => x.Amount),
            CashPayments = completedPayments.Where(x => x.PaymentMethod == PaymentMethod.CashOffice).Sum(x => x.Amount),
            PopularTrips = await context.Reservations.AsNoTracking().Include(x => x.Trip).Where(x => x.Trip != null).GroupBy(x => x.Trip!.Destination).Select(x => new PopularTripItem { Label = x.Key, Count = x.Count() }).OrderByDescending(x => x.Count).Take(5).ToListAsync(),
            PopularDestinations = await context.Tickets.AsNoTracking().Include(x => x.Reservation)!.ThenInclude(x => x!.Trip).Where(x => x.Reservation != null && x.Reservation.Trip != null).GroupBy(x => x.Reservation!.Trip!.Country).Select(x => new PopularTripItem { Label = x.Key, Count = x.Count() }).OrderByDescending(x => x.Count).Take(5).ToListAsync(),
            MonthlyRevenue = completedPayments.GroupBy(x => x.CreatedAt.ToString("yyyy-MM")).Select(x => new MonthlyRevenueItem { Month = x.Key, Amount = x.Sum(y => y.Amount) }).OrderBy(x => x.Month).TakeLast(12).ToList(),
            MonthlyReservations = reservations.GroupBy(x => x.CreatedAt.ToString("yyyy-MM")).Select(x => new MonthlyCountItem { Month = x.Key, Count = x.Count() }).OrderBy(x => x.Month).TakeLast(12).ToList(),
            PaymentMix = completedPayments.GroupBy(x => x.PaymentMethod.ToString()).Select(x => new PopularTripItem { Label = x.Key, Count = x.Count() }).ToList()
        };

        return View(model);
    }

    public async Task<IActionResult> Users()
    {
        var gate = EnsureManager();
        if (gate is not null) return gate;
        SetAdminViewData();
        return View(await context.AdminUsers.AsNoTracking().OrderBy(x => x.Role).ThenBy(x => x.Username).ToListAsync());
    }

    public IActionResult CreateUser()
    {
        var gate = EnsureManager();
        if (gate is not null) return gate;
        SetAdminViewData();
        return View(new AdminUserCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(AdminUserCreateViewModel model)
    {
        var gate = EnsureManager();
        if (gate is not null) return gate;
        if (await context.AdminUsers.AnyAsync(x => x.Username == model.Username))
        {
            ModelState.AddModelError(nameof(model.Username), "Ky username ekziston.");
        }
        if (!ModelState.IsValid) return View(model);

        context.AdminUsers.Add(new AdminUser
        {
            FullName = model.FullName,
            Username = model.Username,
            PasswordHash = AdminAuthService.HashPassword(model.Password),
            Role = model.Role
        });
        await context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Përdoruesi i adminit u krijua.";
        return RedirectToAction(nameof(Users));
    }

    private IActionResult? EnsureAdmin() => IsLoggedIn() ? null : RedirectToAction(nameof(Login));

    private IActionResult? EnsureManager()
    {
        var gate = EnsureAdmin();
        if (gate is not null) return gate;
        if (!IsManager())
        {
            TempData["ErrorMessage"] = "Ky veprim lejohet vetëm për Manager.";
            return RedirectToAction(nameof(Dashboard));
        }
        return null;
    }

    private bool IsLoggedIn() => !string.IsNullOrWhiteSpace(HttpContext.Session.GetString(AdminAuthService.SessionKey));
    private bool IsManager() => HttpContext.Session.GetString(AdminAuthService.RoleSessionKey) == AdminRole.Manager.ToString();

    private void SetAdminViewData()
    {
        ViewData["AdminName"] = HttpContext.Session.GetString(AdminAuthService.NameSessionKey);
        ViewData["AdminRole"] = HttpContext.Session.GetString(AdminAuthService.RoleSessionKey);
        ViewData["IsManager"] = IsManager();
    }

    private async Task<ReservationCreateViewModel> BuildReservationViewModelAsync(ReservationCreateViewModel model)
    {
        model.Trips = await context.Trips
            .AsNoTracking()
            .Where(x => x.Status == TripStatus.Active && x.AvailableSeats > 0)
            .OrderBy(x => x.DepartureDate)
            .ThenBy(x => x.DepartureTime)
            .ToListAsync();

        var tripIds = model.Trips.Select(x => x.Id).ToList();
        var seats = await context.Seats
            .AsNoTracking()
            .Where(x => tripIds.Contains(x.TripId))
            .OrderBy(x => x.TripId)
            .ThenBy(x => x.SeatNumber)
            .ToListAsync();
        model.SeatsByTrip = seats.GroupBy(x => x.TripId).ToDictionary(x => x.Key, x => x.ToList());
        return model;
    }

    private void ValidateTripSeatBalance(Trip trip)
    {
        if (trip.TotalSeats < trip.OccupiedSeats || trip.AvailableSeats < 0 || trip.TotalSeats != trip.AvailableSeats + trip.OccupiedSeats)
        {
            ModelState.AddModelError(string.Empty, "Bilanci i ulëseve nuk është valid.");
        }
    }
}
