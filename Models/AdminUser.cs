using System.ComponentModel.DataAnnotations;

namespace ArlianTrans.Web.Models;

public class AdminUser
{
    public int Id { get; set; }
    [Required, StringLength(80)] public string FullName { get; set; } = string.Empty;
    [Required, StringLength(50)] public string Username { get; set; } = string.Empty;
    [Required] public string PasswordHash { get; set; } = string.Empty;
    public AdminRole Role { get; set; } = AdminRole.Manager;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
