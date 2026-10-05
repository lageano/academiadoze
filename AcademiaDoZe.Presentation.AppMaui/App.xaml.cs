// gabriel geremias vieira
using AcademiaDoZe.Presentation.AppMaui.Helpers;
using AcademiaDoZe.Presentation.AppMaui.Message;
using CommunityToolkit.Mvvm.Messaging;

namespace AcademiaDoZe.Presentation.AppMaui;

// Application qualificado por causa do namespace AcademiaDoZe.Application da camada de aplicação.
public partial class App : Microsoft.Maui.Controls.Application
{
    public App()
    {
        InitializeComponent();

        // Disponibiliza o gerenciador de idioma também como recurso da aplicação
        Resources["LocalizedStrings"] = LocalizationManager.Instance;

        // Aplica o tema salvo nas preferências
        AplicarTema();

        // Toda vez que o usuário trocar o tema, essa mensagem chega e o tema é atualizado
        WeakReferenceMessenger.Default.Register<TemaPreferencesUpdatedMessage>(this, (r, m) =>
        {
            AplicarTema();
        });

        // Toda vez que o usuário trocar o idioma, a cultura é aplicada e a interface recarrega
        WeakReferenceMessenger.Default.Register<CulturaPreferencesUpdatedMessage>(this, (r, m) =>
        {
            LocalizationManager.Instance.SetCulture(m.Value);
        });
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell())
        {
            Title = "Academia do Zé"
        };
    }

    // "light" e "dark" forçam o tema; qualquer outro valor acompanha o sistema operacional.
    private void AplicarTema()
    {
        UserAppTheme = Preferences.Get("Tema", "system") switch
        {
            "light" => AppTheme.Light,
            "dark" => AppTheme.Dark,
            _ => AppTheme.Unspecified
        };
    }
}
