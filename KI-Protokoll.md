# KI-Protokoll Reiseplaner (PROG1 HS26)

**Tool:** Claude Code (Claude Sonnet 5.x), Desktop-App. Kein GitHub Copilot im Einsatz.
**Hinweis:** Der Code wurde fast vollständig von der KI geschrieben. Verstehen und erklären müssen wir ihn selbst (LB2, LB3).
**Status:** Entwurf, aus dem Chat-Verlauf zusammengefasst. Reflexionsantworten bitte mit eigener Erfahrung prüfen und ergänzen.

## K0 – App erstellen

| Datum | Prompt (kurz) | Nützlich | Korrigiert / falsch |
|---|---|---|---|
| 14.08. | Alle Dateien und ZIPs im Ordner analysieren, Auftrag aufzeigen | Überblick über K0–K6, LB1–LB3, Bewertungsraster | – |
| 14.08. | Gute, praktisch umsetzbare App-Ideen; Lebensmittel retten oder Reiseplaner? | Ideenliste mit Entitäten, 1:N und Workflow; Entscheid für Reiseplaner (Reise → Programmpunkt, Budget) | Der Entscheid lag bei uns, die KI hat nur Optionen aufbereitet |
| 14.08. | Wie starten wir? Projekt direkt erstellen | Angepasster Starter-Prompt, komplettes WPF-Projekt (Code-Behind, ADO.NET, SQLite) | Ohne .NET SDK nie gebaut oder getestet, also ungeprüft geliefert |
| 28.08. | App lokal starten | KI installierte das SDK, baute und bediente die App per UI-Automation | Build-Fehler: `ImplicitUsings` fehlte. Sicherheitswarnung bei SQLitePCLRaw, auf neuere Version gepinnt |
| 14.08. | Git init und Push ins Repo | Repo eingerichtet, Code gepusht | Push zuerst 403: Berechtigung fehlte, der Partner musste uns als Collaborator eintragen |
| 28.08. | Spalten "Geplantes" und "Verbleibendes Budget" in der Reisen-Liste | SQL-Summe pro Reise, Live-Aktualisierung | Anzeige in `$` statt CHF (WPF-Bindings nutzen en-US), danach Kultur gesetzt |
| 28.08. | Platzhaltertext in den Eingabefeldern wie beim Datum | Platzhalter als Overlay über der TextBox | Die KI vermutete einen Layout-Fehler durch ihr erstes Template und wechselte den Ansatz. Die Ursache war aber ein Messfehler ihrer Screenshots (DPI-Skalierung), kein App-Fehler |
| 28.08. | Kurze Doku | README | – |

**Reflexion (Entwurf):** Stärke: In kurzer Zeit eine lauffähige App, und die KI konnte sie selbst bedienen und prüfen. Grenze: Die erste Lieferung war ungebaut, die Fehler zeigten sich erst beim Kompilieren. Diagnosen der KI können falsch sein, auch ihre Messwerkzeuge.

## K1 – Schichtenarchitektur

| Datum | Prompt (kurz) | Nützlich | Korrigiert / falsch |
|---|---|---|---|
| 28.08. | Weiter mit K1 (Unterlagen als Vorlage) | Umbau zu UI → Service → DataAccess. Erledigt-Umschalten und Budget-Berechnung vom Code-Behind in den Service. Kontrollfrage "kein `Sqlite` im Code-Behind" bestanden | Der K0-Prompt gab schon "Repository"-Klassen vor, K1 will `DataAccess`: umbenannt (Vorgriff, K3 benennt zurück) |
| 28.08. | Branch, Pull Request an den Partner | Branch `feature/k1-schichten`, 2 Commits, PR #1 | Das `gh`-Token hatte kein `read:org`: Reviewer über REST-API gesetzt. Auf unseren Wunsch ohne Review gemergt (Partner abwesend) |
| 28.08. | Wie funktionieren Branches, sind das zwei Programme? Darf ich mergen? | Erklärung: Branch = Zeiger auf einen Stand, nicht eine Kopie | – |

**Reflexion (Entwurf):** Einfacher: Die UI enthält kein SQL mehr, Logik ist auffindbar. Komplizierter: mehr Klassen und Dateien für dieselbe Funktion.

## K2 – ORM / EF Core

| Datum | Prompt (kurz) | Nützlich | Korrigiert / falsch |
|---|---|---|---|
| 18.09. | App nach den K2-Folien auf EF Core umschreiben (PDF und PPTX angehängt) | Pakete, `AppDbContext`, Entities mit Annotations und Navigation Properties, Migration, DataAccess auf LINQ | Fehlendes `using` für `Migrate()`. `decimal` wird in SQLite als TEXT statt REAL gespeichert (geprüft, so gewollt) |
| 18.09. | Test der umgebauten App | Gebaut und per UI-Automation geprüft: Verhalten wie vorher. Kontrollfrage "kein `SqliteConnection`" bestanden | Alte `app.db` mit Alt-Schema gelöscht und neu per Migration erzeugt. SQLite-Zusatzdateien (`-wal`, `-shm`) mussten in `.gitignore` |
| 18.09. | Commits, PR #2, Merge | 4 Commits in sinnvollen Schritten | – |

**Reflexion (Entwurf):** Einfacher: kein SQL-String und kein manuelles Mapping mehr. Ungewohnt: Migrations, und dass die Budget-Summe jetzt in C# über `Include()` statt per SQL-JOIN entsteht.

## K3 – Interfaces, Repository, DTOs

| Datum | Prompt (kurz) | Nützlich | Korrigiert / falsch |
|---|---|---|---|
| 06.11. | K3 umsetzen | `IRepository<T>`, `IProgrammpunktRepository`, generische Basisklasse, Konstruktor-Injection, Composition in `App.xaml.cs` | `IRepository<T>` reichte nicht für `GetByReise` → eigenes Interface |
| 06.11. | (Teil von K3) DTOs einführen | `ReiseDto` und `ProgrammpunktDto` mit manuellem Mapping, UI kennt keine Entities mehr, `BudgetStatus` wurde überflüssig | Das Testskript der KI löschte die falsche Zeile (Sortierung nach Datum), Testdaten wurden zurückgesetzt |
| 06.11. | Wie stehen wir im Projekt? | Abgleich mit Semesterplan und LB1-Raster: Rückstand bei K3/K4, fehlendes KI-Protokoll, fehlende Update-Funktion | – |
| 06.11. | Branch, PR #3 | 3 Commits, gemergt | – |

**Reflexion (Entwurf):** AutoMapper wurde nicht vorgeschlagen, weil die Vorgabe (manuelles Mapping) im Auftrag stand. DTO: `ReiseDto` lohnt sich klar (berechnete Werte, versteckt die Collection), `ProgrammpunktDto` ist fast eine 1:1-Kopie.

## K4 – Clean Code und Unit Tests

| Datum | Prompt (kurz) | Nützlich | Korrigiert / falsch |
|---|---|---|---|
| 06.11. | K4 umsetzen: Code-Review | Befunde: Magic Strings (Connection String, Datumsformat, Kategorien, Meldungen), Sprachmix bei Methodennamen, else-if-Kette, lange Klick-Handler. Alles behoben | – |
| 06.11. | Testprojekt einrichten | Solution angelegt (gab es nicht), xUnit-Projekt, Fake-Repositories, 20 Tests (Positiv, Negativ, Grenzfälle) | Ein geplanter Test (DTO-Id wird ignoriert) verworfen: Er wäre mit dem Fake immer grün gewesen |
| 06.11. | (Teil von K4) Tests prüfen | Gegenproben: absichtlich Fehler im Service eingebaut (`< 0` zu `<= 0`, `Update()` entfernt), die Tests schlugen fehl | – |
| 06.11. | Branch, PR #4 | Frischer Checkout geprüft (20/20 grün, Start mit leerer DB), gemergt | – |

**Reflexion (Entwurf):** Fachlich falsche Testfälle gab es keine, aber einen wertlosen (siehe oben). Erkannt haben wir das, weil wir uns gefragt haben, ob der Test überhaupt fehlschlagen kann. Am meisten verändert hat sich das Vertrauen: Nach jeder Änderung zeigen 20 Tests sofort, ob etwas kaputtgegangen ist.

## Weiterentwicklung nach K4

| Datum | Prompt (kurz) | Nützlich | Korrigiert / falsch |
|---|---|---|---|
| 06.11. | Bearbeiten-Funktion für Reisen und Programmpunkte | `Aktualisieren()` in beiden Services (gemeinsames `UebernehmeWerte`, kein doppelter Code), Bearbeitungsmodus im Formular, 8 neue Tests (28 insgesamt). Das schliesst das fehlende "U" im CRUD | Der Reise-Fake gab beim Laden dieselbe Instanz zurück, ein vergessener `Update()`-Aufruf wäre nicht aufgefallen: Fake angepasst (Kopie wie die Datenbank), danach Gegenproben (Erledigt-Status überschreiben, `Update()` entfernen) |

## V-D Design Patterns (Observer)

| Datum | Prompt (kurz) | Nützlich | Korrigiert / falsch |
|---|---|---|---|
| 06.11. | Wir nehmen V-A und V-D (Unterlage angehängt) | Die KI prüfte zuerst, ob ein Pattern ein echtes Problem löst, und zählte die Stellen: 7 manuelle Neuladen-Aufrufe im `MainWindow`. Entscheid: Observer | Ihr erster Vorschlag (Observer plus Strategy für eine Sortierung) war nach dem Lesen der Unterlage nicht haltbar: Strategy hätte erst ein künstliches Problem gebraucht. Verworfen |
| 06.11. | Observer umsetzen | `DatenGeaendert`-Event in beiden Services, `MainWindow` abonniert es, 10 neue Tests (38 insgesamt), Ende-zu-Ende-Test der App | Das Testskript der KI scheiterte zuerst: Die kopierte Datenbank war leer, weil die WAL-Datei fehlte, und die Programmpunkt-Auswahl geht nach dem Neuladen verloren. Fehler im Testwerkzeug, nicht in der App |

**Reflexion (Entwurf):**
- **Gewähltes Pattern und Grund:** Observer, weil 7 manuelle Neuladen-Aufrufe vergessen werden können. Ehrlich: Es gibt heute nur einen Abonnenten, der Gewinn ist eher Wartbarkeit als Entkopplung.
- **Vorschlag der KI nicht übernommen:** Strategy (Sortierung) und Factory Method. Es gab kein echtes Problem dafür, das Pattern wäre Selbstzweck gewesen.
- **Rückblick:** Schon in K1 und K2 hätte Observer geholfen, als nach jedem Service-Aufruf von Hand neu geladen wurde.
