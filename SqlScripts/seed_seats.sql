WITH RECURSIVE numbers(n) AS
(
    SELECT 1
    UNION ALL
    SELECT n + 1 FROM numbers WHERE n < 300
)
INSERT INTO Seats (TripId, SeatNumber, Status)
SELECT t.Id, numbers.n, 1
FROM Trips t
JOIN numbers ON numbers.n <= t.TotalSeats;
