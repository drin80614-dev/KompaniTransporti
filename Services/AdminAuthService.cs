using ArlianTrans.Web.Data;
using ArlianTrans.Web.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace ArlianTrans.Web.Services;

public class AdminAuthService(AppDbContext context)
{
    public const string SessionKey = "ArlianTrans.AdminUsername";
    public const string RoleSessionKey = "ArlianTrans.AdminRole";
    public const string NameSessionKey = "ArlianTrans.AdminName";

    public async Task<AdminUser?> ValidateAsync(string username, string password)
    {
        var hash = HashPassword(password);
        return await context.AdminUsers.FirstOrDefaultAsync(x => x.Username == username && x.PasswordHash == hash);
    }

    public static string HashPassword(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }
}
