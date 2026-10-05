// gabriel geremias vieira
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Presentation.AppMaui.Helpers;
using AcademiaDoZe.Presentation.AppMaui.Message;
using AcademiaDoZe.Presentation.AppMaui.Resources.Strings;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class DashboardViewModel : BaseViewModel
{
    private readonly IAlunoService _alunoService;
    private readonly IColaboradorService _colaboradorService;
    private readonly IMatriculaService _matriculaService;

    // Rótulo exibido no Picker -> código da cultura gravado nas preferências
    private static readonly Dictionary<string, string> CulturaPorRotulo = new()
    {
        ["Português (pt-BR)"] = "pt-BR",
        ["English (en-US)"] = "en-US",
        ["Español (es-ES)"] = "es-ES"
    };

    public IReadOnlyList<string> Idiomas { get; } = [.. CulturaPorRotulo.Keys];

    private string _idiomaSelecionado;
    public string IdiomaSelecionado
    {
        get => _idiomaSelecionado;
        set
        {
            if (SetProperty(ref _idiomaSelecionado, value))
                AplicarIdioma(value);
        }
    }

    // O tema é apresentado traduzido, mas gravado como "light", "dark" ou "system"
    public IReadOnlyList<string> Temas => [AppResources.strTemaSistema, AppResources.strTemaClaro, AppResources.strTemaEscuro];

    private string _temaSelecionado;
    public string TemaSelecionado
    {
        get => _temaSelecionado;
        set
        {
            if (SetProperty(ref _temaSelecionado, value))
                AplicarTema(value);
        }
    }

    private int _totalAlunos;
    public int TotalAlunos
    {
        get => _totalAlunos;
        set => SetProperty(ref _totalAlunos, value);
    }

    private int _totalColaboradores;
    public int TotalColaboradores
    {
        get => _totalColaboradores;
        set => SetProperty(ref _totalColaboradores, value);
    }

    private int _totalMatriculasAtivas;
    public int TotalMatriculasAtivas
    {
        get => _totalMatriculasAtivas;
        set => SetProperty(ref _totalMatriculasAtivas, value);
    }

    public DashboardViewModel(IAlunoService alunoService, IColaboradorService colaboradorService, IMatriculaService matriculaService)
    {
        _alunoService = alunoService;
        _colaboradorService = colaboradorService;
        _matriculaService = matriculaService;
        Title = AppResources.strDashboard;

        // Os seletores começam no que já está salvo nas preferências
        var culturaSalva = Preferences.Get("Cultura", LocalizationManager.ObterCulturaPadrao());
        _idiomaSelecionado = CulturaPorRotulo.FirstOrDefault(p => p.Value == culturaSalva).Key ?? "Português (pt-BR)";

        _temaSelecionado = Preferences.Get("Tema", "system") switch
        {
            "light" => AppResources.strTemaClaro,
            "dark" => AppResources.strTemaEscuro,
            _ => AppResources.strTemaSistema
        };
    }

    [RelayCommand]
    public async Task CarregarResumoAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));

            var alunos = await _alunoService.ObterTodosAsync(cts.Token);
            var colaboradores = await _colaboradorService.ObterTodosAsync(cts.Token);
            var ativas = await _matriculaService.ObterAtivasAsync(0, cts.Token);

            TotalAlunos = alunos.Count();
            TotalColaboradores = colaboradores.Count();
            TotalMatriculasAtivas = ativas.Count();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Não foi possível carregar o resumo: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private static async Task NavegarAsync(string rota)
    {
        if (string.IsNullOrWhiteSpace(rota)) return;
        await Shell.Current.GoToAsync($"//{rota}");
    }

    // A troca é publicada como mensagem: quem aplica de fato é o App, que assina CulturaPreferencesUpdatedMessage.
    private void AplicarIdioma(string idioma)
    {
        if (!CulturaPorRotulo.TryGetValue(idioma ?? string.Empty, out var cultura)) return;

        WeakReferenceMessenger.Default.Send(new CulturaPreferencesUpdatedMessage(cultura));

        Title = AppResources.strDashboard;

        // Os próprios rótulos dos temas são traduzidos, então a lista precisa ser reavaliada
        OnPropertyChanged(nameof(Temas));
        _temaSelecionado = Preferences.Get("Tema", "system") switch
        {
            "light" => AppResources.strTemaClaro,
            "dark" => AppResources.strTemaEscuro,
            _ => AppResources.strTemaSistema
        };
        OnPropertyChanged(nameof(TemaSelecionado));
    }

    // Grava a preferência e avisa o App, que aplica o tema na janela inteira.
    private void AplicarTema(string tema)
    {
        var valor = tema == AppResources.strTemaClaro ? "light"
                  : tema == AppResources.strTemaEscuro ? "dark"
                  : "system";

        Preferences.Set("Tema", valor);
        WeakReferenceMessenger.Default.Send(new TemaPreferencesUpdatedMessage(valor));
    }
}
