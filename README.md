# Reiseplaner

WPF-Desktop-App zur Planung von Reisen mit Programmpunkten und Budget-Überwachung.
Entstanden im Modul PROG1 (gibb) als laufendes Projekt über K0–K3.

## Funktionen

- **Reisen verwalten**: anlegen, löschen (Titel, Zielort, Start-/Enddatum, Budget)
- **Programmpunkte** pro Reise: anlegen, löschen, als *erledigt* markieren (Titel, Datum, Kategorie, Kosten)
- **Budget-Übersicht**: Geplantes und Verbleibendes Budget stehen direkt in der Reisen-Liste und werden live aktualisiert; Warnung bei Überschreitung
- **Eingabefelder** mit Platzhaltertext (z. B. "Reisetitel"), der beim Tippen verschwindet

## Datenmodell

- `Reise` (1) → `Programmpunkt` (N) als EF-Core-Navigation-Property, verknüpft über `ReiseId`
- Speicherung lokal in SQLite (`app.db`, nicht im Repo – wird beim ersten Start per EF-Core-Migration erzeugt)

## Architektur

```
MainWindow (UI)  →  Services  →  Repositories  →  EF Core / SQLite
      ↑ DTOs ↑          (Mapping Entity ↔ DTO)
```

- `Dtos/`: `ReiseDto`, `ProgrammpunktDto` – die UI sieht nur DTOs, keine Entities
- `Services/`: `ReiseService`, `ProgrammpunktService` – Business-Logik (Budget-Berechnung, Erledigt-Umschalten) und Mapping
- `DataAccess/`: `IRepository<T>`, `IProgrammpunktRepository`, generische Basisklasse `Repository<T>`, `ReiseRepository`, `ProgrammpunktRepository`, `AppDbContext`
- `Models/`: Entity-Klassen, `Migrations/`: EF-Core-Migrationen
- `App.xaml.cs`: erzeugt die Repositories und übergibt sie per Konstruktor an die Services (Dependency Injection); die Services kennen nur Interfaces

### Wer ruft wen?

| Service | Methode | Aufruf im Repository |
|---|---|---|
| ReiseService | `GetAlle()` | `IRepository<Reise>.GetAll()` |
| ReiseService | `Hinzufuegen(dto)` | `Add(reise)` |
| ReiseService | `Loeschen(id)` | `Delete(id)` |
| ProgrammpunktService | `GetByReise(reiseId)` | `IProgrammpunktRepository.GetByReise(reiseId)` |
| ProgrammpunktService | `Hinzufuegen(reiseId, dto)` | `Add(programmpunkt)` |
| ProgrammpunktService | `ErledigtUmschalten(id)` | `GetById(id)` und `Update(programmpunkt)` |
| ProgrammpunktService | `Loeschen(id)` | `Delete(id)` |

### Entscheide K3

- **`IRepository<T>` reicht nicht für alles:** Programmpunkte brauchen `GetByReise()`, deshalb erweitert `IProgrammpunktRepository` das generische Interface.
- **Generische Basisklasse:** Die fünf Standardmethoden stehen einmal in `Repository<T>`. `ProgrammpunktRepository` besteht nur noch aus einer Methode (15 statt 42 Zeilen, obwohl `GetById` und `Update` neu dazukamen).
- **`ReiseDto` lohnt sich klar:** Es trägt berechnete Werte (geplant, verbleibend, überschritten), versteckt die Programmpunkt-Collection und ersetzt die `[NotMapped]`-Felder in der Entity.
- **`ProgrammpunktDto` ist fast eine 1:1-Kopie:** Weggelassen sind der Fremdschlüssel `ReiseId` und die Navigation Property `Reise`. Der Nutzen liegt vor allem darin, dass die UI keine Entities mehr kennt.

## Stand

| Einheit | Status |
|---|---|
| K0 – KI-gestützte App-Erstellung | abgeschlossen |
| K1 – Schichtenarchitektur | abgeschlossen (PR #1) |
| K2 – ORM / EF Core | abgeschlossen (PR #2) |
| K3 – Interfaces, Repository, DTOs | abgeschlossen |
| K4–K5 | offen |

## Tech-Stack

.NET 10, WPF, Entity Framework Core (SQLite-Provider), SQLite
