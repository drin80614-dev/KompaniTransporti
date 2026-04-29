# Arlian Trans

Website funksional për kompani transporti me `ASP.NET Core MVC`, `SQLite`, frontend responsive dhe admin panel. Të gjitha të dhënat ruhen në një file real SQLite që mund të hapet manualisht me DB Browser for SQLite ose SQLiteStudio.

## Teknologjitë

- .NET 9
- ASP.NET Core MVC
- Entity Framework Core
- SQLite
- HTML, CSS, JavaScript

## Si startohet projekti

1. Hape terminalin në:
   `C:\Users\DrinKrasniqi\source\repos\Databaz transporti\ArlianTrans.Web`
2. Ekzekuto:
   `dotnet restore`
   `dotnet build`
   `dotnet run`
3. Hape URL-në që shfaqet në terminal.

## Ku gjendet file i databazës

File kryesor i databazës është:

`C:\Users\DrinKrasniqi\source\repos\Databaz transporti\ArlianTrans.Web\database\arlian_trans.db`

Ky file krijohet automatikisht në nisjen e parë të aplikacionit. Të gjitha të dhënat ruhen aty:

- `Trips`
- `Customers`
- `Reservations`
- `Tickets`
- `Payments`
- `Seats`
- `AdminUsers`

## Si hapet me DB Browser for SQLite ose SQLiteStudio

1. Hape DB Browser for SQLite ose SQLiteStudio
2. Zgjidh `Open Database`
3. Shko te:
   `C:\Users\DrinKrasniqi\source\repos\Databaz transporti\ArlianTrans.Web\database\arlian_trans.db`
4. Hape file-in `arlian_trans.db`

Pastaj mund të shohësh direkt tabelat, rekordet dhe të ekzekutosh SQL queries.

## SQL files

Në folderin [database](C:\Users\DrinKrasniqi\source\repos\Databaz transporti\ArlianTrans.Web\database) gjenden:

- [schema.sql](C:\Users\DrinKrasniqi\source\repos\Databaz transporti\ArlianTrans.Web\database\schema.sql)
- [seed.sql](C:\Users\DrinKrasniqi\source\repos\Databaz transporti\ArlianTrans.Web\database\seed.sql)

`schema.sql` përmban:

- `CREATE TABLE` për të gjitha tabelat
- `FOREIGN KEY` relationships
- `INSERT` për admin user demo
- `INSERT` për minimum 100 udhëtime
- `INSERT` për ulëset e secilit udhëtim

`seed.sql` përmban:

- minimum 100 udhëtime nga Prishtina drejt qyteteve në Kosovë, Evropë dhe botë

## Si ekzekutohet schema.sql

Nga një SQLite app:

1. Krijo ose hape databazën `arlian_trans.db`
2. Hape `Execute SQL`
3. Ngarko përmbajtjen e `schema.sql`
4. Ekzekuto skriptin

Nëse përdor `sqlite3` CLI:

```powershell
sqlite3 database/arlian_trans.db ".read database/schema.sql"
```

## Si ekzekutohet seed.sql

Pas krijimit të tabelave, mund të ekzekutosh:

```powershell
sqlite3 database/arlian_trans.db ".read database/seed.sql"
```

Në DB Browser ose SQLiteStudio, hap `Execute SQL`, kopjo përmbajtjen e `seed.sql` dhe ekzekutoje.

Shënim:
- `schema.sql` tashmë përfshin edhe seed bazë për adminin, udhëtimet dhe ulëset.
- `seed.sql` është file i veçantë për seed manual të udhëtimeve.

## Admin paneli

Admin paneli hapet te:

- `/Admin`

Kredencialet demo:

- Username: `admin`
- Password: `Admin123!`

Admin paneli mund të:

- shikojë të gjitha rezervimet
- shtojë udhëtime të reja në databazë
- ndryshojë udhëtime ekzistuese
- fshijë udhëtime pa rezervime aktive
- anulojë rezervime
- konfirmojë pagesat CASH
- shtojë manualisht rezervime
- ndryshojë numrin e ulëseve përmes editimit të udhëtimit
- shikojë pagesat
- shikojë biletat
- shikojë raportet

Kur admini shton ose ndryshon të dhëna, ato ruhen menjëherë në file-in SQLite dhe mbeten të ruajtura edhe pasi website mbyllet dhe hapet sërish.

## Si kontrollohen rezervimet në tabelën Reservations

Me SQL app:

```sql
SELECT * FROM Reservations ORDER BY CreatedAt DESC;
```

Për të parë edhe klientin dhe udhëtimin:

```sql
SELECT r.Id, c.FirstName, c.LastName, t.Destination, r.SeatCount, r.Status, r.TotalAmount
FROM Reservations r
JOIN Customers c ON c.Id = r.CustomerId
JOIN Trips t ON t.Id = r.TripId
ORDER BY r.CreatedAt DESC;
```

## Si shtohen manualisht të dhëna nga SQL app

Shembull për klient:

```sql
INSERT INTO Customers (FirstName, LastName, PhoneNumber, Email, CreatedAt)
VALUES ('Arben', 'Krasniqi', '+38344111222', 'arben@example.com', datetime('now'));
```

Shembull për udhëtim:

```sql
INSERT INTO Trips
(DepartureCity, Destination, Country, DepartureDate, DepartureTime, ReturnDate, ReturnTime, Price, TransportType, TotalSeats, AvailableSeats, OccupiedSeats, Status, Description)
VALUES
('Prishtinë', 'Tiranë', 'Shqipëri', '2026-05-20', '08:00:00', '2026-05-23', '18:00:00', 35.00, 1, 60, 60, 0, 1, 'Linjë manuale e shtuar nga SQL app');
```

Pastaj shto ulëset:

```sql
WITH RECURSIVE numbers(n) AS
(
    SELECT 1
    UNION ALL
    SELECT n + 1 FROM numbers WHERE n < 60
)
INSERT INTO Seats (TripId, SeatNumber, Status)
SELECT (SELECT MAX(Id) FROM Trips), n, 1
FROM numbers;
```

## Logjika e ruajtjes në SQLite

Rezervimet dhe blerjet nuk ruhen në `localStorage`. Burimi kryesor i të dhënave është vetëm SQLite file.

Kur bëhet rezervim ose blerje:

1. kontrollohen ulëset e lira
2. ruhet klienti në `Customers`
3. ruhet rezervimi në `Reservations`
4. ruhet pagesa në `Payments` kur ka pagesë
5. përditësohen `Trips` dhe `Seats`
6. bëhet `COMMIT`
7. në gabim bëhet `ROLLBACK`

## Gjendja fillestare

Në nisjen e parë aplikacioni krijon automatikisht:

- 1 admin demo
- 120 udhëtime
- ulëset për secilin udhëtim

## Verifikim i shpejtë

- `database/arlian_trans.db` mund të hapet manualisht me SQLite app
- faqja `Udhëtimet` lexon të dhënat nga SQLite
- `Rezervimet` ruajnë rekordet në `Reservations`
- `Biletat` ruajnë në `Tickets` dhe `Payments`
- `Admin` shkruan direkt në SQLite file
