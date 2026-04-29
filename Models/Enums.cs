namespace ArlianTrans.Web.Models;

public enum TransportType
{
    Autobus = 1,
    Minibus = 2,
    Van = 3,
    Aeroplan = 4,
    Tren = 5
}

public enum TripStatus
{
    Active = 1,
    Cancelled = 2,
    Completed = 3
}

public enum PaymentMethod
{
    Card = 1,
    CashOffice = 2
}

public enum ReservationStatus
{
    Pending = 1,
    Confirmed = 2,
    Cancelled = 3,
    PendingOfficePayment = 4
}

public enum PaymentStatus
{
    Pending = 1,
    Completed = 2,
    Failed = 3,
    Cancelled = 4
}

public enum SeatStatus
{
    Free = 1,
    Reserved = 2,
    Booked = 3
}

public enum AdminRole
{
    Manager = 1,
    OfficeStaff = 2
}
