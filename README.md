# Reiseplaner

WPF-Desktop-App zur Planung von Reisen mit Programmpunkten und Budget-Überwachung.
Entstanden im Modul PROG1 (gibb) als laufendes Projekt über K0–K4.

## Funktionen

- **Reisen verwalten**: anlegen, bearbeiten, löschen (Titel, Zielort, Start-/Enddatum, Budget)
- **Programmpunkte** pro Reise: anlegen, bearbeiten, löschen, als *erledigt* markieren (Titel, Datum, Kategorie, Kosten)
- **Budget-Übersicht**: Geplantes und Verbleibendes Budget stehen direkt in der Reisen-Liste und werden live aktualisiert; Warnung bei Überschreitung
- **Eingabefelder** mit Platzhaltertext (z. B. "Reisetitel"), der beim Tippen verschwindet

## Projektstruktur

```
Reiseplaner.sln
├── Reiseplaner/          WPF-App (.NET 10)
└── Reiseplaner.Tests/    xUnit-Tests (28 Tests)
```

Starten: `Reiseplaner.sln` in Visual Studio öffnen, Startprojekt `Reiseplaner`. Tests: Test-Explorer oder `dotnet test`.

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
- `Models/`: Entity-Klassen und `Kategorien`, `Migrations/`: EF-Core-Migrationen
- `App.xaml.cs`: erzeugt die Repositories und übergibt sie per Konstruktor an die Services (Dependency Injection); die Services kennen nur Interfaces

### Wer ruft wen?

| Service | Methode | Aufruf im Repository |
|---|---|---|
| ReiseService | `LadeAlle()` | `IRepository<Reise>.GetAll()` |
| ReiseService | `Hinzufuegen(dto)` | `Add(reise)` |
| ReiseService | `Aktualisieren(dto)` | `GetById(id)` und `Update(reise)` |
| ReiseService | `Loeschen(id)` | `Delete(id)` |
| ProgrammpunktService | `LadeProgrammpunkte(reiseId)` | `IProgrammpunktRepository.GetByReise(reiseId)` |
| ProgrammpunktService | `Hinzufuegen(reiseId, dto)` | `Add(programmpunkt)` |
| ProgrammpunktService | `Aktualisieren(dto)` | `GetById(id)` und `Update(programmpunkt)` |
| ProgrammpunktService | `ErledigtUmschalten(id)` | `GetById(id)` und `Update(programmpunkt)` |
| ProgrammpunktService | `Loeschen(id)` | `Delete(id)` |

### Entscheide K3

- **`IRepository<T>` reicht nicht für alles:** Programmpunkte brauchen `GetByReise()`, deshalb erweitert `IProgrammpunktRepository` das generische Interface.
- **Generische Basisklasse:** Die fünf Standardmethoden stehen einmal in `Repository<T>`. `ProgrammpunktRepository` besteht nur noch aus einer Methode (15 statt 42 Zeilen, obwohl `GetById` und `Update` neu dazukamen).
- **`ReiseDto` lohnt sich klar:** Es trägt berechnete Werte (geplant, verbleibend, überschritten), versteckt die Programmpunkt-Collection und ersetzt die `[NotMapped]`-Felder in der Entity.
- **`ProgrammpunktDto` ist fast eine 1:1-Kopie:** Weggelassen sind der Fremdschlüssel `ReiseId` und die Navigation Property `Reise`. Der Nutzen liegt vor allem darin, dass die UI keine Entities mehr kennt.

## Qualität (K4)

**Magic Strings ersetzt:** Connection String (`AppDbContext`), Datumsformat `"yyyy-MM-dd"` (3x im Code-Behind), die vier Kategorien (vorher im XAML, jetzt `Kategorien.Alle`), Meldungstexte (2 davon doppelt).

**Naming:** Service-Methoden sind einheitlich deutsch (`GetAlle` wurde zu `LadeAlle`, `GetByReise` zu `LadeProgrammpunkte`). Die englischen Namen in `IRepository<T>` bleiben bewusst, weil das ein bekanntes Standardmuster ist.

**Single Responsibility:** Die Klick-Handler im `MainWindow` haben das Formular geprüft, das DTO gebaut, den Service aufgerufen und das Formular geleert, alles in einer Methode. Jede dieser Aufgaben hat jetzt eine eigene Methode (`IstReiseFormularVollstaendig`, `LeseReiseAusFormular`, `LeereReiseFormular`), der Handler steuert nur noch den Ablauf. So lässt sich jede Aufgabe einzeln lesen und ändern, etwa wenn ein Formularfeld dazukommt. Die else-if-Kette für die Platzhalter ist durch ein `Dictionary` ersetzt.

**Tests:** 28 xUnit-Tests für beide Services, ohne Datenbank dank Fake-Repositories (`Reiseplaner.Tests/Fakes`). Darunter Positivtests, Negativtests (nicht existierende Id) und Grenzfälle (Kosten genau am Budget). Das wäre ohne die Interfaces aus K3 nicht möglich gewesen: Die Services erzeugten ihr Repository früher selbst und hätten immer an der echten Datenbank gehangen.

## Stand

| Einheit | Status |
|---|---|
| K0 – KI-gestützte App-Erstellung | abgeschlossen |
| K1 – Schichtenarchitektur | abgeschlossen (PR #1) |
| K2 – ORM / EF Core | abgeschlossen (PR #2) |
| K3 – Interfaces, Repository, DTOs | abgeschlossen (PR #3) |
| K4 – Clean Code und Unit Tests | abgeschlossen (PR #4) |
| Bearbeiten-Funktion (vollständiges CRUD) | abgeschlossen |
| K5 | offen |

## Tech-Stack

.NET 10, WPF, Entity Framework Core (SQLite-Provider), SQLite, xUnit
