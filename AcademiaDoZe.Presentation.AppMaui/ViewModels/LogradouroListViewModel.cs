// gabriel geremias vieira
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Presentation.AppMaui.Resources.Strings;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class LogradouroListViewModel : BaseViewModel
{
    private readonly ILogradouroService _logradouroService;

    public IReadOnlyList<string> TiposFiltro { get; } = ["Todos", "CEP", "Cidade"];

    private string _filtroSelecionado = "Todos";
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

    private ObservableCollection<LogradouroDto> _logradouros = [];
    public ObservableCollection<LogradouroDto> Logradouros
    {
        get => _logradouros;
        set
        {
            if (SetProperty(ref _logradouros, value))
                OnPropertyChanged(nameof(TotalRegistros));
        }
    }

    public int TotalRegistros => Logradouros.Count;

    public LogradouroListViewModel(ILogradouroService logradouroService)
    {
        _logradouroService = logradouroService;
        Title = AppResources.strLogradouro;
    }

    [RelayCommand]
    public async Task CarregarAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            var lista = await _logradouroService.ObterTodosAsync(cts.Token);
            Logradouros = [.. lista];
            OnPropertyChanged(nameof(TotalRegistros));
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao carregar logradouros: {ex.Message}", "OK");
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

            IEnumerable<LogradouroDto> resultados;

            if (string.IsNullOrWhiteSpace(termo))
            {
                resultados = await _logradouroService.ObterTodosAsync(cts.Token);
            }
            else if (FiltroSelecionado == "CEP")
            {
                var encontrado = await _logradouroService.ObterPorCepAsync(termo, cts.Token);
                resultados = encontrado is null ? [] : [encontrado];
            }
            else if (FiltroSelecionado == "Cidade")
            {
                resultados = await _logradouroService.ObterPorCidadeAsync(termo, cts.Token);
            }
            else
            {
                var todos = await _logradouroService.ObterTodosAsync(cts.Token);
                resultados = todos.Where(l =>
                    l.Nome.Contains(termo, StringComparison.OrdinalIgnoreCase)
                    || l.Bairro.Contains(termo, StringComparison.OrdinalIgnoreCase)
                    || l.Cidade.Contains(termo, StringComparison.OrdinalIgnoreCase)
                    || l.Cep.Contains(new string(termo.Where(char.IsDigit).ToArray())));
            }

            Logradouros = [.. resultados];
            OnPropertyChanged(nameof(TotalRegistros));
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao buscar logradouros: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private static async Task AdicionarAsync()
        => await Shell.Current.GoToAsync("logradouro");

    [RelayCommand]
    private static async Task EditarAsync(LogradouroDto? logradouro)
    {
        if (logradouro is null) return;
        await Shell.Current.GoToAsync($"logradouro?Id={logradouro.Id}");
    }

    [RelayCommand]
    private async Task ExcluirAsync(LogradouroDto? logradouro)
    {
        if (logradouro is null) return;

        bool confirma = await Shell.Current.DisplayAlertAsync(
            "Confirmar Exclusão",
            $"Deseja realmente excluir o logradouro {logradouro.Nome}?",
            "Sim", "Não");

        if (!confirma) return;

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));

            if (await _logradouroService.RemoverAsync(logradouro.Id, cts.Token))
            {
                Logradouros.Remove(logradouro);
                OnPropertyChanged(nameof(TotalRegistros));
            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Erro", "Não foi possível excluir o logradouro.", "OK");
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
