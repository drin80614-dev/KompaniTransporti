CREATE TEMP TABLE IF NOT EXISTS SeedDestinations
(
    City TEXT NOT NULL,
    Country TEXT NOT NULL
);

DELETE FROM SeedDestinations;

INSERT INTO SeedDestinations (City, Country) VALUES
('Prizren', 'Kosovë'), ('Pejë', 'Kosovë'), ('Gjakovë', 'Kosovë'), ('Mitrovicë', 'Kosovë'),
('Gjilan', 'Kosovë'), ('Ferizaj', 'Kosovë'), ('Podujevë', 'Kosovë'), ('Suharekë', 'Kosovë'),
('Tiranë', 'Shqipëri'), ('Durrës', 'Shqipëri'), ('Vlorë', 'Shqipëri'), ('Shkodër', 'Shqipëri'),
('Shkup', 'Maqedoni e Veriut'), ('Tetovë', 'Maqedoni e Veriut'), ('Ohër', 'Maqedoni e Veriut'), ('Strugë', 'Maqedoni e Veriut'),
('Podgoricë', 'Mali i Zi'), ('Budva', 'Mali i Zi'), ('Ulqin', 'Mali i Zi'), ('Sarajevë', 'Bosnje dhe Hercegovinë'),
('Mostar', 'Bosnje dhe Hercegovinë'), ('Beograd', 'Serbi'), ('Novi Sad', 'Serbi'), ('Nish', 'Serbi'),
('Zagreb', 'Kroaci'), ('Split', 'Kroaci'), ('Dubrovnik', 'Kroaci'), ('Ljubljanë', 'Sloveni'),
('Maribor', 'Sloveni'), ('Vjenë', 'Austri'), ('Salzburg', 'Austri'), ('Berlin', 'Gjermani'),
('Munich', 'Gjermani'), ('Frankfurt', 'Gjermani'), ('Hamburg', 'Gjermani'), ('Stuttgart', 'Gjermani'),
('Zurich', 'Zvicër'), ('Geneva', 'Zvicër'), ('Basel', 'Zvicër'), ('Lausanne', 'Zvicër'),
('Paris', 'Francë'), ('Lyon', 'Francë'), ('Marseille', 'Francë'), ('London', 'Mbretëria e Bashkuar'),
('Manchester', 'Mbretëria e Bashkuar'), ('Birmingham', 'Mbretëria e Bashkuar'), ('Rome', 'Itali'), ('Milan', 'Itali'),
('Naples', 'Itali'), ('Venice', 'Itali'), ('Brussels', 'Belgjikë'), ('Amsterdam', 'Holandë'),
('Rotterdam', 'Holandë'), ('The Hague', 'Holandë'), ('Stockholm', 'Suedi'), ('Gothenburg', 'Suedi'),
('Oslo', 'Norvegji'), ('Bergen', 'Norvegji'), ('Copenhagen', 'Danimarkë'), ('Aarhus', 'Danimarkë'),
('Istanbul', 'Turqi'), ('Ankara', 'Turqi'), ('Izmir', 'Turqi'), ('Athens', 'Greqi'),
('Thessaloniki', 'Greqi'), ('Sofia', 'Bullgari'), ('Varna', 'Bullgari'), ('Bucharest', 'Rumani'),
('Cluj-Napoca', 'Rumani'), ('Budapest', 'Hungari'), ('Prague', 'Çeki'), ('Brno', 'Çeki'),
('Warsaw', 'Poloni'), ('Krakow', 'Poloni'), ('Gdansk', 'Poloni'), ('Madrid', 'Spanjë'),
('Barcelona', 'Spanjë'), ('Valencia', 'Spanjë'), ('Lisbon', 'Portugali'), ('Porto', 'Portugali'),
('Dublin', 'Irlandë'), ('Cork', 'Irlandë'), ('Reykjavik', 'Islandë'), ('Helsinki', 'Finlandë'),
('Tallinn', 'Estoni'), ('Riga', 'Letoni'), ('Vilnius', 'Lituani'), ('Moscow', 'Rusi'),
('Saint Petersburg', 'Rusi'), ('Kyiv', 'Ukrainë'), ('Lviv', 'Ukrainë'), ('New York', 'SHBA'),
('Boston', 'SHBA'), ('Chicago', 'SHBA'), ('Toronto', 'Kanada'), ('Montreal', 'Kanada'),
('Dubai', 'Emiratet e Bashkuara Arabe'), ('Abu Dhabi', 'Emiratet e Bashkuara Arabe'), ('Doha', 'Katar'), ('Riyadh', 'Arabia Saudite'),
('Jeddah', 'Arabia Saudite'), ('Cairo', 'Egjipt'), ('Alexandria', 'Egjipt'), ('Casablanca', 'Marok'),
('Tunis', 'Tunizi'), ('Algiers', 'Algjeri'), ('Tokyo', 'Japoni'), ('Osaka', 'Japoni'),
('Seoul', 'Kore e Jugut'), ('Singapore', 'Singapor'), ('Bangkok', 'Tajlandë'), ('Kuala Lumpur', 'Malajzi'),
('Sydney', 'Australi'), ('Melbourne', 'Australi'), ('Auckland', 'Zelandë e Re'), ('Johannesburg', 'Afrika e Jugut'),
('Cape Town', 'Afrika e Jugut'), ('Sao Paulo', 'Brazil'), ('Rio de Janeiro', 'Brazil'), ('Mexico City', 'Meksikë');

INSERT INTO Trips
(
    DepartureCity, Destination, Country, DepartureDate, DepartureTime, ReturnDate, ReturnTime,
    Price, TransportType, TotalSeats, AvailableSeats, OccupiedSeats, Status, Description
)
SELECT
    'Prishtinë',
    City,
    Country,
    date('2026-05-01', printf('+%d day', ((rowid - 1) % 31) + ((rowid - 1) / 12))),
    time(printf('%02d:%02d:00', 6 + ((rowid - 1) % 10), (((rowid - 1) * 7) % 60))),
    date('2026-05-03', printf('+%d day', ((rowid - 1) % 31) + ((rowid - 1) / 12) + ((rowid - 1) % 8))),
    time(printf('%02d:%02d:00', 12 + ((rowid - 1) % 10), (((rowid - 1) * 7) % 60))),
    CASE ((rowid - 1) % 5)
        WHEN 3 THEN 110 + (((rowid - 1) * 6) % 220)
        WHEN 4 THEN 45 + (((rowid - 1) * 4) % 90)
        WHEN 2 THEN 30 + (((rowid - 1) * 3) % 70)
        WHEN 1 THEN 28 + (((rowid - 1) * 3) % 65)
        ELSE 25 + (((rowid - 1) * 5) % 120)
    END,
    CASE ((rowid - 1) % 5) WHEN 0 THEN 1 WHEN 1 THEN 2 WHEN 2 THEN 3 WHEN 3 THEN 4 ELSE 5 END,
    CASE ((rowid - 1) % 5) WHEN 0 THEN 60 WHEN 1 THEN 28 WHEN 2 THEN 16 WHEN 3 THEN 180 ELSE 90 END,
    CASE ((rowid - 1) % 5) WHEN 0 THEN 60 WHEN 1 THEN 28 WHEN 2 THEN 16 WHEN 3 THEN 180 ELSE 90 END,
    0,
    1,
    'Linjë profesionale Arlian Trans nga Prishtina.'
FROM SeedDestinations;
