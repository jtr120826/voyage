using System.Windows;
using Reiseplaner.Services;

namespace Reiseplaner;

/// <summary>
/// Anmeldedialog. Gibt es noch kein Konto, legt er stattdessen den ersten Administrator an (Ersteinrichtung).
/// Das Dialogergebnis ist true, wenn jemand angemeldet ist.
/// </summary>
public partial class LoginWindow : Window
{
    private const string AnmeldungFehlgeschlagenMeldung = "Benutzername oder Passwort ist falsch.";
    private const string PasswoerterUngleichMeldung = "Die beiden Passwörter stimmen nicht überein.";

    private readonly AuthService _authService;
    private readonly bool _istErsteinrichtung;

    public LoginWindow(AuthService authService)
    {
        _authService = authService;
        _istErsteinrichtung = !authService.GibtEsBenutzer();

        InitializeComponent();

        if (_istErsteinrichtung)
        {
            Title = "Reiseplaner – Ersteinrichtung";
            UeberschriftText.Text = "Ersten Administrator anlegen";
            HinweisText.Text = $"Es gibt noch kein Benutzerkonto. Dieses Konto wird Administrator. "
                + $"Das Passwort braucht mindestens {AuthService.MindestlaengePasswort} Zeichen.";
            WiederholenPanel.Visibility = Visibility.Visible;
            AnmeldenButton.Content = "Anlegen";
        }

        Loaded += (_, _) => BenutzernameBox.Focus();
    }

    private void AnmeldenButton_Click(object sender, RoutedEventArgs e)
    {
        FehlerText.Text = "";

        if (_istErsteinrichtung)
        {
            LegeErstenAdministratorAn();
        }
        else
        {
            MeldeAn();
        }
    }

    private void MeldeAn()
    {
        // Immer dieselbe Meldung, egal ob der Benutzername oder das Passwort nicht stimmt.
        var benutzer = _authService.MeldeAn(BenutzernameBox.Text, PasswortBox.Password);
        if (benutzer == null)
        {
            FehlerText.Text = AnmeldungFehlgeschlagenMeldung;
            PasswortBox.Clear();
            return;
        }

        BeendeMitErfolg();
    }

    private void LegeErstenAdministratorAn()
    {
        if (PasswortBox.Password != PasswortWiederholenBox.Password)
        {
            FehlerText.Text = PasswoerterUngleichMeldung;
            return;
        }

        try
        {
            _authService.ErstelleErstenAdministrator(BenutzernameBox.Text, PasswortBox.Password);
        }
        catch (ArgumentException ex)
        {
            FehlerText.Text = ex.Message;
            return;
        }

        // Das neue Konto ist angelegt, jetzt direkt damit anmelden.
        if (_authService.MeldeAn(BenutzernameBox.Text, PasswortBox.Password) == null)
        {
            FehlerText.Text = AnmeldungFehlgeschlagenMeldung;
            return;
        }

        BeendeMitErfolg();
    }

    private void BeendeMitErfolg()
    {
        PasswortBox.Clear();
        PasswortWiederholenBox.Clear();
        DialogResult = true;
    }
}
