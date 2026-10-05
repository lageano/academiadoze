// gabriel geremias vieira
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Presentation.AppMaui.Resources.Strings;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class ColaboradorListViewModel : BaseViewModel
{
    private readonly IColaboradorService _colaboradorService;

    public IReadOnlyList<string> TiposFiltro { get; } = ["Nome", "CPF", "E-mail", "Tipo", "Vínculo"];

    private string _filtroSelecionado = "Nome";
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

    private ObservableCollection<ColaboradorDto> _colaboradores = [];
    public ObservableCollection<ColaboradorDto> Colaboradores
    {
        get => _colaboradores;
        set
        {
            if (SetProperty(ref _colaboradores, value))
                OnPropertyChanged(nameof(TotalRegistros));
        }
    }

    public int TotalRegistros => Colaboradores.Count;

    public ColaboradorListViewModel(IColaboradorService colaboradorService)
    {
        _colaboradorService = colaboradorService;
        Title = AppResources.strColaborador;
    }

    [RelayCommand]
    public async Task CarregarAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            Colaboradores = [.. await _colaboradorService.ObterTodosAsync(cts.Token)];
            OnPropertyChanged(nameof(TotalRegistros));
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao carregar colaboradores: {ex.Message}", "OK");
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

            if (string.IsNullOrWhiteSpace(termo))
            {
                Colaboradores = [.. await _colaboradorService.ObterTodosAsync(cts.Token)];
            }
            else if (FiltroSelecionado == "CPF")
            {
                var digitos = new string(termo.Where(char.IsDigit).ToArray());
                var encontrado = await _colaboradorService.ObterPorCpfAsync(digitos, cts.Token);
                Colaboradores = encontrado is null ? [] : [encontrado];
            }
            else if (FiltroSelecionado == "E-mail")
            {
                var encontrado = await _colaboradorService.ObterPorEmailAsync(termo, cts.Token);
                Colaboradores = encontrado is null ? [] : [encontrado];
            }
            else if (FiltroSelecionado == "Tipo" && Enum.TryParse<AppColaboradorTipo>(termo, true, out var tipo))
            {
                Colaboradores = [.. await _colaboradorService.ObterPorTipoAsync(tipo, cts.Token)];
            }
            else if (FiltroSelecionado == "Vínculo" && Enum.TryParse<AppColaboradorVinculo>(termo, true, out var vinculo))
            {
                Colaboradores = [.. await _colaboradorService.ObterPorVinculoAsync(vinculo, cts.Token)];
            }
            else
            {
                var todos = await _colaboradorService.ObterTodosAsync(cts.Token);
                Colaboradores = [.. todos.Where(c => c.Nome.Contains(termo, StringComparison.OrdinalIgnoreCase))];
            }

            OnPropertyChanged(nameof(TotalRegistros));
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao buscar colaboradores: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private static async Task AdicionarAsync()
        => await Shell.Current.GoToAsync("colaborador");

    [RelayCommand]
    private static async Task EditarAsync(ColaboradorDto? colaborador)
    {
        if (colaborador is null) return;
        await Shell.Current.GoToAsync($"colaborador?Id={colaborador.Id}");
    }

    [RelayCommand]
    private async Task ExcluirAsync(ColaboradorDto? colaborador)
    {
        if (colaborador is null) return;

        bool confirma = await Shell.Current.DisplayAlertAsync(
            "Confirmar Exclusão",
            $"Deseja realmente excluir o colaborador {colaborador.Nome}?",
            "Sim", "Não");

        if (!confirma) return;

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));

            if (await _colaboradorService.RemoverAsync(colaborador.Id, cts.Token))
            {
                Colaboradores.Remove(colaborador);
                OnPropertyChanged(nameof(TotalRegistros));
            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Erro", "Não foi possível excluir o colaborador.", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao excluir: {ex.Message}", "OK");
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
