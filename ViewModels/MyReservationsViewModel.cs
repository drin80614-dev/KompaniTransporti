using ArlianTrans.Web.Models;
using System.ComponentModel.DataAnnotations;

namespace ArlianTrans.Web.ViewModels;

public class MyReservationsViewModel
{
    [Required, Display(Name = "Email ose telefon")]
    public string Query { get; set; } = string.Empty;

    public bool HasSearched { get; set; }
    public List<Reservation> Reservations { get; set; } = new();
}
