using Microsoft.Data.Sqlite;

namespace Reiseplaner;

public static class DbInitializer
{
    // Pfad zur SQLite-Datenbankdatei (im App-Verzeichnis)
    public static readonly string ConnectionString = "Data Source=app.db";

    public static void Initialize()
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Reisen (
                Id         INTEGER PRIMARY KEY AUTOINCREMENT,
                Titel      TEXT    NOT NULL,
                Zielort    TEXT    NOT NULL,
                Startdatum TEXT    NOT NULL,
                Enddatum   TEXT    NOT NULL,
                Budget     REAL    NOT NULL DEFAULT 0
            );

            CREATE TABLE IF NOT EXISTS Programmpunkte (
                Id         INTEGER PRIMARY KEY AUTOINCREMENT,
                Titel      TEXT    NOT NULL,
                Datum      TEXT    NOT NULL,
                Kategorie  TEXT    NOT NULL,
                Kosten     REAL    NOT NULL DEFAULT 0,
                Erledigt   INTEGER NOT NULL DEFAULT 0,
                ReiseId    INTEGER NOT NULL,
                FOREIGN KEY (ReiseId) REFERENCES Reisen(Id)
            );";
        cmd.ExecuteNonQuery();
    }
}
