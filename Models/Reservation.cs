namespace ArlianTrans.Web.Models;

public class Reservation
{
    public int Id { get; set; }
    public int TripId { get; set; }
    public int CustomerId { get; set; }
    public int SeatCount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public ReservationStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public bool EmailSent { get; set; }
    public DateTime? EmailSentAt { get; set; }
    public string? EmailErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
    public Trip? Trip { get; set; }
    public Customer? Customer { get; set; }
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<EmailLog> EmailLogs { get; set; } = new List<EmailLog>();
}
