namespace ArlianTrans.Web.Services;

public class BookingResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? ReservationId { get; set; }
    public string? TicketNumber { get; set; }
}
