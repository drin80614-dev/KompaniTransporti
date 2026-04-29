using ArlianTrans.Web.Models;

namespace ArlianTrans.Web.ViewModels;

public class AdminDashboardViewModel
{
    public int TotalTrips { get; set; }
    public int TotalReservations { get; set; }
    public int TotalTickets { get; set; }
    public decimal TotalRevenue { get; set; }
    public List<Reservation> LatestReservations { get; set; } = new();
    public List<Trip> LowSeatTrips { get; set; } = new();
}
