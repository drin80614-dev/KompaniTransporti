using System.ComponentModel.DataAnnotations;

namespace ArlianTrans.Web.Models;

public class Trip
{
    public int Id { get; set; }
    [Required, StringLength(120)] public string DepartureCity { get; set; } = "Prishtinë";
    [Required, StringLength(120)] public string Destination { get; set; } = string.Empty;
    [Required, StringLength(120)] public string Country { get; set; } = string.Empty;
    public DateOnly DepartureDate { get; set; }
    public TimeOnly DepartureTime { get; set; }
    public DateOnly ReturnDate { get; set; }
    public TimeOnly ReturnTime { get; set; }
    [Range(1, 100000)] public decimal Price { get; set; }
    public TransportType TransportType { get; set; }
    [Range(1, 500)] public int TotalSeats { get; set; }
    [Range(0, 500)] public int AvailableSeats { get; set; }
    [Range(0, 500)] public int OccupiedSeats { get; set; }
    public TripStatus Status { get; set; }
    [StringLength(500)] public string Description { get; set; } = string.Empty;
    public ICollection<Seat> Seats { get; set; } = new List<Seat>();
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
