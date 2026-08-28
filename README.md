# Reiseplaner

WPF-Desktop-App zur Planung von Reisen mit Programmpunkten und Budget-Überwachung.
Entstanden im Modul PROG1 (gibb) als laufendes Projekt über K0–K1.

## Funktionen

- **Reisen verwalten**: anlegen, löschen (Titel, Zielort, Start-/Enddatum, Budget)
- **Programmpunkte** pro Reise: anlegen, löschen, als *erledigt* markieren (Titel, Datum, Kategorie, Kosten)
- **Budget-Übersicht**: Geplantes Budget (Summe der Kosten) und Verbleibendes Budget werden direkt in der Reisen-Liste angezeigt und live aktualisiert; Warnung bei Überschreitung
- **Eingabefelder** mit Platzhaltertext (z. B. "Reisetitel"), der beim Tippen verschwindet

## Datenmodell

- `Reise` (1) → `Programmpunkt` (N), verknüpft über `ReiseId`
- Speicherung lokal in SQLite (`app.db`, nicht im Repo – wird beim ersten Start automatisch erzeugt)

## Architektur

Schichtenarchitektur (seit K1):

```
MainWindow.xaml.cs (UI)  →  Services/  (Business-Logik)  →  DataAccess/  (SQL/ADO.NET)
```

- `Services/`: `ReiseService`, `ProgrammpunktService` – u. a. Erledigt-Umschalten-Entscheidung und Budget-Berechnung
- `DataAccess/`: `ReiseDataAccess`, `ProgrammpunktDataAccess`, `DbInitializer` – reiner SQL-Zugriff via `Microsoft.Data.Sqlite`, kein ORM
- Kein MVVM, kein Repository-Pattern/Interfaces – kommt planmässig erst in K3

## Stand

| Einheit | Status |
|---|---|
| K0 – KI-gestützte App-Erstellung | ✅ abgeschlossen |
| K1 – Schichtenarchitektur | ✅ abgeschlossen (PR #1 gemerged) |
| K2–K5 | offen |

## Tech-Stack

.NET 10, WPF, Microsoft.Data.Sqlite, SQLite
