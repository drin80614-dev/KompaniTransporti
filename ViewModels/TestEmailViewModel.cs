using System.ComponentModel.DataAnnotations;

namespace ArlianTrans.Web.ViewModels;

public class TestEmailViewModel
{
    [Required, EmailAddress, Display(Name = "Email testues")]
    public string RecipientEmail { get; set; } = string.Empty;
}
