using ArlianTrans.Web.Models;

namespace ArlianTrans.Web.ViewModels;

public class HomeViewModel
{
    public int TotalTrips { get; set; }
    public int ActiveReservations { get; set; }
    public int TicketsSold { get; set; }
    public decimal TotalRevenue { get; set; }
    public List<Trip> FeaturedTrips { get; set; } = new();
}
