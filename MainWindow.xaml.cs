using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Reiseplaner.Models;
using Reiseplaner.Repositories;

namespace Reiseplaner;

public partial class MainWindow : Window
{
    private readonly ReiseRepository _reiseRepository = new();
    private readonly ProgrammpunktRepository _programmpunktRepository = new();

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

    private void ReisenLaden()
    {
        ReisenGrid.ItemsSource = _reiseRepository.GetAll();
    }

    // Lädt die Reisen neu (aktualisiert Geplantes/Verbleibendes Budget) und behält die Auswahl bei,
    // damit die Programmpunkt-Ansicht nach einer Änderung nicht verloren geht.
    private void ReisenAktualisierenMitAuswahl()
    {
        var aktuelleId = AktuelleReise?.Id;
        var reisen = _reiseRepository.GetAll();
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

        var punkte = _programmpunktRepository.GetByReise(reise.Id);
        ProgrammGrid.ItemsSource = punkte;

        var summeKosten = punkte.Sum(p => p.Kosten);
        var restbudget = reise.Budget - summeKosten;

        BudgetInfoText.Text = restbudget >= 0
            ? $"Budget: {reise.Budget:C} | Ausgegeben: {summeKosten:C} | Rest: {restbudget:C}"
            : $"Budget: {reise.Budget:C} | Ausgegeben: {summeKosten:C} | ÜBERSCHRITTEN um {-restbudget:C}!";
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

        _reiseRepository.Add(reise);

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

        _reiseRepository.Delete(reise.Id);
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

        _programmpunktRepository.Add(punkt);

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

        _programmpunktRepository.SetErledigt(punkt.Id, !punkt.Erledigt);
        ReisenAktualisierenMitAuswahl();
    }

    private void PunktLoeschenButton_Click(object sender, RoutedEventArgs e)
    {
        if (ProgrammGrid.SelectedItem is not Programmpunkt punkt)
        {
            MessageBox.Show("Bitte zuerst einen Programmpunkt auswählen.");
            return;
        }

        _programmpunktRepository.Delete(punkt.Id);
        ReisenAktualisierenMitAuswahl();
    }
}
