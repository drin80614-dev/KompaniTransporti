BEGIN TRANSACTION;

UPDATE Trips
SET AvailableSeats = AvailableSeats - @SeatCount,
    OccupiedSeats = OccupiedSeats + @SeatCount
WHERE Id = @TripId
  AND AvailableSeats >= @SeatCount
  AND Status = 1;

UPDATE Seats
SET Status = @SeatStatus,
    ReservationId = @ReservationId,
    TicketId = @TicketId
WHERE Id IN
(
    SELECT Id
    FROM Seats
    WHERE TripId = @TripId AND Status = 1
    ORDER BY SeatNumber
    LIMIT @SeatCount
);

COMMIT;
