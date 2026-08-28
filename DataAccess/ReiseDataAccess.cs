using Microsoft.Data.Sqlite;
using Reiseplaner.Models;

namespace Reiseplaner.DataAccess;

public class ReiseDataAccess
{
    public List<Reise> GetAll()
    {
        var list = new List<Reise>();
        using var con = new SqliteConnection(DbInitializer.ConnectionString);
        con.Open();

        var cmd = con.CreateCommand();
        cmd.CommandText = @"
            SELECT r.Id, r.Titel, r.Zielort, r.Startdatum, r.Enddatum, r.Budget,
                   COALESCE(SUM(p.Kosten), 0) AS GeplantesBudget
            FROM Reisen r
            LEFT JOIN Programmpunkte p ON p.ReiseId = r.Id
            GROUP BY r.Id, r.Titel, r.Zielort, r.Startdatum, r.Enddatum, r.Budget
            ORDER BY r.Startdatum";

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new Reise
            {
                Id = reader.GetInt32(0),
                Titel = reader.GetString(1),
                Zielort = reader.GetString(2),
                Startdatum = reader.GetString(3),
                Enddatum = reader.GetString(4),
                Budget = reader.GetDecimal(5),
                GeplantesBudget = reader.GetDecimal(6)
            });
        }
        return list;
    }

    public void Add(Reise reise)
    {
        using var con = new SqliteConnection(DbInitializer.ConnectionString);
        con.Open();

        var cmd = con.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Reisen (Titel, Zielort, Startdatum, Enddatum, Budget)
            VALUES ($titel, $zielort, $start, $ende, $budget)";
        cmd.Parameters.AddWithValue("$titel", reise.Titel);
        cmd.Parameters.AddWithValue("$zielort", reise.Zielort);
        cmd.Parameters.AddWithValue("$start", reise.Startdatum);
        cmd.Parameters.AddWithValue("$ende", reise.Enddatum);
        cmd.Parameters.AddWithValue("$budget", reise.Budget);
        cmd.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var con = new SqliteConnection(DbInitializer.ConnectionString);
        con.Open();

        // Abhängige Programmpunkte zuerst entfernen (SQLite erzwingt FK ohne PRAGMA nicht automatisch)
        var cmdChildren = con.CreateCommand();
        cmdChildren.CommandText = "DELETE FROM Programmpunkte WHERE ReiseId = $id";
        cmdChildren.Parameters.AddWithValue("$id", id);
        cmdChildren.ExecuteNonQuery();

        var cmd = con.CreateCommand();
        cmd.CommandText = "DELETE FROM Reisen WHERE Id = $id";
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
    }
}
