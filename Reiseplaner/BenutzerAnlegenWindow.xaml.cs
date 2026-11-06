using System.Windows;
using System.Windows.Media;
using Reiseplaner.Models;
using Reiseplaner.Services;

namespace Reiseplaner;

/// <summary>
/// Dialog für Administratoren, um weitere Benutzerkonten anzulegen.
/// Die Berechtigung prüft der <see cref="AuthService"/>, nicht dieser Dialog.
/// </summary>
public partial class BenutzerAnlegenWindow : Window
{
    private readonly AuthService _authService;

    public BenutzerAnlegenWindow(AuthService authService)
    {
        _authService = authService;

        InitializeComponent();

        RolleBox.ItemsSource = Enum.GetValues<Rolle>();
        RolleBox.SelectedItem = Rolle.Leser;

        Loaded += (_, _) => BenutzernameBox.Focus();
    }

    private void AnlegenButton_Click(object sender, RoutedEventArgs e)
    {
        if (PasswortBox.Password != PasswortWiederholenBox.Password)
        {
            ZeigeMeldung("Die beiden Passwörter stimmen nicht überein.", istFehler: true);
            return;
        }

        try
        {
            _authService.ErstelleBenutzer(BenutzernameBox.Text, PasswortBox.Password, (Rolle)RolleBox.SelectedItem);
        }
        catch (ArgumentException ex)
        {
            ZeigeMeldung(ex.Message, istFehler: true);
            return;
        }
        catch (UnauthorizedAccessException ex)
        {
            ZeigeMeldung(ex.Message, istFehler: true);
            return;
        }

        ZeigeMeldung($"Benutzer \"{BenutzernameBox.Text.Trim().ToLowerInvariant()}\" wurde angelegt.", istFehler: false);
        BenutzernameBox.Clear();
        PasswortBox.Clear();
        PasswortWiederholenBox.Clear();
        RolleBox.SelectedItem = Rolle.Leser;
        BenutzernameBox.Focus();
    }

    private void ZeigeMeldung(string text, bool istFehler)
    {
        MeldungText.Text = text;
        MeldungText.Foreground = istFehler ? Brushes.Firebrick : Brushes.SeaGreen;
    }
}
