namespace ArlianTrans.Web.Models;

public class Ticket
{
    public int Id { get; set; }
    public int ReservationId { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    public string PassengerName { get; set; } = string.Empty;
    public Reservation? Reservation { get; set; }
}
