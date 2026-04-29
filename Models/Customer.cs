using System.ComponentModel.DataAnnotations;

namespace ArlianTrans.Web.Models;

public class Customer
{
    public int Id { get; set; }
    [Required, StringLength(80)] public string FirstName { get; set; } = string.Empty;
    [Required, StringLength(80)] public string LastName { get; set; } = string.Empty;
    [Required, StringLength(30)] public string PhoneNumber { get; set; } = string.Empty;
    [Required, EmailAddress, StringLength(120)] public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
