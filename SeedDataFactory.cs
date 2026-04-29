using ArlianTrans.Web.Models;

namespace ArlianTrans.Web.Data;

public static class SeedDataFactory
{
    public static List<Trip> CreateTrips()
    {
        var destinations = new (string City, string Country)[]
        {
            ("Prizren", "Kosovë"), ("Pejë", "Kosovë"), ("Gjakovë", "Kosovë"), ("Mitrovicë", "Kosovë"),
            ("Gjilan", "Kosovë"), ("Ferizaj", "Kosovë"), ("Podujevë", "Kosovë"), ("Suharekë", "Kosovë"),
            ("Tiranë", "Shqipëri"), ("Durrës", "Shqipëri"), ("Vlorë", "Shqipëri"), ("Shkodër", "Shqipëri"),
            ("Shkup", "Maqedoni e Veriut"), ("Tetovë", "Maqedoni e Veriut"), ("Ohër", "Maqedoni e Veriut"), ("Strugë", "Maqedoni e Veriut"),
            ("Podgoricë", "Mali i Zi"), ("Budva", "Mali i Zi"), ("Ulqin", "Mali i Zi"), ("Sarajevë", "Bosnje dhe Hercegovinë"),
            ("Mostar", "Bosnje dhe Hercegovinë"), ("Beograd", "Serbi"), ("Novi Sad", "Serbi"), ("Nish", "Serbi"),
            ("Zagreb", "Kroaci"), ("Split", "Kroaci"), ("Dubrovnik", "Kroaci"), ("Ljubljanë", "Sloveni"),
            ("Maribor", "Sloveni"), ("Vjenë", "Austri"), ("Salzburg", "Austri"), ("Berlin", "Gjermani"),
            ("Munich", "Gjermani"), ("Frankfurt", "Gjermani"), ("Hamburg", "Gjermani"), ("Stuttgart", "Gjermani"),
            ("Zurich", "Zvicër"), ("Geneva", "Zvicër"), ("Basel", "Zvicër"), ("Lausanne", "Zvicër"),
            ("Paris", "Francë"), ("Lyon", "Francë"), ("Marseille", "Francë"), ("London", "Mbretëria e Bashkuar"),
            ("Manchester", "Mbretëria e Bashkuar"), ("Birmingham", "Mbretëria e Bashkuar"), ("Rome", "Itali"), ("Milan", "Itali"),
            ("Naples", "Itali"), ("Venice", "Itali"), ("Brussels", "Belgjikë"), ("Amsterdam", "Holandë"),
            ("Rotterdam", "Holandë"), ("The Hague", "Holandë"), ("Stockholm", "Suedi"), ("Gothenburg", "Suedi"),
            ("Oslo", "Norvegji"), ("Bergen", "Norvegji"), ("Copenhagen", "Danimarkë"), ("Aarhus", "Danimarkë"),
            ("Istanbul", "Turqi"), ("Ankara", "Turqi"), ("Izmir", "Turqi"), ("Athens", "Greqi"),
            ("Thessaloniki", "Greqi"), ("Sofia", "Bullgari"), ("Varna", "Bullgari"), ("Bucharest", "Rumani"),
            ("Cluj-Napoca", "Rumani"), ("Budapest", "Hungari"), ("Prague", "Çeki"), ("Brno", "Çeki"),
            ("Warsaw", "Poloni"), ("Krakow", "Poloni"), ("Gdansk", "Poloni"), ("Madrid", "Spanjë"),
            ("Barcelona", "Spanjë"), ("Valencia", "Spanjë"), ("Lisbon", "Portugali"), ("Porto", "Portugali"),
            ("Dublin", "Irlandë"), ("Cork", "Irlandë"), ("Reykjavik", "Islandë"), ("Helsinki", "Finlandë"),
            ("Tallinn", "Estoni"), ("Riga", "Letoni"), ("Vilnius", "Lituani"), ("Moscow", "Rusi"),
            ("Saint Petersburg", "Rusi"), ("Kyiv", "Ukrainë"), ("Lviv", "Ukrainë"), ("New York", "SHBA"),
            ("Boston", "SHBA"), ("Chicago", "SHBA"), ("Toronto", "Kanada"), ("Montreal", "Kanada"),
            ("Dubai", "Emiratet e Bashkuara Arabe"), ("Abu Dhabi", "Emiratet e Bashkuara Arabe"), ("Doha", "Katar"), ("Riyadh", "Arabia Saudite"),
            ("Jeddah", "Arabia Saudite"), ("Cairo", "Egjipt"), ("Alexandria", "Egjipt"), ("Casablanca", "Marok"),
            ("Tunis", "Tunizi"), ("Algiers", "Algjeri"), ("Tokyo", "Japoni"), ("Osaka", "Japoni"),
            ("Seoul", "Kore e Jugut"), ("Singapore", "Singapor"), ("Bangkok", "Tajlandë"), ("Kuala Lumpur", "Malajzi"),
            ("Sydney", "Australi"), ("Melbourne", "Australi"), ("Auckland", "Zelandë e Re"), ("Johannesburg", "Afrika e Jugut"),
            ("Cape Town", "Afrika e Jugut"), ("Sao Paulo", "Brazil"), ("Rio de Janeiro", "Brazil"), ("Mexico City", "Meksikë")
        };

        var transportCycle = new[]
        {
            TransportType.Autobus, TransportType.Minibus, TransportType.Van, TransportType.Aeroplan, TransportType.Tren
        };

        var trips = new List<Trip>();
        var startDate = new DateTime(2026, 5, 1);

        for (var i = 0; i < destinations.Length; i++)
        {
            var destination = destinations[i];
            var transport = transportCycle[i % transportCycle.Length];
            var departDate = startDate.AddDays(i % 31).AddDays(i / 12);
            var departTime = new TimeOnly(6 + (i % 10), (i * 7) % 60);
            var durationHours = transport switch
            {
                TransportType.Aeroplan => 3 + (i % 8),
                TransportType.Tren => 8 + (i % 6),
                TransportType.Van => 6 + (i % 7),
                TransportType.Minibus => 5 + (i % 7),
                _ => 7 + (i % 9)
            };
            var returnDate = departDate.AddDays(2 + (i % 8));
            var returnTime = departTime.AddHours((durationHours + 2) % 24);
            var totalSeats = transport switch
            {
                TransportType.Autobus => 60,
                TransportType.Minibus => 28,
                TransportType.Van => 16,
                TransportType.Aeroplan => 180,
                _ => 90
            };
            var price = transport switch
            {
                TransportType.Aeroplan => 110 + (i * 6 % 220),
                TransportType.Tren => 45 + (i * 4 % 90),
                TransportType.Van => 30 + (i * 3 % 70),
                TransportType.Minibus => 28 + (i * 3 % 65),
                _ => 25 + (i * 5 % 120)
            };

            trips.Add(new Trip
            {
                DepartureCity = "Prishtinë",
                Destination = destination.City,
                Country = destination.Country,
                DepartureDate = DateOnly.FromDateTime(departDate),
                DepartureTime = departTime,
                ReturnDate = DateOnly.FromDateTime(returnDate),
                ReturnTime = returnTime,
                Price = price,
                TransportType = transport,
                TotalSeats = totalSeats,
                AvailableSeats = totalSeats,
                OccupiedSeats = 0,
                Status = TripStatus.Active,
                Description = $"Linjë profesionale Arlian Trans nga Prishtina për në {destination.City}, {destination.Country}."
            });
        }

        return trips;
    }
}
