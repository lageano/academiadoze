// gabriel geremias vieira
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

[QueryProperty(nameof(MatriculaId), "Id")]
public partial class MatriculaViewModel : BaseViewModel
{
    private const int IdadeLimiteLaudo = 16;

    private readonly IMatriculaService _matriculaService;
    private readonly IAlunoService _alunoService;

    public IReadOnlyList<AppMatriculaPlano> PlanoTipos { get; } = [.. Enum.GetValues<AppMatriculaPlano>()];

    // Itens para múltipla seleção (CheckBoxes) das restrições médicas (flags)
    public ObservableCollection<RestricaoSelectable> RestricoesMulti { get; } = [];

    // Evita write-back enquanto a seleção está sendo sincronizada a partir do DTO
    private bool _suspendSelectionSync;

    private MatriculaDto _matricula = CriarMatriculaVazia();
    public MatriculaDto Matricula
    {
        get => _matricula;
        set
        {
            if (SetProperty(ref _matricula, value))
                SyncSelectionFromFlags();
        }
    }

    private int _matriculaId;
    public int MatriculaId
    {
        get => _matriculaId;
        set => SetProperty(ref _matriculaId, value);
    }

    private bool _isEditMode;
    public bool IsEditMode
    {
        get => _isEditMode;
        set => SetProperty(ref _isEditMode, value);
    }

    private string _cpfBusca = string.Empty;
    public string CpfBusca
    {
        get => _cpfBusca;
        set => SetProperty(ref _cpfBusca, value);
    }

    #region Propriedades visuais derivadas

    public bool HasAlunoVinculado => Matricula?.AlunoMatricula != null && Matricula.AlunoMatricula.Id > 0;
    public bool HasNoAlunoVinculado => !HasAlunoVinculado;

    public bool IsMenor16 => Matricula?.AlunoMatricula != null
        && Matricula.AlunoMatricula.DataNascimento != default
        && Matricula.AlunoMatricula.DataNascimento > DateOnly.FromDateTime(DateTime.Today.AddYears(-IdadeLimiteLaudo));

    public bool HasRestricoes => Matricula != null && Matricula.RestricoesMedicas != AppMatriculaRestricoes.None;
    public bool ExigeLaudo => IsMenor16 || HasRestricoes;
    public bool HasLaudo => Matricula?.LaudoMedico?.Conteudo is { Length: > 0 };
    public bool LaudoPendente => ExigeLaudo && !HasLaudo;

    public void NotificarPropriedadesVisuais()
    {
        OnPropertyChanged(nameof(HasAlunoVinculado));
        OnPropertyChanged(nameof(HasNoAlunoVinculado));
        OnPropertyChanged(nameof(IsMenor16));
        OnPropertyChanged(nameof(HasRestricoes));
        OnPropertyChanged(nameof(ExigeLaudo));
        OnPropertyChanged(nameof(HasLaudo));
        OnPropertyChanged(nameof(LaudoPendente));
    }

    #endregion

    public MatriculaViewModel(IMatriculaService matriculaService, IAlunoService alunoService)
    {
        _matriculaService = matriculaService;
        _alunoService = alunoService;
        Title = "Detalhes da Matrícula";
        BuildRestricoesMulti();
    }

    private static MatriculaDto CriarMatriculaVazia() => new()
    {
        AlunoMatricula = new AlunoDto
        {
            Nome = string.Empty,
            Cpf = string.Empty,
            DataNascimento = DateOnly.FromDateTime(DateTime.Today.AddYears(-18)),
            Telefone = string.Empty,
            Numero = string.Empty
        },
        Plano = AppMatriculaPlano.Anual,
        DataInicio = DateOnly.FromDateTime(DateTime.Today),
        DataFim = DateOnly.FromDateTime(DateTime.Today.AddYears(1)),
        Objetivo = string.Empty,
        RestricoesMedicas = AppMatriculaRestricoes.None,
        ObservacoesRestricoes = string.Empty,
        LaudoMedico = null
    };

    public async Task InicializarAsync()
    {
        if (MatriculaId > 0)
        {
            IsEditMode = true;
            Title = "Editar Matrícula";
            await CarregarMatriculaAsync();
        }
        else
        {
            IsEditMode = false;
            Title = "Nova Matrícula";
            SyncSelectionFromFlags();
        }

        NotificarPropriedadesVisuais();
    }

    #region enum Flags <-> seleção múltipla (CheckBoxes)

    // Monta a lista de checkboxes a partir do enum (ignora None)
    private void BuildRestricoesMulti()
    {
        RestricoesMulti.Clear();

        foreach (var value in Enum.GetValues<AppMatriculaRestricoes>())
        {
            if (value == AppMatriculaRestricoes.None) continue;

            var item = new RestricaoSelectable
            {
                Value = value,
                IsSelected = Matricula.RestricoesMedicas.HasFlag(value)
            };

            item.PropertyChanged += RestricaoItem_PropertyChanged;
            RestricoesMulti.Add(item);
        }
    }

    // Atualiza o DTO quando o usuário marca ou desmarca
    private void RestricaoItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(RestricaoSelectable.IsSelected)) return;
        if (_suspendSelectionSync) return;

        RecomputeRestricoesFlags();
    }

    // Recalcula a bitmask a partir das seleções
    private void RecomputeRestricoesFlags()
    {
        var flags = AppMatriculaRestricoes.None;

        foreach (var item in RestricoesMulti)
        {
            if (item.IsSelected) flags |= item.Value;
        }

        Matricula.RestricoesMedicas = flags;
        OnPropertyChanged(nameof(Matricula));
        NotificarPropriedadesVisuais();
    }

    // Sincroniza a seleção visual a partir do DTO (ao carregar ou substituir a matrícula)
    private void SyncSelectionFromFlags()
    {
        _suspendSelectionSync = true;
        try
        {
            foreach (var item in RestricoesMulti)
            {
                item.IsSelected = Matricula.RestricoesMedicas.HasFlag(item.Value);
            }
        }
        finally
        {
            _suspendSelectionSync = false;
        }

        NotificarPropriedadesVisuais();
    }

    #endregion

    private async Task CarregarMatriculaAsync()
    {
        if (MatriculaId <= 0) return;

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            var dados = await _matriculaService.ObterPorIdAsync(MatriculaId, cts.Token);

            if (dados != null)
            {
                Matricula = dados;
                CpfBusca = dados.AlunoMatricula.Cpf;
                SyncSelectionFromFlags();
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao carregar matrícula: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task BuscarPorCpfAsync()
    {
        if (string.IsNullOrWhiteSpace(CpfBusca))
        {
            await Shell.Current.DisplayAlertAsync("Aviso", "Informe o CPF para realizar a busca.", "OK");
            return;
        }

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            var cpfLimpo = new string(CpfBusca.Where(char.IsDigit).ToArray());

            var alunoData = await _alunoService.ObterPorCpfAsync(cpfLimpo, cts.Token);

            if (alunoData != null)
            {
                Matricula.AlunoMatricula = alunoData;
                OnPropertyChanged(nameof(Matricula));
                NotificarPropriedadesVisuais();
                await Shell.Current.DisplayAlertAsync("Aviso", "Aluno encontrado! Dados preenchidos automaticamente.", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Aviso", "Aluno não encontrado.", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao buscar CPF: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SelecionarLaudoAsync()
    {
        try
        {
            string escolha = await Shell.Current.DisplayActionSheetAsync("Origem do Laudo Médico", "Cancelar", null, "Galeria", "Câmera", "Remover Laudo");

            if (escolha == "Remover Laudo")
            {
                Matricula.LaudoMedico = null;
                OnPropertyChanged(nameof(Matricula));
                NotificarPropriedadesVisuais();
                return;
            }

            FileResult? resultado = escolha switch
            {
                "Galeria" => await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "Selecione o Laudo Médico",
                    FileTypes = FilePickerFileType.Images
                }),
                "Câmera" when MediaPicker.Default.IsCaptureSupported => await MediaPicker.Default.CapturePhotoAsync(),
                _ => null
            };

            if (resultado is null) return;

            using var stream = await resultado.OpenReadAsync();
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);

            Matricula.LaudoMedico = new ArquivoDto { Conteudo = ms.ToArray() };
            OnPropertyChanged(nameof(Matricula));
            NotificarPropriedadesVisuais();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao selecionar imagem: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private void RemoverLaudo()
    {
        Matricula.LaudoMedico = null;
        OnPropertyChanged(nameof(Matricula));
        NotificarPropriedadesVisuais();
    }

    [RelayCommand]
    private async Task SalvarAsync()
    {
        if (IsBusy) return;

        // garante que o DTO está com os flags corretos
        RecomputeRestricoesFlags();

        if (!await ValidarAsync(Matricula)) return;

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));

            if (IsEditMode)
            {
                await _matriculaService.AtualizarAsync(Matricula, cts.Token);
                await Shell.Current.DisplayAlertAsync("Sucesso", "Matrícula atualizada com sucesso!", "OK");
            }
            else
            {
                await _matriculaService.AdicionarAsync(Matricula, cts.Token);
                await Shell.Current.DisplayAlertAsync("Sucesso", "Matrícula criada com sucesso!", "OK");
            }

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao salvar matrícula: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private static async Task CancelarAsync()
        => await Shell.Current.GoToAsync("..");

    /*
        Validação utilizando o padrão Notification Pattern, retornando uma lista de erros para o usuário,
        em vez de fail-fast exceptions. Assim o usuário vê todos os problemas de uma vez.
    */
    private static async Task<bool> ValidarAsync(MatriculaDto matricula)
    {
        var erros = new List<string>();

        if (matricula.AlunoMatricula == null || matricula.AlunoMatricula.Id <= 0)
            erros.Add("• Aluno é obrigatório. Informe o CPF e realize a busca.");

        if (matricula.DataInicio == default)
            erros.Add("• Data de início é obrigatória.");

        if (string.IsNullOrWhiteSpace(matricula.Objetivo))
            erros.Add("• Objetivo da matrícula é obrigatório.");

        if (!Enum.IsDefined(matricula.Plano))
            erros.Add("• Plano da matrícula é inválido.");

        bool menor16 = matricula.AlunoMatricula != null
            && matricula.AlunoMatricula.DataNascimento != default
            && matricula.AlunoMatricula.DataNascimento > DateOnly.FromDateTime(DateTime.Today.AddYears(-IdadeLimiteLaudo));

        bool possuiLaudo = matricula.LaudoMedico?.Conteudo is { Length: > 0 };

        if (menor16 && !possuiLaudo)
            erros.Add("• Alunos menores de 16 anos devem obrigatoriamente apresentar laudo médico.");

        if (matricula.RestricoesMedicas != AppMatriculaRestricoes.None && !possuiLaudo)
            erros.Add("• Alunos com restrições médicas cadastradas devem obrigatoriamente apresentar laudo médico.");

        if (erros.Count > 0)
        {
            await Shell.Current.DisplayAlertAsync("Erros de Validação",
                "Por favor, corrija os seguintes campos:\n\n" + string.Join("\n", erros), "OK");
            return false;
        }

        return true;
    }
}
