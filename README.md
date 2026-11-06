# Reiseplaner

Desktop-App (WPF, .NET 10) zur Planung von Reisen mit Programmpunkten und Budget-Überwachung (lokal mit SQLite).
Entstanden im Modul PROG1 (gibb) als laufendes Projekt über K0–K4 und die Vertiefungen V-A und V-D.

## Funktionen

- **Reisen verwalten**: anlegen, bearbeiten, löschen (Titel, Zielort, Start-/Enddatum, Budget)
- **Programmpunkte** pro Reise: anlegen, bearbeiten, löschen, als *erledigt* markieren (Titel, Datum, Kategorie, Kosten)
- **Budget-Übersicht**: Geplantes und Verbleibendes Budget stehen direkt in der Reisen-Liste und werden live aktualisiert; Warnung bei Überschreitung
- **Anmeldung mit Rollen**: Leser, Sachbearbeiter, Administrator (siehe V-A)
- **Eingabefelder** mit Platzhaltertext (z. B. "Reisetitel"), der beim Tippen verschwindet

## Projektstruktur

```
Reiseplaner.sln
├── Reiseplaner/          WPF-App (.NET 10)
└── Reiseplaner.Tests/    xUnit-Tests (91 Tests)
```

Starten: `Reiseplaner.sln` in Visual Studio öffnen, Startprojekt `Reiseplaner`. Tests: Test-Explorer oder `dotnet test`.

## Datenmodell

- `Reise` (1) → `Programmpunkt` (N) als EF-Core-Navigation-Property, verknüpft über `ReiseId`
- `Benutzer` (Benutzername eindeutig, Passwort-Hash, Salt, Iterationen, Rolle), ohne Verknüpfung zu Reisen
- Speicherung lokal in SQLite (`app.db`, nicht im Repo – wird beim ersten Start per EF-Core-Migration erzeugt)

## Architektur

```
MainWindow (UI)  →  Services  →  Repositories  →  EF Core / SQLite
      ↑ DTOs ↑          (Mapping Entity ↔ DTO)
```

- `Dtos/`: `ReiseDto`, `ProgrammpunktDto`, `BenutzerDto` – die UI sieht nur DTOs, keine Entities
- `Services/`: `ReiseService`, `ProgrammpunktService` – Business-Logik (Budget-Berechnung, Erledigt-Umschalten), Mapping und Berechtigungsprüfung; `AuthService`, `PasswortHasher`, `BenutzerSitzung`, `Berechtigungen` (V-A)
- `DataAccess/`: `IRepository<T>`, `IProgrammpunktRepository`, `IBenutzerRepository`, generische Basisklasse `Repository<T>`, `ReiseRepository`, `ProgrammpunktRepository`, `BenutzerRepository`, `AppDbContext`
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

**Tests:** 38 xUnit-Tests für beide Services, ohne Datenbank dank Fake-Repositories (`Reiseplaner.Tests/Fakes`). Darunter Positivtests, Negativtests (nicht existierende Id) und Grenzfälle (Kosten genau am Budget). Das wäre ohne die Interfaces aus K3 nicht möglich gewesen: Die Services erzeugten ihr Repository früher selbst und hätten immer an der echten Datenbank gehangen.

## Vertiefungen

### V-A Security und Authentication

**Anmeldung:** Beim Start erscheint ein Login (`LoginWindow`). Gibt es noch kein Konto, legt die App stattdessen den **ersten Administrator** an (Ersteinrichtung). Es gibt kein eingebautes Standardkonto und kein Standardpasswort. Weitere Konten legen Administratoren über "Benutzer anlegen" an (keine Selbstregistrierung). Passwort: mindestens 8 Zeichen, Benutzernamen sind eindeutig (ohne Gross-/Kleinschreibung).

**Passwort-Hashing:** PBKDF2-HMAC-SHA256 mit **600 000 Iterationen**, 16 Byte Zufalls-Salt pro Passwort, 32 Byte Hash (`PasswortHasher`, .NET `Rfc2898DeriveBytes.Pbkdf2`). Gespeichert werden nur Hash, Salt und Iterationszahl (Spalten in `Benutzer`), nie das Passwort. Der Vergleich läuft mit `CryptographicOperations.FixedTimeEquals` (kein Timing-Leck).
- **Warum PBKDF2 und nicht bcrypt/Argon2?** Argon2id wäre laut OWASP die erste Wahl, ist aber nicht in .NET eingebaut und bräuchte ein zusätzliches Paket. PBKDF2 ist in .NET enthalten (keine Fremdbibliothek, kein eigener Kryptocode) und bei OWASP als Option aufgeführt.
- **OWASP-Prüfung:** Die Zahl 600 000 für PBKDF2-HMAC-SHA256 wurde am Password Storage Cheat Sheet von OWASP nachgeprüft (nicht aus dem Gedächtnis der KI übernommen). Test: `StandardIterationen >= 600_000`.
- **Iterationen pro Benutzer gespeichert:** So lässt sich der Wert später erhöhen, ohne bestehende Konten zu sperren.

**Rollen und Rechte (RBAC):**

| Recht | Leser | Sachbearbeiter | Administrator |
|---|---|---|---|
| Ansehen | ja | ja | ja |
| Bearbeiten (Hinzufügen, Ändern, Erledigt) | nein | ja | ja |
| Löschen | nein | nein | ja |
| Benutzer verwalten | nein | nein | ja |

**Wo wird geprüft?** Im **Service**, nicht nur in der Oberfläche. Jede Methode von `ReiseService` und `ProgrammpunktService` ruft zuerst `IBenutzerSitzung.Pruefe(Recht)` auf und wirft sonst `UnauthorizedAccessException` (ohne Anmeldung ebenso). Die Tabelle steht an genau einer Stelle (`Berechtigungen.IstErlaubt`). Die Oberfläche sperrt Schaltflächen nur als Komfort. Beweis: `ServiceBerechtigungTests` rufen die Services direkt auf, ganz ohne Oberfläche.

**Weitere Schutzmassnahmen:**
- Falscher Benutzername und falsches Passwort geben dieselbe Meldung ("Benutzername oder Passwort ist falsch."), damit niemand Benutzernamen erraten kann
- `BenutzerDto` enthält weder Hash noch Salt, diese verlassen die Datenzugriffsschicht nie
- Keine Klartext-Passwörter in Code, Logs, Debug-Ausgaben oder Tests: Tests erzeugen ihre Passwörter zur Laufzeit zufällig (`Guid`), es steht kein Passwort im Repo; die Datenbank (`app.db`) ist per `.gitignore` ausgeschlossen
- Datenbankzugriff nur über EF Core (parametrisierte Abfragen, keine SQL-Injection)

**OWASP-Bezug:** A01 Broken Access Control (Prüfung im Service, nicht nur in der UI), A02 Cryptographic Failures (gesalzener, langsamer Hash statt Klartext), A07 Identification and Authentication Failures (Passwortlänge, kein Standardpasswort, generische Fehlermeldung), A03 Injection (EF Core).

**Bekannte Grenzen (bewusst nicht umgesetzt):**
- Keine Sperre nach mehreren Fehlversuchen (kein Brute-Force-Schutz)
- Bei unbekanntem Benutzer wird kein Dummy-Hash berechnet, die Antwortzeit verrät also theoretisch, ob es den Namen gibt
- Die SQLite-Datei ist nicht verschlüsselt: Wer die Datei hat, kann Reisedaten lesen und die Hashes angreifen. Die Rollen schützen nur die Bedienung über die App
- Kein automatisches Erneuern alter Hashes beim Login (die Iterationen sind gespeichert, der Mechanismus wäre vorbereitet)
- Kein Passwort ändern/zurücksetzen und kein Löschen von Benutzern. Daten gehören nicht einzelnen Benutzern: Die Rollen gelten für alle Reisen gleich, wie in der Aufgabe vorgegeben

**Tests:** 53 neue Tests (91 insgesamt): `PasswortHasherTests`, `BenutzerSitzungTests` (komplette Rollen-Rechte-Matrix), `AuthServiceTests`, `ServiceBerechtigungTests`. Gegenproben: Recht in `Berechtigungen` aufgeweicht, Salt konstant gemacht, Prüfungen in Services und `AuthService` entfernt, jeweils schlugen Tests fehl. Zusätzlich ein Ende-zu-Ende-Test der Oberfläche (34 Prüfungen, u. a. Ersteinrichtung, alle drei Rollen, Passwort nicht im Klartext in der Datenbankdatei).

### V-D Design Patterns: Observer

**Problem vorher:** Nach jeder Änderung musste das `MainWindow` von Hand die Liste neu laden, an 7 Stellen, und zwar in zwei uneinheitlichen Varianten (`ReisenLaden` und `ReisenAktualisierenMitAuswahl`). Wer eine neue Aktion ergänzte, konnte das Neuladen vergessen, und die Anzeige zeigte veraltete Werte.

**Lösung:** `ReiseService` und `ProgrammpunktService` lösen nach jeder Änderung das C#-Event `DatenGeaendert` aus. Das `MainWindow` abonniert es einmal (`OnDatenGeaendert`). Die Services kennen ihre Abonnenten nicht und wissen nicht, was mit der Meldung passiert.

**Alternative ohne Pattern:** Weiter nach jedem Service-Aufruf im Handler neu laden, wie vorher. Das ist solange verständlich, wie es nur ein Fenster gibt. Ehrlich eingeschätzt gibt es heute nur einen Abonnenten. Der Gewinn ist, dass das Neuladen nicht mehr vergessen werden kann und weitere Abonnenten (etwa eine Statistik oder ein Protokoll) ohne Änderung der Services dazukommen könnten.

**Nicht gewählt:** Strategy (in der App gibt es keine if/else-Kette mit gleicher Signatur) und Factory Method (keine wachsenden Objektvarianten). Dort gäbe es kein echtes Problem, ein Pattern wäre Selbstzweck.

**Tests:** 10 Tests prüfen das Event: Es kommt bei jeder Änderung, bei unbekannter Id nicht, und mehrere unabhängige Abonnenten werden alle informiert.

## Stand

| Einheit | Status |
|---|---|
| K0 – KI-gestützte App-Erstellung | abgeschlossen |
| K1 – Schichtenarchitektur | abgeschlossen (PR #1) |
| K2 – ORM / EF Core | abgeschlossen (PR #2) |
| K3 – Interfaces, Repository, DTOs | abgeschlossen (PR #3) |
| K4 – Clean Code und Unit Tests | abgeschlossen (PR #4) |
| Bearbeiten-Funktion (vollständiges CRUD) | abgeschlossen (PR #5) |
| V-D Design Patterns (Observer) | abgeschlossen (PR #6) |
| V-A Security und Authentication | abgeschlossen |
| K5 | offen |

## Tech-Stack

.NET 10, WPF, Entity Framework Core (SQLite-Provider), SQLite, xUnit, `System.Security.Cryptography` (PBKDF2)
