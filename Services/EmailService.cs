using ArlianTrans.Web.Data;
using ArlianTrans.Web.Models;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace ArlianTrans.Web.Services;

public interface IEmailService
{
    Task<EmailSendResult> SendReservationEmailAsync(int reservationId);
    Task<EmailSendResult> ResendEmailAsync(int emailLogId);
    Task<EmailSendResult> SendTestEmailAsync(string recipientEmail);
}

public class EmailService(AppDbContext context, IConfiguration configuration, ILogger<EmailService> logger) : IEmailService
{
    public async Task<EmailSendResult> SendReservationEmailAsync(int reservationId)
    {
        var reservation = await LoadReservationAsync(reservationId);
        if (reservation is null)
        {
            return new EmailSendResult(false, $"Reservation #{reservationId} nuk u gjet.");
        }

        if (reservation.Customer is null || reservation.Trip is null)
        {
            return await SaveReservationEmailStatusAsync(reservationId, false, "Rezervimi nuk ka klient ose udhetim te lidhur.");
        }

        var subject = BuildTicketSubject(reservation);
        var body = await BuildReservationEmailBodyAsync(reservation);
        var ticketId = reservation.Tickets.OrderByDescending(x => x.Id).FirstOrDefault()?.Id;
        var result = await SendAndLogAsync(reservation.Customer.Email, subject, body, reservation.Id, ticketId);
        return await SaveReservationEmailStatusAsync(reservation.Id, result.Success, result.ErrorMessage);
    }

    public async Task<EmailSendResult> ResendEmailAsync(int emailLogId)
    {
        var original = await context.EmailLogs.AsNoTracking().FirstOrDefaultAsync(x => x.Id == emailLogId);
        if (original is null)
        {
            return new EmailSendResult(false, $"Email log #{emailLogId} nuk u gjet.");
        }

        var result = await SendAndLogAsync(original.RecipientEmail, original.Subject, original.Body, original.ReservationId, original.TicketId);
        if (original.ReservationId.HasValue)
        {
            await SaveReservationEmailStatusAsync(original.ReservationId.Value, result.Success, result.ErrorMessage);
        }

        return result;
    }

    public async Task<EmailSendResult> SendTestEmailAsync(string recipientEmail)
    {
        var subject = "SMTP Test - Arlian Trans";
        var body = """
        <div style="font-family:Arial,sans-serif;max-width:640px;margin:auto;border:1px solid #d7dde4;border-radius:12px;padding:20px">
            <h2 style="margin-top:0;color:#0a6c74">SMTP OK - Arlian Trans</h2>
            <p>Ky eshte email testues nga paneli i adminit.</p>
        </div>
        """;
        return await SendAndLogAsync(recipientEmail, subject, body, null, null);
    }

    private async Task<Reservation?> LoadReservationAsync(int reservationId) =>
        await context.Reservations.AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.Trip)
            .Include(x => x.Tickets)
            .Include(x => x.Payments)
            .FirstOrDefaultAsync(x => x.Id == reservationId);

    private async Task<EmailSendResult> SendAndLogAsync(string recipientEmail, string subject, string body, int? reservationId, int? ticketId)
    {
        var log = new EmailLog
        {
            RecipientEmail = recipientEmail,
            Subject = subject,
            Body = body,
            Status = "Pending",
            ReservationId = reservationId,
            TicketId = ticketId,
            CreatedAt = DateTime.UtcNow
        };

        context.EmailLogs.Add(log);
        await context.SaveChangesAsync();

        try
        {
            if (!IsValidEmail(recipientEmail))
            {
                throw new InvalidOperationException($"Email adresa nuk eshte valide: {recipientEmail}");
            }

            var settings = ReadSettings();
            settings.Validate();

            using var message = new MailMessage
            {
                From = new MailAddress(settings.SenderEmail, settings.SenderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8
            };
            message.To.Add(recipientEmail);

            using var client = new SmtpClient(settings.SmtpHost, settings.SmtpPort)
            {
                EnableSsl = settings.EnableSsl,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(settings.Username, settings.Password)
            };

            await client.SendMailAsync(message);
            log.Status = "Sent";
            log.SentAt = DateTime.UtcNow;
            log.ErrorMessage = null;
            await context.SaveChangesAsync();
            return new EmailSendResult(true);
        }
        catch (Exception ex)
        {
            var error = GetFullError(ex);
            logger.LogWarning(ex, "Email sending failed for {RecipientEmail}", recipientEmail);
            log.Status = "Failed";
            log.ErrorMessage = error;
            await context.SaveChangesAsync();
            return new EmailSendResult(false, error);
        }
    }

    private async Task<EmailSendResult> SaveReservationEmailStatusAsync(int reservationId, bool success, string? errorMessage)
    {
        var reservation = await context.Reservations.FirstOrDefaultAsync(x => x.Id == reservationId);
        if (reservation is null)
        {
            return new EmailSendResult(false, $"Reservation #{reservationId} nuk u gjet per perditesim email status.");
        }

        reservation.EmailSent = success;
        reservation.EmailSentAt = success ? DateTime.UtcNow : null;
        reservation.EmailErrorMessage = success ? null : errorMessage;
        await context.SaveChangesAsync();
        return new EmailSendResult(success, errorMessage);
    }

    private async Task<string> BuildReservationEmailBodyAsync(Reservation reservation)
    {
        var trip = reservation.Trip!;
        var customer = reservation.Customer!;
        var tickets = reservation.Tickets.OrderBy(x => x.Id).Select(x => x.TicketNumber).ToList();
        var payments = reservation.Payments.OrderByDescending(x => x.Id).ToList();
        var payment = payments.FirstOrDefault();
        var seatNumbers = await context.Seats.AsNoTracking()
            .Where(x => x.ReservationId == reservation.Id)
            .OrderBy(x => x.SeatNumber)
            .Select(x => x.SeatNumber)
            .ToListAsync();

        var paymentStatus = payment?.Status.ToString() ?? (reservation.PaymentMethod == PaymentMethod.CashOffice ? "Pending" : "Pending");
        var ticketText = tickets.Count > 0 ? string.Join(", ", tickets) : "-";
        var seatText = seatNumbers.Count > 0 ? string.Join(", ", seatNumbers) : "-";

        return $"""
        <div style="font-family:Arial,sans-serif;max-width:720px;margin:auto;border:1px solid #d7dde4;border-radius:16px;padding:24px;color:#19222c">
            <h1 style="margin-top:0;color:#0a6c74">Arlian Trans</h1>
            <h2 style="margin-bottom:8px">Detajet e rezervimit / biletes</h2>
            <p>Pershendetje {Html(customer.FirstName)} {Html(customer.LastName)},</p>
            <p>Me poshte jane detajet e rezervimit tuaj.</p>
            <table style="width:100%;border-collapse:collapse">
                {Row("Klienti", $"{customer.FirstName} {customer.LastName}")}
                {Row("Numri i rezervimit", $"#{reservation.Id}")}
                {Row("Numri i biletes", ticketText)}
                {Row("Qyteti i nisjes", trip.DepartureCity)}
                {Row("Destinacioni", trip.Destination)}
                {Row("Shteti", trip.Country)}
                {Row("Data e nisjes", trip.DepartureDate.ToString("dd/MM/yyyy"))}
                {Row("Ora e nisjes", trip.DepartureTime.ToString("HH:mm"))}
                {Row("Data e kthimit", trip.ReturnDate.ToString("dd/MM/yyyy"))}
                {Row("Ora e kthimit", trip.ReturnTime.ToString("HH:mm"))}
                {Row("Numri i uleseve", reservation.SeatCount.ToString())}
                {Row("Numrat e uleseve", seatText)}
                {Row("Cmimi per person", $"{trip.Price:0.00} EUR")}
                {Row("Shuma totale", $"{reservation.TotalAmount:0.00} EUR")}
                {Row("Menyra e pageses", reservation.PaymentMethod == PaymentMethod.Card ? "Kartele" : "CASH")}
                {Row("Statusi i pageses", paymentStatus)}
                {Row("Statusi i rezervimit", reservation.Status.ToString())}
            </table>
            <div style="margin-top:20px;padding-top:14px;border-top:1px solid #d7dde4;color:#5d6773">
                <strong>Arlian Trans</strong><br />
                Tel: +383 44 123 456<br />
                Email: info@arliantrans.com<br />
                Prishtine, Kosove
            </div>
        </div>
        """;
    }

    private static string BuildTicketSubject(Reservation reservation)
    {
        var ticketNumber = reservation.Tickets.OrderByDescending(x => x.Id).FirstOrDefault()?.TicketNumber;
        return string.IsNullOrWhiteSpace(ticketNumber)
            ? $"Rezervimi #{reservation.Id} - Arlian Trans"
            : $"Bileta {ticketNumber} - Arlian Trans";
    }

    private EmailSettings ReadSettings()
    {
        var section = configuration.GetSection("EmailSettings");
        var fallback = configuration.GetSection("Email");
        var password = section["Password"] ?? fallback["Password"];
        var passwordEnvName = section["PasswordEnvironmentVariable"] ?? fallback["PasswordEnvironmentVariable"];
        if (!string.IsNullOrWhiteSpace(passwordEnvName))
        {
            password = Environment.GetEnvironmentVariable(passwordEnvName)
                ?? Environment.GetEnvironmentVariable(passwordEnvName, EnvironmentVariableTarget.User)
                ?? Environment.GetEnvironmentVariable(passwordEnvName, EnvironmentVariableTarget.Machine)
                ?? password;
        }

        return new EmailSettings
        {
            SmtpHost = section["SmtpHost"] ?? fallback["SmtpHost"] ?? Environment.GetEnvironmentVariable("EMAILSETTINGS__SMTPHOST") ?? string.Empty,
            SmtpPort = int.TryParse(section["SmtpPort"] ?? fallback["SmtpPort"] ?? Environment.GetEnvironmentVariable("EMAILSETTINGS__SMTPPORT"), out var port) ? port : 587,
            EnableSsl = bool.TryParse(section["EnableSsl"] ?? fallback["EnableSsl"] ?? Environment.GetEnvironmentVariable("EMAILSETTINGS__ENABLESSL"), out var ssl) ? ssl : true,
            SenderName = section["SenderName"] ?? fallback["SenderName"] ?? "Arlian Trans",
            SenderEmail = section["SenderEmail"] ?? fallback["From"] ?? fallback["SenderEmail"] ?? string.Empty,
            Username = section["Username"] ?? fallback["Username"] ?? string.Empty,
            Password = password ?? string.Empty
        };
    }

    private static bool IsValidEmail(string value)
    {
        try
        {
            _ = new MailAddress(value);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string GetFullError(Exception ex)
    {
        var messages = new List<string>();
        for (var current = ex; current is not null; current = current.InnerException)
        {
            messages.Add(current.Message);
        }
        return string.Join(" | ", messages);
    }

    private static string Row(string label, string value) =>
        $"""<tr><td style="padding:8px 10px;border-bottom:1px solid #eef3f6"><strong>{Html(label)}</strong></td><td style="padding:8px 10px;border-bottom:1px solid #eef3f6">{Html(value)}</td></tr>""";

    private static string Html(string value) => WebUtility.HtmlEncode(value);

    private class EmailSettings
    {
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public bool EnableSsl { get; set; } = true;
        public string SenderName { get; set; } = "Arlian Trans";
        public string SenderEmail { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(SmtpHost)) throw new InvalidOperationException("SMTP host mungon.");
            if (SmtpPort <= 0) throw new InvalidOperationException("SMTP port nuk eshte valid.");
            if (!IsValidEmail(SenderEmail)) throw new InvalidOperationException($"SenderEmail nuk eshte valid: {SenderEmail}");
            if (string.IsNullOrWhiteSpace(Username)) throw new InvalidOperationException("SMTP username mungon.");
            if (string.IsNullOrWhiteSpace(Password)) throw new InvalidOperationException("SMTP password/App Password mungon.");
        }
    }
}
