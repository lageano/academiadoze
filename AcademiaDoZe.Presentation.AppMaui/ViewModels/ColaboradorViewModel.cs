// gabriel geremias vieira
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

[QueryProperty(nameof(ColaboradorId), "Id")]
public partial class ColaboradorViewModel : BaseViewModel
{
    private readonly IColaboradorService _colaboradorService;
    private readonly ILogradouroService _logradouroService;

    public IReadOnlyList<AppColaboradorTipo> Tipos { get; } = [.. Enum.GetValues<AppColaboradorTipo>()];
    public IReadOnlyList<AppColaboradorVinculo> Vinculos { get; } = [.. Enum.GetValues<AppColaboradorVinculo>()];

    private ColaboradorDto _colaborador = CriarColaboradorVazio();
    public ColaboradorDto Colaborador
    {
        get => _colaborador;
        set => SetProperty(ref _colaborador, value);
    }

    private ObservableCollection<LogradouroDto> _logradouros = [];
    public ObservableCollection<LogradouroDto> Logradouros
    {
        get => _logradouros;
        set => SetProperty(ref _logradouros, value);
    }

    private LogradouroDto? _logradouroSelecionado;
    public LogradouroDto? LogradouroSelecionado
    {
        get => _logradouroSelecionado;
        set
        {
            if (SetProperty(ref _logradouroSelecionado, value) && value is not null)
                Colaborador.Endereco = value;
        }
    }

    private int _colaboradorId;
    public int ColaboradorId
    {
        get => _colaboradorId;
        set => SetProperty(ref _colaboradorId, value);
    }

    private bool _isEditMode;
    public bool IsEditMode
    {
        get => _isEditMode;
        set => SetProperty(ref _isEditMode, value);
    }

    public ColaboradorViewModel(IColaboradorService colaboradorService, ILogradouroService logradouroService)
    {
        _colaboradorService = colaboradorService;
        _logradouroService = logradouroService;
        Title = "Novo Colaborador";
    }

    private static ColaboradorDto CriarColaboradorVazio() => new()
    {
        Nome = string.Empty,
        Cpf = string.Empty,
        DataNascimento = DateOnly.FromDateTime(DateTime.Today.AddYears(-25)),
        Telefone = string.Empty,
        Numero = string.Empty,
        Email = string.Empty,
        Complemento = string.Empty,
        Senha = string.Empty,
        DataAdmissao = DateOnly.FromDateTime(DateTime.Today),
        Tipo = AppColaboradorTipo.Instrutor,
        Vinculo = AppColaboradorVinculo.CLT
    };

    public async Task InicializarAsync()
    {
        await CarregarLogradourosAsync();

        if (ColaboradorId > 0)
        {
            IsEditMode = true;
            Title = "Editar Colaborador";
            await CarregarColaboradorAsync();
        }
        else
        {
            IsEditMode = false;
            Title = "Novo Colaborador";
        }
    }

    private async Task CarregarLogradourosAsync()
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            Logradouros = [.. await _logradouroService.ObterTodosAsync(cts.Token)];
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao carregar logradouros: {ex.Message}", "OK");
        }
    }

    private async Task CarregarColaboradorAsync()
    {
        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            var dados = await _colaboradorService.ObterPorIdAsync(ColaboradorId, cts.Token);

            if (dados != null)
            {
                Colaborador = dados;
                LogradouroSelecionado = Logradouros.FirstOrDefault(l => l.Id == dados.Endereco?.Id);
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao carregar colaborador: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SelecionarFotoAsync()
    {
        try
        {
            string escolha = await Shell.Current.DisplayActionSheetAsync("Origem da Foto", "Cancelar", null, "Galeria", "Câmera", "Remover Foto");

            if (escolha == "Remover Foto")
            {
                Colaborador.Foto = null;
                OnPropertyChanged(nameof(Colaborador));
                return;
            }

            FileResult? resultado = escolha switch
            {
                "Galeria" => await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "Selecione a foto do colaborador",
                    FileTypes = FilePickerFileType.Images
                }),
                "Câmera" when MediaPicker.Default.IsCaptureSupported => await MediaPicker.Default.CapturePhotoAsync(),
                _ => null
            };

            if (resultado is null) return;

            using var stream = await resultado.OpenReadAsync();
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);

            Colaborador.Foto = new ArquivoDto { Conteudo = ms.ToArray() };
            OnPropertyChanged(nameof(Colaborador));
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao selecionar imagem: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task SalvarAsync()
    {
        if (IsBusy) return;
        if (!await ValidarAsync(Colaborador, IsEditMode)) return;

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));

            if (IsEditMode)
            {
                await _colaboradorService.AtualizarAsync(Colaborador, cts.Token);
                await Shell.Current.DisplayAlertAsync("Sucesso", "Colaborador atualizado com sucesso!", "OK");
            }
            else
            {
                await _colaboradorService.AdicionarAsync(Colaborador, cts.Token);
                await Shell.Current.DisplayAlertAsync("Sucesso", "Colaborador criado com sucesso!", "OK");
            }

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao salvar colaborador: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private static async Task CancelarAsync()
        => await Shell.Current.GoToAsync("..");

    private static async Task<bool> ValidarAsync(ColaboradorDto colaborador, bool edicao)
    {
        var erros = new List<string>();

        if (string.IsNullOrWhiteSpace(colaborador.Nome)) erros.Add("• Nome é obrigatório.");

        if (string.IsNullOrWhiteSpace(colaborador.Cpf)) erros.Add("• CPF é obrigatório.");
        else if (colaborador.Cpf.Count(char.IsDigit) != 11) erros.Add("• CPF deve conter 11 dígitos.");

        if (colaborador.DataNascimento == default) erros.Add("• Data de nascimento é obrigatória.");
        else if (colaborador.DataNascimento > DateOnly.FromDateTime(DateTime.Today)) erros.Add("• Data de nascimento não pode ser futura.");

        if (colaborador.DataAdmissao == default) erros.Add("• Data de admissão é obrigatória.");
        else if (colaborador.DataAdmissao > DateOnly.FromDateTime(DateTime.Today)) erros.Add("• Data de admissão não pode ser futura.");

        if (string.IsNullOrWhiteSpace(colaborador.Telefone)) erros.Add("• Telefone é obrigatório.");
        else if (colaborador.Telefone.Count(char.IsDigit) != 11) erros.Add("• Telefone deve conter DDD + 9 dígitos.");

        // O e-mail é obrigatório para colaboradores.
        if (string.IsNullOrWhiteSpace(colaborador.Email)) erros.Add("• E-mail é obrigatório para colaboradores.");

        if (colaborador.Endereco is null || colaborador.Endereco.Id <= 0) erros.Add("• Logradouro é obrigatório.");
        if (string.IsNullOrWhiteSpace(colaborador.Numero)) erros.Add("• Número do endereço é obrigatório.");

        // Administrador só pode ter vínculo CLT.
        if (colaborador.Tipo == AppColaboradorTipo.Administrador && colaborador.Vinculo != AppColaboradorVinculo.CLT)
            erros.Add("• Colaborador do tipo Administrador deve ter vínculo CLT.");

        if (!edicao && string.IsNullOrWhiteSpace(colaborador.Senha)) erros.Add("• Senha é obrigatória.");
        else if (!string.IsNullOrWhiteSpace(colaborador.Senha) && colaborador.Senha.Length < 6)
            erros.Add("• Senha deve ter no mínimo 6 caracteres, com letras e números.");

        if (erros.Count > 0)
        {
            await Shell.Current.DisplayAlertAsync("Erros de Validação",
                "Por favor, corrija os seguintes campos:\n\n" + string.Join("\n", erros), "OK");
            return false;
        }

        return true;
    }
}
