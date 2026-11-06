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

## V-A Security und Authentication

| Datum | Prompt (kurz) | Nützlich | Korrigiert / falsch |
|---|---|---|---|
| 06.11. | V-A umsetzen (Unterlage angehängt, direkt mergen) | Zuerst Entwurf statt Code: Rollen und Rechte als Tabelle, Prüfung im Service, erster Benutzer wird Administrator (kein Standardkonto), Passwort-Hashing mit PBKDF2. Hash-Verfahren am aktuellen OWASP Password Storage Cheat Sheet nachgeprüft statt aus dem Gedächtnis übernommen | Veraltetes (MD5, SHA-1, wenige Iterationen) wurde nicht vorgeschlagen. Die Zahl 600 000 Iterationen war nach der Prüfung gegen die Quelle richtig |
| 06.11. | Passwort-Hashing und Rollen mit Tests | `PasswortHasher`, `Berechtigungen`, `BenutzerSitzung`, 25 Tests. Gegenproben (Recht aufgeweicht, Salt konstant): Tests schlugen fehl | – |
| 06.11. | Benutzer speichern und anmelden | `Benutzer`-Entity, Migration, `AuthService`, Login-Dialog, Benutzer anlegen, 18 Tests. Gleiche Meldung bei falschem Namen und falschem Passwort | – |
| 06.11. | Rechte in den Services erzwingen | Prüfung in jeder Service-Methode, 38 bestehende Tests mit Administrator-Sitzung angepasst, 10 neue Tests. Gegenproben (Prüfung entfernt): Tests schlugen fehl | – |
| 06.11. | Ende-zu-Ende-Test der Oberfläche | Skript steuerte die App wie ein Benutzer (34 Prüfungen: Ersteinrichtung, falsches Passwort, alle drei Rollen, Abmelden) und suchte die Passwörter in der Datenbankdatei: nicht gefunden | Zwei Fehler im Testskript, nicht in der App: `Pwd` ist in PowerShell ein Alias (alle "zufälligen" Passwörter waren gleich), und Dialoge mit Owner stehen im UI-Baum unter dem Hauptfenster. Ein echter Fund: Das Fenster war breiter als der Laptop-Bildschirm, "Abmelden" lag ausserhalb. Die Fenstergrösse wird jetzt begrenzt |
| 06.11. | Branch, PR #7, App starten und Administrator anlegen | 7 Commits in sinnvollen Schritten, Reviewer jtr120826 eingetragen, auf Wunsch direkt gemergt ([PR #7](https://github.com/jtr120826/voyage/pull/7)). Danach die App geöffnet, du hast den ersten Administrator selbst angelegt | – |
| 06.11. | Warum muss das Passwort mindestens 10 Zeichen haben? | Die KI erklärte ehrlich: Die Unterlage nennt keine Mindestlänge, die 10 war ihre eigene Annahme (Länge schützt mehr als Zeichenregeln), nicht nachgeschlagen. Auf deinen Wunsch auf 8 gesenkt, eine Konstante (`AuthService.MindestlaengePasswort`), Tests blieben grün ([PR #8](https://github.com/jtr120826/voyage/pull/8)) | Die 10 war eine unbelegte Annahme der KI, die Entscheidung für 8 triffst du. Die Mindestlänge ist ein Kompromiss zwischen Sicherheit und Bedienbarkeit, keine feste Vorgabe |

**Reflexion (Entwurf):**
- **Hash-Verfahren und Grund:** PBKDF2-HMAC-SHA256, 600 000 Iterationen, Salt pro Passwort. Argon2id wäre laut OWASP die erste Wahl, braucht aber ein Fremdpaket. PBKDF2 ist in .NET eingebaut. Die Iterationszahl wurde gegen die aktuelle Quelle geprüft, denn Empfehlungen ändern sich (früher galten deutlich weniger Iterationen als ausreichend).
- **Hat die KI etwas Veraltetes vorgeschlagen?** Nein, nicht in dieser Runde. Dass wir es geprüft haben, ist trotzdem der wichtige Teil.
- **Wo hätte in K0–K5 schon eine Berechtigungsprüfung hingehört?** Überall dort, wo Daten verändert werden: `Hinzufuegen`, `Aktualisieren`, `Loeschen` und `ErledigtUmschalten` in beiden Services, ab den ersten Schreibzugriffen (K1/K2). Auch die Bearbeiten-Funktion (PR #5) war ohne Prüfung für jeden offen. Eine Prüfung nur in der Oberfläche hätte nicht gereicht, weil die Services ohne Klick aufrufbar sind (siehe `ServiceBerechtigungTests`).
- **Offen gelassen (ehrlich):** Keine Sperre nach Fehlversuchen, unverschlüsselte Datenbankdatei, Daten gehören nicht einzelnen Benutzern. Steht so im README.

## K5 Git und Kollaboration

| Datum | Prompt (kurz) | Nützlich | Korrigiert / falsch |
|---|---|---|---|
| 06.11. | K5-Unterlage angehängt: Aufgaben B1 bis B4 umsetzen | Die KI las die Folien und prüfte zuerst den Ist-Zustand, bevor sie etwas änderte: `git log --graph` zeigte K1 bis K4 schon als Branches mit Merge-Commits. Ein rückwirkendes Umbenennen wurde bewusst nicht gemacht, weil es den Verlauf auf dem Server hätte umschreiben müssen | – |
| 06.11. | .gitignore und Secrets prüfen (Prompt 3 der Folien, selbst ausgeführt) | Gesamte Historie durchsucht (Dateinamen und alle hinzugefügten Zeilen): kein Secret, keine Datenbank, nur `Data Source=app.db`. `.gitignore` nur ergänzt (IDE, Testergebnisse, SQLite, `appsettings.json`, Schlüsseldateien), nichts Bestehendes geändert | Der erste Suchbefehl scheiterte an einem ungültigen regulären Ausdruck (ein Fehler im Werkzeug, kein Fund). Mit einfacherem Muster wiederholt |
| 06.11. | Merge-Konflikt bewusst provozieren und lösen (Prompt 1 der Folien) | Dieselbe README-Zeile auf zwei Branches geändert. Git zeigte `<<<<<<< HEAD` (Version aus `main`) gegen `feature/k5-konflikt-b`. Erst verstanden, was sich gegenübersteht, dann entschieden: beide Änderungen kombiniert, weil sie sich nicht ausschliessen | – |
| 06.11. | Pull-Request-Text (Prompt 2 der Folien) | Beschreibung nach dem Schema Was, Warum, Worauf achten, mit konkreter Frage an den Reviewer (passt `*.db` in der `.gitignore`?) | – |
| 06.11. | Pull Request öffnen und mergen | PR #9 mit Beschreibung (Was, Warum, Worauf achten) und Reviewer jtr120826, auf deinen Wunsch direkt gemergt ([PR #9](https://github.com/jtr120826/voyage/pull/9)). Tests auf `main` danach grün (91) | Wieder ohne Review des Partners (abwesend). Die KI hat bewusst keinen Review-Vermerk erfunden |

**Reflexion (Entwurf):**
- **Hat uns die KI beim Git-Verlauf geholfen oder behindert?** Geholfen beim Prüfen: Die Suche über die ganze Historie wäre von Hand mühsam und lückenhaft gewesen. Gebremst hat sie sich selbst, indem sie nichts umgeschrieben hat, was nicht nötig war.
- **Was bleibt bei uns?** Die Entscheidung im Konflikt (welche Version, warum) und die Prüfung, ob ein Review wirklich stattgefunden hat. Die KI kann einen Pull Request beschreiben, aber nicht für den Partner prüfen.
- **Ehrliche Lücke:** PR #1 bis #8 wurden ohne Review des Partners gemerged (er war abwesend). Das ist im README so festgehalten und nicht nachträglich beschönigt.
- **Rückblick:** Hätten wir von Anfang an Commit-Nachrichten wie "K2: ..." und Branches nach `feature/<thema>` benutzt, wäre B1 überflüssig gewesen. Das haben wir ab K1 gemacht.
