using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Reiseplaner.Models;
using Reiseplaner.Services;

namespace Reiseplaner;

public partial class MainWindow : Window
{
    private readonly ReiseService _reiseService = new();
    private readonly ProgrammpunktService _programmpunktService = new();

    public MainWindow()
    {
        InitializeComponent();

        // XAML-Bindings (z.B. StringFormat={}{0:C}) nutzen sonst immer en-US statt der
        // System-Kultur (bekannter WPF-Effekt) - dadurch würde CHF 1'500.00 im Code-Behind-Text
        // nicht zur $-Anzeige in den DataGrid-Spalten passen.
        Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);

        ReisenLaden();
    }

    private Reise? AktuelleReise => ReisenGrid.SelectedItem as Reise;

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

        var punkte = _programmpunktService.GetByReise(reise.Id);
        ProgrammGrid.ItemsSource = punkte;

        var status = _programmpunktService.BerechneBudgetStatus(reise, punkte);
        BudgetInfoText.Text = status.Ueberschritten
            ? $"Budget: {reise.Budget:C} | Ausgegeben: {status.Ausgegeben:C} | ÜBERSCHRITTEN um {-status.Rest:C}!"
            : $"Budget: {reise.Budget:C} | Ausgegeben: {status.Ausgegeben:C} | Rest: {status.Rest:C}";
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

        var reise = new Reise
        {
            Titel = TitelBox.Text,
            Zielort = ZielortBox.Text,
            Startdatum = StartPicker.SelectedDate.Value.ToString("yyyy-MM-dd"),
            Enddatum = EndePicker.SelectedDate.Value.ToString("yyyy-MM-dd"),
            Budget = budget
        };

        _reiseService.Hinzufuegen(reise);

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

        var punkt = new Programmpunkt
        {
            Titel = PunktTitelBox.Text,
            Datum = PunktDatumPicker.SelectedDate.Value.ToString("yyyy-MM-dd"),
            Kategorie = ((ComboBoxItem)KategorieBox.SelectedItem).Content.ToString()!,
            Kosten = kosten,
            ReiseId = reise.Id
        };

        _programmpunktService.Hinzufuegen(punkt);

        PunktTitelBox.Clear();
        PunktDatumPicker.SelectedDate = null;
        KategorieBox.SelectedItem = null;
        KostenBox.Clear();

        ReisenAktualisierenMitAuswahl();
    }

    private void ErledigtButton_Click(object sender, RoutedEventArgs e)
    {
        if (ProgrammGrid.SelectedItem is not Programmpunkt punkt)
        {
            MessageBox.Show("Bitte zuerst einen Programmpunkt auswählen.");
            return;
        }

        _programmpunktService.ErledigtUmschalten(punkt);
        ReisenAktualisierenMitAuswahl();
    }

    private void PunktLoeschenButton_Click(object sender, RoutedEventArgs e)
    {
        if (ProgrammGrid.SelectedItem is not Programmpunkt punkt)
        {
            MessageBox.Show("Bitte zuerst einen Programmpunkt auswählen.");
            return;
        }

        _programmpunktService.Loeschen(punkt.Id);
        ReisenAktualisierenMitAuswahl();
    }
}
