// gabriel geremias vieira
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Presentation.AppMaui.Configuration;
using AcademiaDoZe.Presentation.AppMaui.Message;
using AcademiaDoZe.Presentation.AppMaui.Resources.Strings;
using CommunityToolkit.Mvvm.Messaging;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class ConfigPage : ContentPage
{
    // Mesma ordem do enum AppDatabaseType
    private static readonly AppDatabaseType[] Tipos =
    [
        AppDatabaseType.SqlServer,
        AppDatabaseType.MySql,
        AppDatabaseType.Sqlite
    ];

    // Evita que o Picker dispare a troca de idioma enquanto a tela ainda está carregando
    private bool _isCarregando = true;

    public ConfigPage()
    {
        InitializeComponent();
        CarregarTema();
        CarregarBanco();
        CarregarCultura();
        _isCarregando = false;
    }

    // Seleciona na tela o idioma salvo em Preferences
    private void CarregarCultura()
    {
        CulturaPicker.SelectedIndex = Preferences.Get("Cultura", "pt-BR") switch
        {
            "en-US" => 0,
            "es-ES" => 1,
            _ => 2
        };
    }

    private void OnCulturaSelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_isCarregando || CulturaPicker.SelectedIndex < 0) return;

        string selecionada = CulturaPicker.SelectedIndex switch
        {
            0 => "en-US",
            1 => "es-ES",
            _ => "pt-BR"
        };

        // Dispara a mensagem; quem aplica a cultura é o App, que está inscrito
        WeakReferenceMessenger.Default.Send(new CulturaPreferencesUpdatedMessage(selecionada));
    }

    // Seleciona na tela o tema salvo em Preferences; sem preferência, usa "system".
    private void CarregarTema()
    {
        TemaPicker.SelectedIndex = Preferences.Get("Tema", "system") switch
        {
            "light" => 0,
            "dark" => 1,
            _ => 2
        };
    }

    private async void OnSalvarTemaClicked(object sender, EventArgs e)
    {
        string temaSelecionado = TemaPicker.SelectedIndex switch
        {
            0 => "light",
            1 => "dark",
            _ => "system"
        };

        Preferences.Set("Tema", temaSelecionado);

        // Dispara a mensagem; quem aplica o tema é o App, que está inscrito
        WeakReferenceMessenger.Default.Send(new TemaPreferencesUpdatedMessage("TemaAlterado"));

        await DisplayAlertAsync(AppResources.strSucesso, AppResources.strDadosSalvos, "OK");
        await Shell.Current.GoToAsync("//dashboard");
    }

    // Preenche os campos com as credenciais salvas e mostra só os campos do tipo escolhido.
    private void CarregarBanco()
    {
        BancoPicker.ItemsSource = Tipos.Select(t => t.GetDisplayName()).ToList();
        BancoPicker.SelectedIndex = Array.IndexOf(Tipos, AppSettings.BancoSelecionado);

        ServidorEntry.Text = AppSettings.Servidor;
        BancoEntry.Text = AppSettings.Banco;
        UsuarioEntry.Text = AppSettings.Usuario;
        SenhaEntry.Text = AppSettings.Senha;
        CaminhoEntry.Text = AppSettings.CaminhoSqlite;
        ComplementoEntry.Text = AppSettings.Complemento;

        AtualizarCamposVisiveis();
    }

    private void OnTipoBancoChanged(object sender, EventArgs e) => AtualizarCamposVisiveis();

    // O SQLite é um arquivo local: não tem servidor, usuário nem senha.
    private void AtualizarCamposVisiveis()
    {
        if (BancoPicker.SelectedIndex < 0) return;

        var ehSqlite = Tipos[BancoPicker.SelectedIndex] == AppDatabaseType.Sqlite;

        AvisoSqlite.IsVisible = ehSqlite;
        CamposSqlite.IsVisible = ehSqlite;
        CamposServidor.IsVisible = !ehSqlite;
    }

    private async void OnSalvarBancoClicked(object sender, EventArgs e)
    {
        if (BancoPicker.SelectedIndex < 0) return;

        var tipo = Tipos[BancoPicker.SelectedIndex];

        if (tipo == AppDatabaseType.Sqlite)
        {
            if (string.IsNullOrWhiteSpace(CaminhoEntry.Text))
            {
                await DisplayAlertAsync(AppResources.strAviso, AppResources.strInformeCaminho, "OK");
                return;
            }

            AppSettings.CaminhoSqlite = CaminhoEntry.Text.Trim();
        }
        else
        {
            if (string.IsNullOrWhiteSpace(ServidorEntry.Text) || string.IsNullOrWhiteSpace(BancoEntry.Text))
            {
                await DisplayAlertAsync(AppResources.strAviso, AppResources.strInformeServidor, "OK");
                return;
            }

            AppSettings.Servidor = ServidorEntry.Text.Trim();
            AppSettings.Banco = BancoEntry.Text.Trim();
            AppSettings.Usuario = UsuarioEntry.Text?.Trim() ?? string.Empty;
            AppSettings.Senha = SenhaEntry.Text ?? string.Empty;
        }

        AppSettings.BancoSelecionado = tipo;
        AppSettings.Complemento = ComplementoEntry.Text?.Trim() ?? string.Empty;

        // Dispara a mensagem; quem reconfigura os repositórios é o App
        WeakReferenceMessenger.Default.Send(new BancoPreferencesUpdatedMessage("BancoAlterado"));

        await DisplayAlertAsync(AppResources.strSucesso, AppResources.strDadosSalvos, "OK");
        await Shell.Current.GoToAsync("//dashboard");
    }

    private async void OnCancelarClicked(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("//dashboard");

    private async void OnVoltarClicked(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("//dashboard");

    // Ao fechar a página, desinscreve o mensageiro para evitar vazamento de memória
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }
}
