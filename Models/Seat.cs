namespace ArlianTrans.Web.Models;

public class Seat
{
    public int Id { get; set; }
    public int TripId { get; set; }
    public int SeatNumber { get; set; }
    public SeatStatus Status { get; set; }
    public int? ReservationId { get; set; }
    public int? TicketId { get; set; }
    public Trip? Trip { get; set; }
}
