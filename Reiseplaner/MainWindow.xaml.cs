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
    private const string KeineReiseGewaehltMeldung = "Bitte zuerst eine Reise auswählen.";
    private const string KeinProgrammpunktGewaehltMeldung = "Bitte zuerst einen Programmpunkt auswählen.";
    private const string ReiseUnvollstaendigMeldung = "Bitte Titel, Zielort, Start- und Enddatum ausfüllen.";
    private const string ProgrammpunktUnvollstaendigMeldung = "Bitte Titel, Datum und Kategorie ausfüllen.";

    private readonly ReiseService _reiseService;
    private readonly ProgrammpunktService _programmpunktService;
    private readonly Dictionary<TextBox, TextBlock> _platzhalterProTextBox;

    public MainWindow(ReiseService reiseService, ProgrammpunktService programmpunktService)
    {
        _reiseService = reiseService;
        _programmpunktService = programmpunktService;

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

        ReisenLaden();
    }

    private ReiseDto? AktuelleReise => ReisenGrid.SelectedItem as ReiseDto;

    // Blendet den Platzhaltertext aus, sobald die zugehörige TextBox Inhalt hat, und wieder ein, wenn sie leer ist.
    private void PlatzhalterAktualisieren(object sender, TextChangedEventArgs e)
    {
        var textBox = (TextBox)sender;
        var platzhalter = _platzhalterProTextBox[textBox];
        platzhalter.Visibility = textBox.Text.Length == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void ReisenLaden()
    {
        ReisenGrid.ItemsSource = _reiseService.LadeAlle();
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

    private void ReiseHinzufuegenButton_Click(object sender, RoutedEventArgs e)
    {
        if (!IstReiseFormularVollstaendig())
        {
            MessageBox.Show(ReiseUnvollstaendigMeldung);
            return;
        }

        _reiseService.Hinzufuegen(LeseReiseAusFormular());
        LeereReiseFormular();
        ReisenLaden();
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

    private void LeereReiseFormular()
    {
        TitelBox.Clear();
        ZielortBox.Clear();
        StartPicker.SelectedDate = null;
        EndePicker.SelectedDate = null;
        BudgetBox.Clear();
    }

    private void ReiseLoeschenButton_Click(object sender, RoutedEventArgs e)
    {
        var reise = AktuelleReise;
        if (reise == null)
        {
            MessageBox.Show(KeineReiseGewaehltMeldung);
            return;
        }

        _reiseService.Loeschen(reise.Id);
        ReisenLaden();
        ProgrammGrid.ItemsSource = null;
        BudgetInfoText.Text = "";
    }

    private void PunktHinzufuegenButton_Click(object sender, RoutedEventArgs e)
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
        ReisenAktualisierenMitAuswahl();
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

    private void LeereProgrammpunktFormular()
    {
        PunktTitelBox.Clear();
        PunktDatumPicker.SelectedDate = null;
        KategorieBox.SelectedItem = null;
        KostenBox.Clear();
    }

    private void ErledigtButton_Click(object sender, RoutedEventArgs e)
    {
        if (ProgrammGrid.SelectedItem is not ProgrammpunktDto programmpunkt)
        {
            MessageBox.Show(KeinProgrammpunktGewaehltMeldung);
            return;
        }

        _programmpunktService.ErledigtUmschalten(programmpunkt.Id);
        ReisenAktualisierenMitAuswahl();
    }

    private void PunktLoeschenButton_Click(object sender, RoutedEventArgs e)
    {
        if (ProgrammGrid.SelectedItem is not ProgrammpunktDto programmpunkt)
        {
            MessageBox.Show(KeinProgrammpunktGewaehltMeldung);
            return;
        }

        _programmpunktService.Loeschen(programmpunkt.Id);
        ReisenAktualisierenMitAuswahl();
    }

    private static string FormatiereDatum(DateTime? datum)
    {
        return datum?.ToString(DatumsFormat) ?? "";
    }
}
