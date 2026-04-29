using ArlianTrans.Web.Models;
using System.ComponentModel.DataAnnotations;

namespace ArlianTrans.Web.ViewModels;

public class ReservationCreateViewModel : IValidatableObject
{
    [Required, Display(Name = "Emri")] public string FirstName { get; set; } = string.Empty;
    [Required, Display(Name = "Mbiemri")] public string LastName { get; set; } = string.Empty;
    [Required, Display(Name = "Telefoni"), RegularExpression(@"^\+?[0-9\s\-]{8,20}$", ErrorMessage = "Numër telefoni jo valid.")] public string PhoneNumber { get; set; } = string.Empty;
    [Required, EmailAddress, Display(Name = "Email")] public string Email { get; set; } = string.Empty;
    [Required, Display(Name = "Udhëtimi")] public int TripId { get; set; }
    [Range(1, 20), Display(Name = "Numri i ulëseve")] public int SeatCount { get; set; } = 1;
    [Display(Name = "Ulëset e zgjedhura")] public string? SelectedSeatNumbers { get; set; }
    [Required, Display(Name = "Mënyra e pagesës")] public PaymentMethod PaymentMethod { get; set; }
    public List<Trip> Trips { get; set; } = new();
    public Dictionary<int, List<Seat>> SeatsByTrip { get; set; } = new();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (SeatCount <= 0)
        {
            yield return new ValidationResult("Numri i ulëseve duhet të jetë pozitiv.", new[] { nameof(SeatCount) });
        }

        var selectedSeats = ParseSelectedSeats(SelectedSeatNumbers);
        if (selectedSeats.Count > 0 && selectedSeats.Count != SeatCount)
        {
            yield return new ValidationResult("Numri i ulëseve të zgjedhura duhet të përputhet me numrin e ulëseve.", new[] { nameof(SelectedSeatNumbers) });
        }
    }

    public static List<int> ParseSelectedSeats(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? new List<int>()
            : value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(x => int.TryParse(x, out var number) ? number : 0)
                .Where(x => x > 0)
                .Distinct()
                .OrderBy(x => x)
                .ToList();
}
