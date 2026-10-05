// gabriel geremias vieira
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

[QueryProperty(nameof(LogradouroId), "Id")]
public partial class LogradouroViewModel : BaseViewModel
{
    private readonly ILogradouroService _logradouroService;

    private LogradouroDto _logradouro = new()
    {
        Cep = string.Empty,
        Nome = string.Empty,
        Bairro = string.Empty,
        Cidade = string.Empty,
        Estado = string.Empty,
        Pais = "Brasil"
    };

    public LogradouroDto Logradouro
    {
        get => _logradouro;
        set => SetProperty(ref _logradouro, value);
    }

    private int _logradouroId;
    public int LogradouroId
    {
        get => _logradouroId;
        set => SetProperty(ref _logradouroId, value);
    }

    private bool _isEditMode;
    public bool IsEditMode
    {
        get => _isEditMode;
        set => SetProperty(ref _isEditMode, value);
    }

    public LogradouroViewModel(ILogradouroService logradouroService)
    {
        _logradouroService = logradouroService;
        Title = "Novo Logradouro";
    }

    public async Task InicializarAsync()
    {
        if (LogradouroId > 0)
        {
            IsEditMode = true;
            Title = "Editar Logradouro";
            await CarregarAsync();
        }
        else
        {
            IsEditMode = false;
            Title = "Novo Logradouro";
        }
    }

    private async Task CarregarAsync()
    {
        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            var dados = await _logradouroService.ObterPorIdAsync(LogradouroId, cts.Token);
            if (dados != null) Logradouro = dados;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao carregar logradouro: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SalvarAsync()
    {
        if (IsBusy) return;
        if (!await ValidarAsync(Logradouro)) return;

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));

            if (IsEditMode)
            {
                await _logradouroService.AtualizarAsync(Logradouro, cts.Token);
                await Shell.Current.DisplayAlertAsync("Sucesso", "Logradouro atualizado com sucesso!", "OK");
            }
            else
            {
                await _logradouroService.AdicionarAsync(Logradouro, cts.Token);
                await Shell.Current.DisplayAlertAsync("Sucesso", "Logradouro criado com sucesso!", "OK");
            }

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao salvar logradouro: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private static async Task CancelarAsync()
        => await Shell.Current.GoToAsync("..");

    // Validação no padrão Notification: junta todos os erros e mostra de uma vez.
    private static async Task<bool> ValidarAsync(LogradouroDto logradouro)
    {
        var erros = new List<string>();

        if (string.IsNullOrWhiteSpace(logradouro.Cep)) erros.Add("• CEP é obrigatório.");
        else if (logradouro.Cep.Count(char.IsDigit) != 8) erros.Add("• CEP deve conter 8 dígitos.");

        if (string.IsNullOrWhiteSpace(logradouro.Nome)) erros.Add("• Nome do logradouro é obrigatório.");
        if (string.IsNullOrWhiteSpace(logradouro.Bairro)) erros.Add("• Bairro é obrigatório.");
        if (string.IsNullOrWhiteSpace(logradouro.Cidade)) erros.Add("• Cidade é obrigatória.");
        if (string.IsNullOrWhiteSpace(logradouro.Estado)) erros.Add("• Estado é obrigatório.");
        if (string.IsNullOrWhiteSpace(logradouro.Pais)) erros.Add("• País é obrigatório.");

        if (erros.Count > 0)
        {
            await Shell.Current.DisplayAlertAsync("Erros de Validação",
                "Por favor, corrija os seguintes campos:\n\n" + string.Join("\n", erros), "OK");
            return false;
        }

        return true;
    }
}
