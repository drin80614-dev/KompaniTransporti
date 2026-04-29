using Microsoft.Data.Sqlite;

var dbPaths = new[]
{
    Path.GetFullPath(Path.Combine("..", "ArlianTrans.Web", "database", "arlian_trans.db")),
    Path.GetFullPath(Path.Combine("..", "ArlianTrans.Web", "App_Data", "arliantrans.db")),
    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "Databaz transporti", "ArlianTrans.Web", "database", "arlian_trans.db")
};

foreach (var dbPath in dbPaths)
{
    Console.WriteLine($"DB: {dbPath}");
    if (!File.Exists(dbPath))
    {
        Console.WriteLine("missing");
        continue;
    }

    using var connection = new SqliteConnection($"Data Source={dbPath}");
    connection.Open();

    using var countCommand = connection.CreateCommand();
    countCommand.CommandText = "SELECT COUNT(*) FROM Trips";
    Console.WriteLine($"Trips: {countCommand.ExecuteScalar()}");

    using var columnsCommand = connection.CreateCommand();
    columnsCommand.CommandText = "SELECT name FROM pragma_table_info('Reservations') WHERE name IN ('EmailSent','EmailSentAt','EmailErrorMessage')";
    using (var columnsReader = columnsCommand.ExecuteReader())
    {
        var columns = new List<string>();
        while (columnsReader.Read()) columns.Add(columnsReader.GetString(0));
        Console.WriteLine($"Reservation email columns: {string.Join(", ", columns)}");
    }

    using var emailLogsCommand = connection.CreateCommand();
    emailLogsCommand.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='EmailLogs'";
    Console.WriteLine($"EmailLogs table exists: {Convert.ToInt32(emailLogsCommand.ExecuteScalar()) == 1}");

    using var latestCommand = connection.CreateCommand();
    latestCommand.CommandText = """
        SELECT Id, DepartureCity, Destination, Country, DepartureDate, Status
        FROM Trips
        ORDER BY Id DESC
        LIMIT 8
        """;
    using var reader = latestCommand.ExecuteReader();
    while (reader.Read())
    {
        Console.WriteLine($"{reader.GetInt32(0)} | {reader.GetString(1)} - {reader.GetString(2)} | {reader.GetString(3)} | {reader.GetString(4)} | Status={reader.GetInt32(5)}");
    }

    using var searchCommand = connection.CreateCommand();
    searchCommand.CommandText = """
        SELECT Id, DepartureCity, Destination, Country, DepartureDate, Status
        FROM Trips
        WHERE lower(Destination) LIKE '%toky%' OR lower(Country) LIKE '%toky%' OR lower(DepartureCity) LIKE '%toky%'
        ORDER BY Id DESC
        """;
    using var searchReader = searchCommand.ExecuteReader();
    Console.WriteLine("Search: toky");
    while (searchReader.Read())
    {
        Console.WriteLine($"{searchReader.GetInt32(0)} | {searchReader.GetString(1)} - {searchReader.GetString(2)} | {searchReader.GetString(3)} | {searchReader.GetString(4)} | Status={searchReader.GetInt32(5)}");
    }

    Console.WriteLine();
}
