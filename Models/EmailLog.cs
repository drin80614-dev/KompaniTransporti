namespace ArlianTrans.Web.Models;

public class EmailLog
{
    public int Id { get; set; }
    public string RecipientEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SentAt { get; set; }
    public int? ReservationId { get; set; }
    public int? TicketId { get; set; }
    public Reservation? Reservation { get; set; }
    public Ticket? Ticket { get; set; }
}
