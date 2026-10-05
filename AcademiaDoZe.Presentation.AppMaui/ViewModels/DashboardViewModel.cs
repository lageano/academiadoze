// gabriel geremias vieira
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Presentation.AppMaui.Helpers;
using AcademiaDoZe.Presentation.AppMaui.Resources.Strings;
using CommunityToolkit.Mvvm.Input;
using System.Globalization;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class DashboardViewModel : BaseViewModel
{
    private readonly IAlunoService _alunoService;
    private readonly IColaboradorService _colaboradorService;
    private readonly IMatriculaService _matriculaService;

    public IReadOnlyList<string> Idiomas { get; } = ["Português (pt-BR)", "English (en-US)", "Español (es-ES)"];

    private string _idiomaSelecionado = "Português (pt-BR)";
    public string IdiomaSelecionado
    {
        get => _idiomaSelecionado;
        set
        {
            if (SetProperty(ref _idiomaSelecionado, value))
                AplicarIdioma(value);
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

    private void AplicarIdioma(string idioma)
    {
        var cultura = idioma switch
        {
            "English (en-US)" => new CultureInfo("en-US"),
            "Español (es-ES)" => new CultureInfo("es-ES"),
            _ => new CultureInfo("pt-BR")
        };

        LocalizationResourceManager.Instance.SetCulture(cultura);
        Title = AppResources.strDashboard;
    }
}
