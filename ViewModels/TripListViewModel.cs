using ArlianTrans.Web.Models;

namespace ArlianTrans.Web.ViewModels;

public class TripListViewModel
{
    public string? Search { get; set; }
    public string? Country { get; set; }
    public TransportType? TransportType { get; set; }
    public List<Trip> Trips { get; set; } = new();
}
