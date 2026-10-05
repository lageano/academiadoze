// gabriel geremias vieira
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class AcessoViewModel : BaseViewModel
{
    private readonly IAlunoService _alunoService;
    private readonly IColaboradorService _colaboradorService;
    private readonly IAcessoAlunoService _acessoAlunoService;
    private readonly IAcessoColaboradorService _acessoColaboradorService;

    public AcessoViewModel(
        IAlunoService alunoService,
        IColaboradorService colaboradorService,
        IAcessoAlunoService acessoAlunoService,
        IAcessoColaboradorService acessoColaboradorService)
    {
        _alunoService = alunoService;
        _colaboradorService = colaboradorService;
        _acessoAlunoService = acessoAlunoService;
        _acessoColaboradorService = acessoColaboradorService;
        Title = "Controle de Acesso";
    }

    #region Modo (Aluno / Colaborador)

    private bool _modoAluno = true;
    public bool ModoAluno
    {
        get => _modoAluno;
        set
        {
            if (SetProperty(ref _modoAluno, value))
            {
                OnPropertyChanged(nameof(ModoColaborador));
                OnPropertyChanged(nameof(ModoAtualTexto));
                OnPropertyChanged(nameof(RotuloSelecao));
            }
        }
    }

    public bool ModoColaborador => !ModoAluno;
    public string ModoAtualTexto => $"Modo atual selecionado: {(ModoAluno ? "Aluno" : "Colaborador")}";
    public string RotuloSelecao => ModoAluno ? "Selecione o Aluno:" : "Selecione o Colaborador:";

    [RelayCommand]
    private async Task TrocarModoAsync(string modo)
    {
        ModoAluno = modo == "aluno";
        LimparSelecao();
        await BuscarPessoasAsync();
    }

    #endregion

    #region Busca e seleção

    private string _textoBusca = string.Empty;
    public string TextoBusca
    {
        get => _textoBusca;
        set => SetProperty(ref _textoBusca, value);
    }

    private ObservableCollection<PessoaAcessoItem> _pessoas = [];
    public ObservableCollection<PessoaAcessoItem> Pessoas
    {
        get => _pessoas;
        set => SetProperty(ref _pessoas, value);
    }

    private PessoaAcessoItem? _pessoaSelecionada;
    public PessoaAcessoItem? PessoaSelecionada
    {
        get => _pessoaSelecionada;
        set
        {
            if (SetProperty(ref _pessoaSelecionada, value))
            {
                OnPropertyChanged(nameof(TemPessoaSelecionada));
                _ = AtualizarPresencaAsync();
            }
        }
    }

    public bool TemPessoaSelecionada => PessoaSelecionada is not null;

    [RelayCommand]
    public async Task BuscarPessoasAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            var termo = TextoBusca?.Trim() ?? string.Empty;

            var encontrados = ModoAluno
                ? await BuscarAlunosAsync(termo, cts.Token)
                : await BuscarColaboradoresAsync(termo, cts.Token);

            Pessoas = [.. encontrados];
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao buscar: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task<List<PessoaAcessoItem>> BuscarAlunosAsync(string termo, CancellationToken ct)
    {
        var alunos = (await _alunoService.ObterTodosAsync(ct)).ToList();

        if (!string.IsNullOrWhiteSpace(termo))
        {
            var somenteDigitos = new string(termo.Where(char.IsDigit).ToArray());
            alunos = [.. alunos.Where(a =>
                a.Nome.Contains(termo, StringComparison.OrdinalIgnoreCase)
                || (somenteDigitos.Length > 0 && a.Cpf.Contains(somenteDigitos)))];
        }

        return [.. alunos.Select(a => new PessoaAcessoItem
        {
            Id = a.Id,
            Nome = a.Nome,
            Cpf = a.Cpf,
            Foto = a.Foto?.Conteudo
        })];
    }

    private async Task<List<PessoaAcessoItem>> BuscarColaboradoresAsync(string termo, CancellationToken ct)
    {
        var colaboradores = (await _colaboradorService.ObterTodosAsync(ct)).ToList();

        if (!string.IsNullOrWhiteSpace(termo))
        {
            var somenteDigitos = new string(termo.Where(char.IsDigit).ToArray());
            colaboradores = [.. colaboradores.Where(c =>
                c.Nome.Contains(termo, StringComparison.OrdinalIgnoreCase)
                || (somenteDigitos.Length > 0 && c.Cpf.Contains(somenteDigitos)))];
        }

        return [.. colaboradores.Select(c => new PessoaAcessoItem
        {
            Id = c.Id,
            Nome = c.Nome,
            Cpf = c.Cpf,
            Foto = c.Foto?.Conteudo
        })];
    }

    private void LimparSelecao()
    {
        PessoaSelecionada = null;
        Pessoas = [];
        MensagemResultado = string.Empty;
        TituloResultado = string.Empty;
    }

    #endregion

    #region Presença e registro

    private bool _estaNaAcademia;
    public bool EstaNaAcademia
    {
        get => _estaNaAcademia;
        set
        {
            if (SetProperty(ref _estaNaAcademia, value))
                OnPropertyChanged(nameof(TextoBotaoAcesso));
        }
    }

    public string TextoBotaoAcesso => EstaNaAcademia ? "Registrar Saída" : "Registrar Entrada";

    private string _tituloResultado = string.Empty;
    public string TituloResultado
    {
        get => _tituloResultado;
        set
        {
            if (SetProperty(ref _tituloResultado, value))
                OnPropertyChanged(nameof(TemResultado));
        }
    }

    private string _mensagemResultado = string.Empty;
    public string MensagemResultado
    {
        get => _mensagemResultado;
        set => SetProperty(ref _mensagemResultado, value);
    }

    private bool _resultadoAutorizado;
    public bool ResultadoAutorizado
    {
        get => _resultadoAutorizado;
        set => SetProperty(ref _resultadoAutorizado, value);
    }

    public bool TemResultado => !string.IsNullOrWhiteSpace(TituloResultado);

    private async Task AtualizarPresencaAsync()
    {
        if (PessoaSelecionada is null)
        {
            EstaNaAcademia = false;
            return;
        }

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            EstaNaAcademia = ModoAluno
                ? await _acessoAlunoService.EstaNaAcademiaAsync(PessoaSelecionada.Id, cts.Token)
                : await _acessoColaboradorService.EstaNaAcademiaAsync(PessoaSelecionada.Id, cts.Token);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao verificar presença: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task RegistrarAcessoAsync()
    {
        if (IsBusy) return;

        if (PessoaSelecionada is null)
        {
            await Shell.Current.DisplayAlertAsync("Aviso", "Selecione uma pessoa antes de registrar o acesso.", "OK");
            return;
        }

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            int id = PessoaSelecionada.Id;

            var resultado = ModoAluno
                ? (EstaNaAcademia
                    ? await _acessoAlunoService.RegistrarSaidaAsync(id, cts.Token)
                    : await _acessoAlunoService.RegistrarEntradaAsync(id, cts.Token))
                : (EstaNaAcademia
                    ? await _acessoColaboradorService.RegistrarSaidaAsync(id, cts.Token)
                    : await _acessoColaboradorService.RegistrarEntradaAsync(id, cts.Token));

            TituloResultado = resultado.Titulo;
            MensagemResultado = resultado.Mensagem;
            ResultadoAutorizado = resultado.Autorizado;

            await AtualizarPresencaAsync();
            await CarregarAcessosDeHojeAsync();
        }
        catch (Exception ex)
        {
            TituloResultado = "Erro";
            MensagemResultado = ex.Message;
            ResultadoAutorizado = false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    #endregion

    #region Acessos de hoje

    private ObservableCollection<AcessoHojeItem> _acessosHoje = [];
    public ObservableCollection<AcessoHojeItem> AcessosHoje
    {
        get => _acessosHoje;
        set => SetProperty(ref _acessosHoje, value);
    }

    [RelayCommand]
    public async Task CarregarAcessosDeHojeAsync()
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            var hoje = DateOnly.FromDateTime(DateTime.Today);

            var alunos = await _acessoAlunoService.ObterPorDataAsync(hoje, cts.Token);
            var colaboradores = await _acessoColaboradorService.ObterPorDataAsync(hoje, cts.Token);

            var itens = new List<(TimeOnly Ordem, AcessoHojeItem Item)>();

            foreach (var acesso in alunos)
            {
                var hora = acesso.Saida ?? acesso.Entrada;
                var descricao = acesso.Saida.HasValue
                    ? $"Saída registrada. Tempo de permanência: {acesso.TempoPermanencia:hh\\:mm\\:ss}."
                    : "Entrada autorizada.";

                itens.Add((hora, new AcessoHojeItem
                {
                    Nome = acesso.Aluno.Nome,
                    Descricao = descricao,
                    Hora = hora.ToString("HH:mm:ss")
                }));
            }

            foreach (var acesso in colaboradores)
            {
                var hora = acesso.Saida ?? acesso.Entrada;
                var descricao = acesso.Saida.HasValue
                    ? $"Saída registrada. Permanência: {acesso.TempoPermanencia:hh\\:mm\\:ss}."
                    : "Entrada registrada.";

                itens.Add((hora, new AcessoHojeItem
                {
                    Nome = acesso.Colaborador.Nome,
                    Descricao = descricao,
                    Hora = hora.ToString("HH:mm:ss")
                }));
            }

            AcessosHoje = [.. itens.OrderByDescending(i => i.Ordem).Select(i => i.Item)];
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao carregar acessos de hoje: {ex.Message}", "OK");
        }
    }

    #endregion
}
