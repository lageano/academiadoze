// gabriel geremias vieira
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

[QueryProperty(nameof(AlunoId), "Id")]
public partial class AlunoViewModel : BaseViewModel
{
    private readonly IAlunoService _alunoService;
    private readonly ILogradouroService _logradouroService;

    private AlunoDto _aluno = CriarAlunoVazio();

    public AlunoDto Aluno
    {
        get => _aluno;
        set => SetProperty(ref _aluno, value);
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
                Aluno.Endereco = value;
        }
    }

    private int _alunoId;
    public int AlunoId
    {
        get => _alunoId;
        set => SetProperty(ref _alunoId, value);
    }

    private bool _isEditMode;
    public bool IsEditMode
    {
        get => _isEditMode;
        set => SetProperty(ref _isEditMode, value);
    }

    public bool TemFoto => Aluno.Foto?.Conteudo is { Length: > 0 };

    public AlunoViewModel(IAlunoService alunoService, ILogradouroService logradouroService)
    {
        _alunoService = alunoService;
        _logradouroService = logradouroService;
        Title = "Novo Aluno";
    }

    private static AlunoDto CriarAlunoVazio() => new()
    {
        Nome = string.Empty,
        Cpf = string.Empty,
        DataNascimento = DateOnly.FromDateTime(DateTime.Today.AddYears(-18)),
        Telefone = string.Empty,
        Numero = string.Empty,
        Email = string.Empty,
        Complemento = string.Empty,
        Senha = string.Empty
    };

    public async Task InicializarAsync()
    {
        await CarregarLogradourosAsync();

        if (AlunoId > 0)
        {
            IsEditMode = true;
            Title = "Editar Aluno";
            await CarregarAlunoAsync();
        }
        else
        {
            IsEditMode = false;
            Title = "Novo Aluno";
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

    private async Task CarregarAlunoAsync()
    {
        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            var dados = await _alunoService.ObterPorIdAsync(AlunoId, cts.Token);

            if (dados != null)
            {
                Aluno = dados;
                LogradouroSelecionado = Logradouros.FirstOrDefault(l => l.Id == dados.Endereco?.Id);
                OnPropertyChanged(nameof(TemFoto));
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao carregar aluno: {ex.Message}", "OK");
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
                Aluno.Foto = null;
                OnPropertyChanged(nameof(Aluno));
                OnPropertyChanged(nameof(TemFoto));
                return;
            }

            FileResult? resultado = escolha switch
            {
                "Galeria" => await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "Selecione a foto do aluno",
                    FileTypes = FilePickerFileType.Images
                }),
                "Câmera" when MediaPicker.Default.IsCaptureSupported => await MediaPicker.Default.CapturePhotoAsync(),
                _ => null
            };

            if (resultado is null) return;

            using var stream = await resultado.OpenReadAsync();
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);

            Aluno.Foto = new ArquivoDto { Conteudo = ms.ToArray() };
            OnPropertyChanged(nameof(Aluno));
            OnPropertyChanged(nameof(TemFoto));
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
        if (!await ValidarAsync(Aluno, IsEditMode)) return;

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));

            if (IsEditMode)
            {
                await _alunoService.AtualizarAsync(Aluno, cts.Token);
                await Shell.Current.DisplayAlertAsync("Sucesso", "Aluno atualizado com sucesso!", "OK");
            }
            else
            {
                await _alunoService.AdicionarAsync(Aluno, cts.Token);
                await Shell.Current.DisplayAlertAsync("Sucesso", "Aluno criado com sucesso!", "OK");
            }

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao salvar aluno: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private static async Task CancelarAsync()
        => await Shell.Current.GoToAsync("..");

    private static async Task<bool> ValidarAsync(AlunoDto aluno, bool edicao)
    {
        var erros = new List<string>();

        if (string.IsNullOrWhiteSpace(aluno.Nome)) erros.Add("• Nome é obrigatório.");

        if (string.IsNullOrWhiteSpace(aluno.Cpf)) erros.Add("• CPF é obrigatório.");
        else if (aluno.Cpf.Count(char.IsDigit) != 11) erros.Add("• CPF deve conter 11 dígitos.");

        if (aluno.DataNascimento == default) erros.Add("• Data de nascimento é obrigatória.");
        else if (aluno.DataNascimento > DateOnly.FromDateTime(DateTime.Today)) erros.Add("• Data de nascimento não pode ser futura.");

        if (string.IsNullOrWhiteSpace(aluno.Telefone)) erros.Add("• Telefone é obrigatório.");
        else if (aluno.Telefone.Count(char.IsDigit) != 11) erros.Add("• Telefone deve conter DDD + 9 dígitos.");

        if (aluno.Endereco is null || aluno.Endereco.Id <= 0) erros.Add("• Logradouro é obrigatório.");
        if (string.IsNullOrWhiteSpace(aluno.Numero)) erros.Add("• Número do endereço é obrigatório.");

        // Na edição a senha é opcional: em branco mantém a senha atual.
        if (!edicao && string.IsNullOrWhiteSpace(aluno.Senha)) erros.Add("• Senha é obrigatória.");
        else if (!string.IsNullOrWhiteSpace(aluno.Senha) && aluno.Senha.Length < 6)
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
