namespace ArlianTrans.Web.Models;

public class Payment
{
    public int Id { get; set; }
    public int ReservationId { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus Status { get; set; }
    public decimal Amount { get; set; }
    public string? CardHolderName { get; set; }
    public string? MaskedCardNumber { get; set; }
    public string ReferenceCode { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Reservation? Reservation { get; set; }
}
