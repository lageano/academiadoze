// gabriel geremias vieira
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Presentation.AppMaui.Resources.Strings;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class MatriculaListViewModel : BaseViewModel
{
    private readonly IMatriculaService _matriculaService;

    public IReadOnlyList<string> TiposFiltro { get; } = ["Ativas", "Todas", "Id", "Id Aluno", "Vencendo em dias"];

    private string _filtroSelecionado = "Ativas";
    public string FiltroSelecionado
    {
        get => _filtroSelecionado;
        set => SetProperty(ref _filtroSelecionado, value);
    }

    private string _textoBusca = string.Empty;
    public string TextoBusca
    {
        get => _textoBusca;
        set => SetProperty(ref _textoBusca, value);
    }

    private ObservableCollection<MatriculaDto> _matriculas = [];
    public ObservableCollection<MatriculaDto> Matriculas
    {
        get => _matriculas;
        set
        {
            if (SetProperty(ref _matriculas, value))
                OnPropertyChanged(nameof(TotalRegistros));
        }
    }

    public int TotalRegistros => Matriculas.Count;

    public MatriculaListViewModel(IMatriculaService matriculaService)
    {
        _matriculaService = matriculaService;
        Title = AppResources.strMatricula;
    }

    [RelayCommand]
    public async Task CarregarAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            Matriculas = [.. await _matriculaService.ObterTodasAsync(cts.Token)];
            OnPropertyChanged(nameof(TotalRegistros));
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao carregar matrículas: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task BuscarAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            var termo = TextoBusca?.Trim() ?? string.Empty;

            IEnumerable<MatriculaDto> resultados = [];

            if (string.IsNullOrWhiteSpace(termo))
            {
                resultados = FiltroSelecionado == "Ativas"
                    ? await _matriculaService.ObterAtivasAsync(0, cts.Token)
                    : await _matriculaService.ObterTodasAsync(cts.Token);
            }
            else if (FiltroSelecionado == "Id")
            {
                if (int.TryParse(termo, out int id))
                {
                    var matricula = await _matriculaService.ObterPorIdAsync(id, cts.Token);
                    if (matricula != null) resultados = [matricula];
                }
                else
                {
                    await Shell.Current.DisplayAlertAsync("Aviso", "Para busca por ID, informe um número válido.", "OK");
                }
            }
            else if (FiltroSelecionado == "Id Aluno")
            {
                if (int.TryParse(termo, out int alunoId))
                    resultados = await _matriculaService.ObterPorAlunoIdAsync(alunoId, cts.Token);
                else
                    await Shell.Current.DisplayAlertAsync("Aviso", "Para busca por ID do Aluno, informe um número válido.", "OK");
            }
            else if (FiltroSelecionado == "Ativas")
            {
                int.TryParse(termo, out int alunoId);
                resultados = await _matriculaService.ObterAtivasAsync(alunoId, cts.Token);
            }
            else if (FiltroSelecionado == "Vencendo em dias")
            {
                if (int.TryParse(termo, out int dias))
                    resultados = await _matriculaService.ObterVencendoEmDiasAsync(dias, cts.Token);
                else
                    await Shell.Current.DisplayAlertAsync("Aviso", "Para busca por vencimento, informe a quantidade de dias.", "OK");
            }
            else
            {
                resultados = await _matriculaService.ObterTodasAsync(cts.Token);
            }

            Matriculas = [.. resultados];
            OnPropertyChanged(nameof(TotalRegistros));
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao buscar matrículas: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private static async Task AdicionarAsync()
        => await Shell.Current.GoToAsync("matricula");

    [RelayCommand]
    private static async Task EditarAsync(MatriculaDto? matricula)
    {
        if (matricula is null) return;
        await Shell.Current.GoToAsync($"matricula?Id={matricula.Id}");
    }

    [RelayCommand]
    private async Task ExcluirAsync(MatriculaDto? matricula)
    {
        if (matricula is null) return;

        bool confirma = await Shell.Current.DisplayAlertAsync(
            "Confirmar Exclusão",
            $"Deseja realmente excluir a matrícula {matricula.Id}?",
            "Sim", "Não");

        if (!confirma) return;

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));

            if (await _matriculaService.RemoverAsync(matricula.Id, cts.Token))
            {
                Matriculas.Remove(matricula);
                OnPropertyChanged(nameof(TotalRegistros));
            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Erro", "Não foi possível excluir a matrícula.", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao excluir matrícula: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task AtualizarAsync()
    {
        IsRefreshing = true;
        await CarregarAsync();
    }
}
