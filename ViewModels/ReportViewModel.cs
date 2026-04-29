namespace ArlianTrans.Web.ViewModels;

public class ReportViewModel
{
    public int TotalReservations { get; set; }
    public int TotalTicketsSold { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal CardPayments { get; set; }
    public decimal CashPayments { get; set; }
    public List<PopularTripItem> PopularTrips { get; set; } = new();
    public List<PopularTripItem> PopularDestinations { get; set; } = new();
    public List<MonthlyRevenueItem> MonthlyRevenue { get; set; } = new();
    public List<MonthlyCountItem> MonthlyReservations { get; set; } = new();
    public List<PopularTripItem> PaymentMix { get; set; } = new();
}

public class PopularTripItem
{
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class MonthlyRevenueItem
{
    public string Month { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class MonthlyCountItem
{
    public string Month { get; set; } = string.Empty;
    public int Count { get; set; }
}
