using ArlianTrans.Web.Data;
using ArlianTrans.Web.Models;
using ArlianTrans.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ArlianTrans.Web.Services;

public class BookingService(AppDbContext context, IEmailService emailService)
{
    public async Task<BookingResult> CreateReservationAsync(ReservationCreateViewModel model)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var trip = await context.Trips.FirstOrDefaultAsync(x => x.Id == model.TripId);
            if (trip is null || trip.Status != TripStatus.Active)
            {
                return new BookingResult { Message = "Udhetimi nuk u gjet ose nuk eshte aktiv." };
            }

            var seats = await GetRequestedSeatsAsync(trip.Id, model.SeatCount, model.SelectedSeatNumbers);
            if (seats.Count != model.SeatCount)
            {
                await transaction.RollbackAsync();
                return new BookingResult { Message = "Uleset e zgjedhura nuk jane me te lira. Zgjidh ulëse tjera." };
            }

            var seatUpdate = await ReserveTripSeatsAsync(model.SeatCount, model.TripId);
            if (seatUpdate == 0)
            {
                await transaction.RollbackAsync();
                return new BookingResult { Message = "Nuk ka ulese te mjaftueshme te lira per kete udhetim." };
            }

            var customer = await GetOrCreateCustomerAsync(model.FirstName, model.LastName, model.PhoneNumber, model.Email);

            var reservation = new Reservation
            {
                TripId = trip.Id,
                CustomerId = customer.Id,
                SeatCount = model.SeatCount,
                PaymentMethod = model.PaymentMethod,
                Status = model.PaymentMethod == PaymentMethod.CashOffice ? ReservationStatus.PendingOfficePayment : ReservationStatus.Pending,
                TotalAmount = trip.Price * model.SeatCount,
                Notes = model.PaymentMethod == PaymentMethod.CashOffice ? "Ne pritje per pagese ne zyre" : "Rezervim online ne pritje"
            };

            context.Reservations.Add(reservation);
            await context.SaveChangesAsync();

            var ticket = new Ticket
            {
                ReservationId = reservation.Id,
                TicketNumber = CreateTicketNumber(),
                PassengerName = $"{model.FirstName} {model.LastName}"
            };
            context.Tickets.Add(ticket);
            await context.SaveChangesAsync();

            foreach (var seat in seats)
            {
                seat.Status = SeatStatus.Reserved;
                seat.ReservationId = reservation.Id;
                seat.TicketId = ticket.Id;
            }

            reservation.Tickets.Add(ticket);
            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            await emailService.SendReservationEmailAsync(reservation.Id);

            return new BookingResult
            {
                Success = true,
                ReservationId = reservation.Id,
                TicketNumber = ticket.TicketNumber,
                Message = "Rezervimi u ruajt me sukses. Bileta u pergatit per email."
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<BookingResult> PurchaseTicketAsync(TicketPurchaseViewModel model)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var trip = await context.Trips.FirstOrDefaultAsync(x => x.Id == model.TripId);
            if (trip is null || trip.Status != TripStatus.Active)
            {
                return new BookingResult { Message = "Udhetimi nuk eshte i disponueshem." };
            }

            var seats = await GetRequestedSeatsAsync(trip.Id, model.SeatCount, model.SelectedSeatNumbers);
            if (seats.Count != model.SeatCount)
            {
                await transaction.RollbackAsync();
                return new BookingResult { Message = "Uleset e zgjedhura nuk jane me te lira. Zgjidh ulese tjera." };
            }

            var seatUpdate = await ReserveTripSeatsAsync(model.SeatCount, model.TripId);
            if (seatUpdate == 0)
            {
                await transaction.RollbackAsync();
                return new BookingResult { Message = "Nuk ka ulese te lira per blerjen e kerkuar." };
            }

            var customer = await GetOrCreateCustomerAsync(model.FirstName, model.LastName, model.PhoneNumber, model.Email);

            var reservation = new Reservation
            {
                TripId = trip.Id,
                CustomerId = customer.Id,
                SeatCount = model.SeatCount,
                PaymentMethod = model.PaymentMethod,
                Status = model.PaymentMethod == PaymentMethod.Card ? ReservationStatus.Confirmed : ReservationStatus.PendingOfficePayment,
                TotalAmount = trip.Price * model.SeatCount,
                Notes = model.PaymentMethod == PaymentMethod.Card ? "Bilete e paguar online" : "Ne pritje per pagese ne zyre"
            };

            context.Reservations.Add(reservation);
            await context.SaveChangesAsync();

            var payment = new Payment
            {
                ReservationId = reservation.Id,
                PaymentMethod = model.PaymentMethod,
                Status = model.PaymentMethod == PaymentMethod.Card ? PaymentStatus.Completed : PaymentStatus.Pending,
                Amount = reservation.TotalAmount,
                CardHolderName = model.PaymentMethod == PaymentMethod.Card ? model.CardHolderName : null,
                MaskedCardNumber = model.PaymentMethod == PaymentMethod.Card ? MaskCard(model.CardNumber) : null,
                ReferenceCode = $"PAY-{DateTime.UtcNow:yyyyMMddHHmmss}-{reservation.Id}"
            };

            context.Payments.Add(payment);
            await context.SaveChangesAsync();

            foreach (var seat in seats)
            {
                seat.Status = model.PaymentMethod == PaymentMethod.Card ? SeatStatus.Booked : SeatStatus.Reserved;
                seat.ReservationId = reservation.Id;
            }

            string? ticketNumber = null;
            if (model.PaymentMethod == PaymentMethod.Card)
            {
                var ticket = new Ticket
                {
                    ReservationId = reservation.Id,
                    TicketNumber = CreateTicketNumber(),
                    PassengerName = $"{model.FirstName} {model.LastName}"
                };

                context.Tickets.Add(ticket);
                await context.SaveChangesAsync();

                foreach (var seat in seats)
                {
                    seat.TicketId = ticket.Id;
                }

                ticketNumber = ticket.TicketNumber;
                reservation.Tickets.Add(ticket);
            }

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            await emailService.SendReservationEmailAsync(reservation.Id);

            return new BookingResult
            {
                Success = true,
                ReservationId = reservation.Id,
                TicketNumber = ticketNumber,
                Message = model.PaymentMethod == PaymentMethod.Card
                    ? "Bileta u ble me sukses. Konfirmimi u pergatit per email."
                    : "Rezervimi u krijua dhe eshte ne pritje per pagese ne zyre."
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> ConfirmCashPaymentAsync(int reservationId)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        var reservation = await context.Reservations
            .Include(x => x.Customer)
            .Include(x => x.Trip)
            .Include(x => x.Payments)
            .Include(x => x.Tickets)
            .FirstOrDefaultAsync(x => x.Id == reservationId);

        if (reservation is null || reservation.Status != ReservationStatus.PendingOfficePayment)
        {
            return false;
        }

        reservation.Status = ReservationStatus.Confirmed;
        var payment = reservation.Payments.FirstOrDefault();
        if (payment is not null)
        {
            payment.Status = PaymentStatus.Completed;
        }

        var seats = await context.Seats.Where(x => x.ReservationId == reservationId && x.Status == SeatStatus.Reserved).ToListAsync();

        var ticket = reservation.Tickets.FirstOrDefault();
        if (ticket is null)
        {
            ticket = new Ticket
            {
                ReservationId = reservation.Id,
                TicketNumber = CreateTicketNumber(),
                PassengerName = $"{reservation.Customer?.FirstName} {reservation.Customer?.LastName}".Trim()
            };
            context.Tickets.Add(ticket);
            await context.SaveChangesAsync();
        }

        foreach (var seat in seats)
        {
            seat.Status = SeatStatus.Booked;
            seat.TicketId = ticket.Id;
        }

        await context.SaveChangesAsync();
        await transaction.CommitAsync();
        await emailService.SendReservationEmailAsync(reservation.Id);
        return true;
    }

    public async Task<bool> CancelReservationAsync(int reservationId)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        var reservation = await context.Reservations.FirstOrDefaultAsync(x => x.Id == reservationId);
        if (reservation is null || reservation.Status == ReservationStatus.Cancelled)
        {
            return false;
        }

        reservation.Status = ReservationStatus.Cancelled;
        await context.Database.ExecuteSqlRawAsync(
            "UPDATE Trips SET AvailableSeats = AvailableSeats + {0}, OccupiedSeats = CASE WHEN OccupiedSeats >= {0} THEN OccupiedSeats - {0} ELSE 0 END WHERE Id = {1}",
            reservation.SeatCount, reservation.TripId);

        var payment = await context.Payments.FirstOrDefaultAsync(x => x.ReservationId == reservationId);
        if (payment is not null && payment.Status == PaymentStatus.Pending)
        {
            payment.Status = PaymentStatus.Cancelled;
        }

        var seats = await context.Seats.Where(x => x.ReservationId == reservationId).ToListAsync();
        foreach (var seat in seats)
        {
            seat.Status = SeatStatus.Free;
            seat.ReservationId = null;
            seat.TicketId = null;
        }

        await context.SaveChangesAsync();
        await transaction.CommitAsync();
        return true;
    }

    private async Task<int> ReserveTripSeatsAsync(int seatCount, int tripId) =>
        await context.Database.ExecuteSqlRawAsync(
            "UPDATE Trips SET AvailableSeats = AvailableSeats - {0}, OccupiedSeats = OccupiedSeats + {0} WHERE Id = {1} AND AvailableSeats >= {0} AND Status = {2}",
            seatCount, tripId, TripStatus.Active);

    private async Task<List<Seat>> GetRequestedSeatsAsync(int tripId, int seatCount, string? selectedSeatNumbers)
    {
        var selectedSeats = ReservationCreateViewModel.ParseSelectedSeats(selectedSeatNumbers);
        if (selectedSeats.Count > 0)
        {
            return await context.Seats
                .Where(x => x.TripId == tripId && x.Status == SeatStatus.Free && selectedSeats.Contains(x.SeatNumber))
                .OrderBy(x => x.SeatNumber)
                .ToListAsync();
        }

        return await context.Seats
            .Where(x => x.TripId == tripId && x.Status == SeatStatus.Free)
            .OrderBy(x => x.SeatNumber)
            .Take(seatCount)
            .ToListAsync();
    }

    private async Task<Customer> GetOrCreateCustomerAsync(string firstName, string lastName, string phoneNumber, string email)
    {
        var customer = await context.Customers.FirstOrDefaultAsync(x => x.Email == email || x.PhoneNumber == phoneNumber);
        if (customer is not null)
        {
            customer.FirstName = firstName;
            customer.LastName = lastName;
            customer.PhoneNumber = phoneNumber;
            customer.Email = email;
            await context.SaveChangesAsync();
            return customer;
        }

        customer = new Customer
        {
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = phoneNumber,
            Email = email
        };

        context.Customers.Add(customer);
        await context.SaveChangesAsync();
        return customer;
    }

    private static string MaskCard(string? number)
    {
        if (string.IsNullOrWhiteSpace(number)) return string.Empty;
        var digits = new string(number.Where(char.IsDigit).ToArray());
        return digits.Length < 4 ? "****" : $"**** **** **** {digits[^4..]}";
    }

    private static string CreateTicketNumber() => $"ARL-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
}
