// gabriel geremias vieira
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Presentation.AppMaui.Resources.Strings;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class AlunoListViewModel : BaseViewModel
{
    private readonly IAlunoService _alunoService;

    public IReadOnlyList<string> TiposFiltro { get; } = ["Nome", "CPF", "E-mail"];

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

    private ObservableCollection<AlunoDto> _alunos = [];
    public ObservableCollection<AlunoDto> Alunos
    {
        get => _alunos;
        set
        {
            if (SetProperty(ref _alunos, value))
                OnPropertyChanged(nameof(TotalRegistros));
        }
    }

    public int TotalRegistros => Alunos.Count;

    public AlunoListViewModel(IAlunoService alunoService)
    {
        _alunoService = alunoService;
        Title = AppResources.strAluno;
    }

    [RelayCommand]
    public async Task CarregarAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            Alunos = [.. await _alunoService.ObterTodosAsync(cts.Token)];
            OnPropertyChanged(nameof(TotalRegistros));
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao carregar alunos: {ex.Message}", "OK");
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
                Alunos = [.. await _alunoService.ObterTodosAsync(cts.Token)];
            }
            else if (FiltroSelecionado == "CPF")
            {
                var digitos = new string(termo.Where(char.IsDigit).ToArray());
                var encontrado = await _alunoService.ObterPorCpfAsync(digitos, cts.Token);
                Alunos = encontrado is null ? [] : [encontrado];
            }
            else if (FiltroSelecionado == "E-mail")
            {
                var encontrado = await _alunoService.ObterPorEmailAsync(termo, cts.Token);
                Alunos = encontrado is null ? [] : [encontrado];
            }
            else
            {
                Alunos = [.. await _alunoService.ObterPorNomeAsync(termo, cts.Token)];
            }

            OnPropertyChanged(nameof(TotalRegistros));
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao buscar alunos: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private static async Task AdicionarAsync()
        => await Shell.Current.GoToAsync("aluno");

    [RelayCommand]
    private static async Task EditarAsync(AlunoDto? aluno)
    {
        if (aluno is null) return;
        await Shell.Current.GoToAsync($"aluno?Id={aluno.Id}");
    }

    [RelayCommand]
    private async Task ExcluirAsync(AlunoDto? aluno)
    {
        if (aluno is null) return;

        bool confirma = await Shell.Current.DisplayAlertAsync(
            "Confirmar Exclusão",
            $"Deseja realmente excluir o aluno {aluno.Nome}?",
            "Sim", "Não");

        if (!confirma) return;

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));

            if (await _alunoService.RemoverAsync(aluno.Id, cts.Token))
            {
                Alunos.Remove(aluno);
                OnPropertyChanged(nameof(TotalRegistros));
            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Erro", "Não foi possível excluir o aluno.", "OK");
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
