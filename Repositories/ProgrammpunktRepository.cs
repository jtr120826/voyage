using Microsoft.Data.Sqlite;
using Reiseplaner.Models;

namespace Reiseplaner.Repositories;

public class ProgrammpunktRepository
{
    public List<Programmpunkt> GetByReise(int reiseId)
    {
        var list = new List<Programmpunkt>();
        using var con = new SqliteConnection(DbInitializer.ConnectionString);
        con.Open();

        var cmd = con.CreateCommand();
        cmd.CommandText = @"
            SELECT Id, Titel, Datum, Kategorie, Kosten, Erledigt, ReiseId
            FROM Programmpunkte
            WHERE ReiseId = $reiseId
            ORDER BY Datum";
        cmd.Parameters.AddWithValue("$reiseId", reiseId);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new Programmpunkt
            {
                Id = reader.GetInt32(0),
                Titel = reader.GetString(1),
                Datum = reader.GetString(2),
                Kategorie = reader.GetString(3),
                Kosten = reader.GetDecimal(4),
                Erledigt = reader.GetInt32(5) == 1,
                ReiseId = reader.GetInt32(6)
            });
        }
        return list;
    }

    public void Add(Programmpunkt punkt)
    {
        using var con = new SqliteConnection(DbInitializer.ConnectionString);
        con.Open();

        var cmd = con.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Programmpunkte (Titel, Datum, Kategorie, Kosten, Erledigt, ReiseId)
            VALUES ($titel, $datum, $kategorie, $kosten, 0, $reiseId)";
        cmd.Parameters.AddWithValue("$titel", punkt.Titel);
        cmd.Parameters.AddWithValue("$datum", punkt.Datum);
        cmd.Parameters.AddWithValue("$kategorie", punkt.Kategorie);
        cmd.Parameters.AddWithValue("$kosten", punkt.Kosten);
        cmd.Parameters.AddWithValue("$reiseId", punkt.ReiseId);
        cmd.ExecuteNonQuery();
    }

    public void SetErledigt(int id, bool erledigt)
    {
        using var con = new SqliteConnection(DbInitializer.ConnectionString);
        con.Open();

        var cmd = con.CreateCommand();
        cmd.CommandText = "UPDATE Programmpunkte SET Erledigt = $val WHERE Id = $id";
        cmd.Parameters.AddWithValue("$val", erledigt ? 1 : 0);
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var con = new SqliteConnection(DbInitializer.ConnectionString);
        con.Open();

        var cmd = con.CreateCommand();
        cmd.CommandText = "DELETE FROM Programmpunkte WHERE Id = $id";
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
    }
}
