using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Reiseplaner.Dtos;
using Reiseplaner.Services;

namespace Reiseplaner;

public partial class MainWindow : Window
{
    private readonly ReiseService _reiseService;
    private readonly ProgrammpunktService _programmpunktService;

    public MainWindow(ReiseService reiseService, ProgrammpunktService programmpunktService)
    {
        _reiseService = reiseService;
        _programmpunktService = programmpunktService;

        InitializeComponent();

        // XAML-Bindings (z.B. StringFormat={}{0:C}) nutzen sonst immer en-US statt der
        // System-Kultur (bekannter WPF-Effekt) - dadurch würde CHF 1'500.00 im Code-Behind-Text
        // nicht zur $-Anzeige in den DataGrid-Spalten passen.
        Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);

        ReisenLaden();
    }

    private ReiseDto? AktuelleReise => ReisenGrid.SelectedItem as ReiseDto;

    // Blendet den Platzhaltertext (z.B. "Reisetitel") aus, sobald die zugehörige TextBox
    // Inhalt hat, und wieder ein, sobald sie leer ist (reine UI-Anzeigelogik).
    private void PlatzhalterAktualisieren(object sender, TextChangedEventArgs e)
    {
        var box = (TextBox)sender;
        var istLeer = box.Text.Length == 0;

        if (box == TitelBox) TitelPlatzhalter.Visibility = istLeer ? Visibility.Visible : Visibility.Collapsed;
        else if (box == ZielortBox) ZielortPlatzhalter.Visibility = istLeer ? Visibility.Visible : Visibility.Collapsed;
        else if (box == BudgetBox) BudgetPlatzhalter.Visibility = istLeer ? Visibility.Visible : Visibility.Collapsed;
        else if (box == PunktTitelBox) PunktTitelPlatzhalter.Visibility = istLeer ? Visibility.Visible : Visibility.Collapsed;
        else if (box == KostenBox) KostenPlatzhalter.Visibility = istLeer ? Visibility.Visible : Visibility.Collapsed;
    }

    private void ReisenLaden()
    {
        ReisenGrid.ItemsSource = _reiseService.GetAlle();
    }

    // Lädt die Reisen neu (aktualisiert Geplantes/Verbleibendes Budget) und behält die Auswahl bei,
    // damit die Programmpunkt-Ansicht nach einer Änderung nicht verloren geht.
    private void ReisenAktualisierenMitAuswahl()
    {
        var aktuelleId = AktuelleReise?.Id;
        var reisen = _reiseService.GetAlle();
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

        ProgrammGrid.ItemsSource = _programmpunktService.GetByReise(reise.Id);

        BudgetInfoText.Text = reise.IstBudgetUeberschritten
            ? $"Budget: {reise.Budget:C} | Ausgegeben: {reise.GeplantesBudget:C} | ÜBERSCHRITTEN um {-reise.VerbleibendesBudget:C}!"
            : $"Budget: {reise.Budget:C} | Ausgegeben: {reise.GeplantesBudget:C} | Rest: {reise.VerbleibendesBudget:C}";
    }

    private void ReiseHinzufuegenButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TitelBox.Text) || string.IsNullOrWhiteSpace(ZielortBox.Text)
            || StartPicker.SelectedDate == null || EndePicker.SelectedDate == null)
        {
            MessageBox.Show("Bitte Titel, Zielort, Start- und Enddatum ausfüllen.");
            return;
        }

        decimal.TryParse(BudgetBox.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out var budget);

        var reiseDto = new ReiseDto
        {
            Titel = TitelBox.Text,
            Zielort = ZielortBox.Text,
            Startdatum = StartPicker.SelectedDate.Value.ToString("yyyy-MM-dd"),
            Enddatum = EndePicker.SelectedDate.Value.ToString("yyyy-MM-dd"),
            Budget = budget
        };

        _reiseService.Hinzufuegen(reiseDto);

        TitelBox.Clear();
        ZielortBox.Clear();
        StartPicker.SelectedDate = null;
        EndePicker.SelectedDate = null;
        BudgetBox.Clear();

        ReisenLaden();
    }

    private void ReiseLoeschenButton_Click(object sender, RoutedEventArgs e)
    {
        var reise = AktuelleReise;
        if (reise == null)
        {
            MessageBox.Show("Bitte zuerst eine Reise auswählen.");
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
            MessageBox.Show("Bitte zuerst eine Reise auswählen.");
            return;
        }

        if (string.IsNullOrWhiteSpace(PunktTitelBox.Text) || PunktDatumPicker.SelectedDate == null
            || KategorieBox.SelectedItem == null)
        {
            MessageBox.Show("Bitte Titel, Datum und Kategorie ausfüllen.");
            return;
        }

        decimal.TryParse(KostenBox.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out var kosten);

        var programmpunktDto = new ProgrammpunktDto
        {
            Titel = PunktTitelBox.Text,
            Datum = PunktDatumPicker.SelectedDate.Value.ToString("yyyy-MM-dd"),
            Kategorie = ((ComboBoxItem)KategorieBox.SelectedItem).Content.ToString()!,
            Kosten = kosten
        };

        _programmpunktService.Hinzufuegen(reise.Id, programmpunktDto);

        PunktTitelBox.Clear();
        PunktDatumPicker.SelectedDate = null;
        KategorieBox.SelectedItem = null;
        KostenBox.Clear();

        ReisenAktualisierenMitAuswahl();
    }

    private void ErledigtButton_Click(object sender, RoutedEventArgs e)
    {
        if (ProgrammGrid.SelectedItem is not ProgrammpunktDto punkt)
        {
            MessageBox.Show("Bitte zuerst einen Programmpunkt auswählen.");
            return;
        }

        _programmpunktService.ErledigtUmschalten(punkt.Id);
        ReisenAktualisierenMitAuswahl();
    }

    private void PunktLoeschenButton_Click(object sender, RoutedEventArgs e)
    {
        if (ProgrammGrid.SelectedItem is not ProgrammpunktDto punkt)
        {
            MessageBox.Show("Bitte zuerst einen Programmpunkt auswählen.");
            return;
        }

        _programmpunktService.Loeschen(punkt.Id);
        ReisenAktualisierenMitAuswahl();
    }
}
