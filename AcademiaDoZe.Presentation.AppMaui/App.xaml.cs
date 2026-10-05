// gabriel geremias vieira
using AcademiaDoZe.Application.DependencyInjection;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Presentation.AppMaui.Configuration;
using AcademiaDoZe.Presentation.AppMaui.Helpers;
using AcademiaDoZe.Presentation.AppMaui.Message;
using CommunityToolkit.Mvvm.Messaging;

namespace AcademiaDoZe.Presentation.AppMaui;

// Application qualificado por causa do namespace AcademiaDoZe.Application da camada de aplicação.
public partial class App : Microsoft.Maui.Controls.Application
{
    private readonly RepositoryConfig _repositoryConfig;

    public App(RepositoryConfig repositoryConfig)
    {
        _repositoryConfig = repositoryConfig;

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

        // Toda vez que o usuário trocar as credenciais do banco, a configuração
        // compartilhada é atualizada e os próximos repositórios já usam os novos valores
        WeakReferenceMessenger.Default.Register<BancoPreferencesUpdatedMessage>(this, (r, m) =>
        {
            AplicarBanco();
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

    // As fábricas de repositório leem estas propriedades a cada chamada,
    // então basta atualizar a instância compartilhada.
    private void AplicarBanco()
    {
        _repositoryConfig.ConnectionString = AppSettings.ConnectionString;
        _repositoryConfig.DatabaseType = AppSettings.BancoSelecionado.ToInfrastructure();
    }
}
