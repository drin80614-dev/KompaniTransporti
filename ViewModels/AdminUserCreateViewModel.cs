using ArlianTrans.Web.Models;
using System.ComponentModel.DataAnnotations;

namespace ArlianTrans.Web.ViewModels;

public class AdminUserCreateViewModel
{
    [Required, Display(Name = "Emri i plotë")] public string FullName { get; set; } = string.Empty;
    [Required, Display(Name = "Username")] public string Username { get; set; } = string.Empty;
    [Required, MinLength(6), Display(Name = "Password")] public string Password { get; set; } = string.Empty;
    [Required, Display(Name = "Roli")] public AdminRole Role { get; set; } = AdminRole.OfficeStaff;
}
