using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Reiseplaner.Dtos;
using Reiseplaner.Models;
using Reiseplaner.Services;

namespace Reiseplaner;

public partial class MainWindow : Window
{
    private const string DatumsFormat = "yyyy-MM-dd";
    private const string BetragsFormat = "0.##";
    private const string HinzufuegenText = "Hinzufügen";
    private const string SpeichernText = "Speichern";
    private const string NeueReiseTitel = "Neue Reise";
    private const string ReiseBearbeitenTitel = "Reise bearbeiten";
    private const string NeuerProgrammpunktTitel = "Neuer Programmpunkt";
    private const string ProgrammpunktBearbeitenTitel = "Programmpunkt bearbeiten";
    private const string KeineReiseGewaehltMeldung = "Bitte zuerst eine Reise auswählen.";
    private const string KeinProgrammpunktGewaehltMeldung = "Bitte zuerst einen Programmpunkt auswählen.";
    private const string ReiseUnvollstaendigMeldung = "Bitte Titel, Zielort, Start- und Enddatum ausfüllen.";
    private const string ProgrammpunktUnvollstaendigMeldung = "Bitte Titel, Datum und Kategorie ausfüllen.";

    private readonly ReiseService _reiseService;
    private readonly ProgrammpunktService _programmpunktService;
    private readonly AuthService _authService;
    private readonly IBenutzerSitzung _sitzung;
    private readonly Dictionary<TextBox, TextBlock> _platzhalterProTextBox;

    // Id der Reise bzw. des Programmpunkts, die gerade im Formular bearbeitet werden (null = neu anlegen).
    private int? _bearbeiteteReiseId;
    private int? _bearbeiteterProgrammpunktId;

    /// <summary>
    /// Wird ausgelöst, wenn der Benutzer auf "Abmelden" klickt. Den Ablauf danach (erneut anmelden oder beenden)
    /// übernimmt die App, dieses Fenster kennt nur die Sitzung.
    /// </summary>
    public event EventHandler? AbmeldenAngefordert;

    public MainWindow(ReiseService reiseService, ProgrammpunktService programmpunktService,
        AuthService authService, IBenutzerSitzung sitzung)
    {
        _reiseService = reiseService;
        _programmpunktService = programmpunktService;
        _authService = authService;
        _sitzung = sitzung;

        InitializeComponent();

        _platzhalterProTextBox = new Dictionary<TextBox, TextBlock>
        {
            [TitelBox] = TitelPlatzhalter,
            [ZielortBox] = ZielortPlatzhalter,
            [BudgetBox] = BudgetPlatzhalter,
            [PunktTitelBox] = PunktTitelPlatzhalter,
            [KostenBox] = KostenPlatzhalter
        };

        // Ohne diese Zeile formatieren XAML-Bindings (StringFormat) immer nach en-US statt nach der Systemkultur.
        Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);

        KategorieBox.ItemsSource = Kategorien.Alle;

        // Observer: Die Services melden Änderungen, das Fenster aktualisiert sich selbst.
        _reiseService.DatenGeaendert += OnDatenGeaendert;
        _programmpunktService.DatenGeaendert += OnDatenGeaendert;

        NachAnmeldungAktualisieren();
    }

    private ReiseDto? AktuelleReise => ReisenGrid.SelectedItem as ReiseDto;

    /// <summary>
    /// Setzt das Fenster für den aktuell angemeldeten Benutzer auf: Formulare leeren, Daten laden, Kopfzeile
    /// und Schaltflächen nach Rolle. Wird beim Start und nach jeder erneuten Anmeldung aufgerufen.
    /// </summary>
    public void NachAnmeldungAktualisieren()
    {
        BeendeReiseBearbeitung();
        BeendeProgrammpunktBearbeitung();

        // Auswahl und Daten des vorherigen Benutzers dürfen nicht stehen bleiben.
        ReisenGrid.ItemsSource = null;
        ProgrammGrid.ItemsSource = null;
        BudgetInfoText.Text = "";

        var benutzer = _sitzung.AngemeldeterBenutzer;
        BenutzerText.Text = benutzer == null ? "Nicht angemeldet" : $"Angemeldet als {benutzer.Benutzername} ({benutzer.Rolle})";
        BenutzerAnlegenButton.Visibility = _sitzung.Darf(Recht.BenutzerVerwalten) ? Visibility.Visible : Visibility.Collapsed;

        ReisenAktualisierenMitAuswahl();
    }

    /// <summary>
    /// Leert alle Daten im Fenster, solange niemand angemeldet ist.
    /// </summary>
    public void DatenAusblenden()
    {
        ReisenGrid.ItemsSource = null;
        ProgrammGrid.ItemsSource = null;
        BudgetInfoText.Text = "";
    }

    private void AbmeldenButton_Click(object sender, RoutedEventArgs e)
    {
        AbmeldenAngefordert?.Invoke(this, EventArgs.Empty);
    }

    private void BenutzerAnlegenButton_Click(object sender, RoutedEventArgs e)
    {
        new BenutzerAnlegenWindow(_authService) { Owner = this }.ShowDialog();
    }

    // Blendet den Platzhaltertext aus, sobald die zugehörige TextBox Inhalt hat, und wieder ein, wenn sie leer ist.
    private void PlatzhalterAktualisieren(object sender, TextChangedEventArgs e)
    {
        var textBox = (TextBox)sender;
        var platzhalter = _platzhalterProTextBox[textBox];
        platzhalter.Visibility = textBox.Text.Length == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void OnDatenGeaendert(object? sender, EventArgs e)
    {
        ReisenAktualisierenMitAuswahl();
    }

    // Lädt die Reisen neu (aktualisiert die Budgetwerte) und behält die Auswahl bei,
    // damit die Programmpunkt-Ansicht nach einer Änderung nicht verloren geht.
    private void ReisenAktualisierenMitAuswahl()
    {
        var aktuelleId = AktuelleReise?.Id;
        var reisen = _reiseService.LadeAlle();
        ReisenGrid.ItemsSource = reisen;
        ReisenGrid.SelectedItem = reisen.FirstOrDefault(r => r.Id == aktuelleId);
    }

    private void ReisenGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ProgrammpunkteLadenUndBudgetAnzeigen();
    }

    private void ProgrammpunkteLadenUndBudgetAnzeigen()
    {
        var reise = AktuelleReise;
        if (reise == null)
        {
            ProgrammGrid.ItemsSource = null;
            BudgetInfoText.Text = "";
            return;
        }

        ProgrammGrid.ItemsSource = _programmpunktService.LadeProgrammpunkte(reise.Id);
        BudgetInfoText.Text = ErstelleBudgetText(reise);
    }

    private static string ErstelleBudgetText(ReiseDto reise)
    {
        var anfang = $"Budget: {reise.Budget:C} | Ausgegeben: {reise.GeplantesBudget:C} | ";

        return reise.IstBudgetUeberschritten
            ? anfang + $"ÜBERSCHRITTEN um {-reise.VerbleibendesBudget:C}!"
            : anfang + $"Rest: {reise.VerbleibendesBudget:C}";
    }

    private void ReiseSpeichernButton_Click(object sender, RoutedEventArgs e)
    {
        if (!IstReiseFormularVollstaendig())
        {
            MessageBox.Show(ReiseUnvollstaendigMeldung);
            return;
        }

        if (_bearbeiteteReiseId is int reiseId)
        {
            SpeichereBearbeiteteReise(reiseId);
        }
        else
        {
            FuegeReiseHinzu();
        }
    }

    private void FuegeReiseHinzu()
    {
        _reiseService.Hinzufuegen(LeseReiseAusFormular());
        LeereReiseFormular();
    }

    private void SpeichereBearbeiteteReise(int reiseId)
    {
        var reiseDto = LeseReiseAusFormular();
        reiseDto.Id = reiseId;

        _reiseService.Aktualisieren(reiseDto);
        BeendeReiseBearbeitung();
    }

    private void ReiseBearbeitenButton_Click(object sender, RoutedEventArgs e)
    {
        var reise = AktuelleReise;
        if (reise == null)
        {
            MessageBox.Show(KeineReiseGewaehltMeldung);
            return;
        }

        FuelleReiseFormular(reise);
        _bearbeiteteReiseId = reise.Id;
        AktualisiereReiseFormularModus();
    }

    private void ReiseAbbrechenButton_Click(object sender, RoutedEventArgs e)
    {
        BeendeReiseBearbeitung();
    }

    private bool IstReiseFormularVollstaendig()
    {
        return !string.IsNullOrWhiteSpace(TitelBox.Text)
            && !string.IsNullOrWhiteSpace(ZielortBox.Text)
            && StartPicker.SelectedDate != null
            && EndePicker.SelectedDate != null;
    }

    private ReiseDto LeseReiseAusFormular()
    {
        decimal.TryParse(BudgetBox.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out var budget);

        return new ReiseDto
        {
            Titel = TitelBox.Text,
            Zielort = ZielortBox.Text,
            Startdatum = FormatiereDatum(StartPicker.SelectedDate),
            Enddatum = FormatiereDatum(EndePicker.SelectedDate),
            Budget = budget
        };
    }

    private void FuelleReiseFormular(ReiseDto reise)
    {
        TitelBox.Text = reise.Titel;
        ZielortBox.Text = reise.Zielort;
        StartPicker.SelectedDate = LeseDatum(reise.Startdatum);
        EndePicker.SelectedDate = LeseDatum(reise.Enddatum);
        BudgetBox.Text = FormatiereBetrag(reise.Budget);
    }

    private void LeereReiseFormular()
    {
        TitelBox.Clear();
        ZielortBox.Clear();
        StartPicker.SelectedDate = null;
        EndePicker.SelectedDate = null;
        BudgetBox.Clear();
    }

    private void BeendeReiseBearbeitung()
    {
        _bearbeiteteReiseId = null;
        LeereReiseFormular();
        AktualisiereReiseFormularModus();
    }

    private void AktualisiereReiseFormularModus()
    {
        var istBearbeitung = _bearbeiteteReiseId != null;

        ReiseFormularTitel.Text = istBearbeitung ? ReiseBearbeitenTitel : NeueReiseTitel;
        ReiseSpeichernButton.Content = istBearbeitung ? SpeichernText : HinzufuegenText;
        ReiseAbbrechenButton.Visibility = istBearbeitung ? Visibility.Visible : Visibility.Collapsed;
    }

    private void ReiseLoeschenButton_Click(object sender, RoutedEventArgs e)
    {
        var reise = AktuelleReise;
        if (reise == null)
        {
            MessageBox.Show(KeineReiseGewaehltMeldung);
            return;
        }

        if (_bearbeiteteReiseId == reise.Id)
        {
            BeendeReiseBearbeitung();
        }

        // Die Programmpunkte der Reise verschwinden mit ihr, eine laufende Bearbeitung ist damit hinfällig.
        BeendeProgrammpunktBearbeitung();

        _reiseService.Loeschen(reise.Id);
    }

    private void PunktSpeichernButton_Click(object sender, RoutedEventArgs e)
    {
        if (_bearbeiteterProgrammpunktId is int programmpunktId)
        {
            SpeichereBearbeitetenProgrammpunkt(programmpunktId);
        }
        else
        {
            FuegeProgrammpunktHinzu();
        }
    }

    private void FuegeProgrammpunktHinzu()
    {
        var reise = AktuelleReise;
        if (reise == null)
        {
            MessageBox.Show(KeineReiseGewaehltMeldung);
            return;
        }

        if (!IstProgrammpunktFormularVollstaendig())
        {
            MessageBox.Show(ProgrammpunktUnvollstaendigMeldung);
            return;
        }

        _programmpunktService.Hinzufuegen(reise.Id, LeseProgrammpunktAusFormular());
        LeereProgrammpunktFormular();
    }

    private void SpeichereBearbeitetenProgrammpunkt(int programmpunktId)
    {
        if (!IstProgrammpunktFormularVollstaendig())
        {
            MessageBox.Show(ProgrammpunktUnvollstaendigMeldung);
            return;
        }

        var programmpunktDto = LeseProgrammpunktAusFormular();
        programmpunktDto.Id = programmpunktId;

        _programmpunktService.Aktualisieren(programmpunktDto);
        BeendeProgrammpunktBearbeitung();
    }

    private void PunktBearbeitenButton_Click(object sender, RoutedEventArgs e)
    {
        if (ProgrammGrid.SelectedItem is not ProgrammpunktDto programmpunkt)
        {
            MessageBox.Show(KeinProgrammpunktGewaehltMeldung);
            return;
        }

        FuelleProgrammpunktFormular(programmpunkt);
        _bearbeiteterProgrammpunktId = programmpunkt.Id;
        AktualisiereProgrammpunktFormularModus();
    }

    private void PunktAbbrechenButton_Click(object sender, RoutedEventArgs e)
    {
        BeendeProgrammpunktBearbeitung();
    }

    private bool IstProgrammpunktFormularVollstaendig()
    {
        return !string.IsNullOrWhiteSpace(PunktTitelBox.Text)
            && PunktDatumPicker.SelectedDate != null
            && KategorieBox.SelectedItem != null;
    }

    private ProgrammpunktDto LeseProgrammpunktAusFormular()
    {
        decimal.TryParse(KostenBox.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out var kosten);

        return new ProgrammpunktDto
        {
            Titel = PunktTitelBox.Text,
            Datum = FormatiereDatum(PunktDatumPicker.SelectedDate),
            Kategorie = (string)KategorieBox.SelectedItem,
            Kosten = kosten
        };
    }

    private void FuelleProgrammpunktFormular(ProgrammpunktDto programmpunkt)
    {
        PunktTitelBox.Text = programmpunkt.Titel;
        PunktDatumPicker.SelectedDate = LeseDatum(programmpunkt.Datum);
        KategorieBox.SelectedItem = programmpunkt.Kategorie;
        KostenBox.Text = FormatiereBetrag(programmpunkt.Kosten);
    }

    private void LeereProgrammpunktFormular()
    {
        PunktTitelBox.Clear();
        PunktDatumPicker.SelectedDate = null;
        KategorieBox.SelectedItem = null;
        KostenBox.Clear();
    }

    private void BeendeProgrammpunktBearbeitung()
    {
        _bearbeiteterProgrammpunktId = null;
        LeereProgrammpunktFormular();
        AktualisiereProgrammpunktFormularModus();
    }

    private void AktualisiereProgrammpunktFormularModus()
    {
        var istBearbeitung = _bearbeiteterProgrammpunktId != null;

        PunktFormularTitel.Text = istBearbeitung ? ProgrammpunktBearbeitenTitel : NeuerProgrammpunktTitel;
        PunktSpeichernButton.Content = istBearbeitung ? SpeichernText : HinzufuegenText;
        PunktAbbrechenButton.Visibility = istBearbeitung ? Visibility.Visible : Visibility.Collapsed;
    }

    private void ErledigtButton_Click(object sender, RoutedEventArgs e)
    {
        if (ProgrammGrid.SelectedItem is not ProgrammpunktDto programmpunkt)
        {
            MessageBox.Show(KeinProgrammpunktGewaehltMeldung);
            return;
        }

        _programmpunktService.ErledigtUmschalten(programmpunkt.Id);
    }

    private void PunktLoeschenButton_Click(object sender, RoutedEventArgs e)
    {
        if (ProgrammGrid.SelectedItem is not ProgrammpunktDto programmpunkt)
        {
            MessageBox.Show(KeinProgrammpunktGewaehltMeldung);
            return;
        }

        if (_bearbeiteterProgrammpunktId == programmpunkt.Id)
        {
            BeendeProgrammpunktBearbeitung();
        }

        _programmpunktService.Loeschen(programmpunkt.Id);
    }

    private static string FormatiereDatum(DateTime? datum)
    {
        return datum?.ToString(DatumsFormat) ?? "";
    }

    private static DateTime? LeseDatum(string datum)
    {
        return DateTime.TryParseExact(datum, DatumsFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var ergebnis)
            ? ergebnis
            : null;
    }

    private static string FormatiereBetrag(decimal betrag)
    {
        return betrag.ToString(BetragsFormat, CultureInfo.InvariantCulture);
    }
}
