// gabriel geremias vieira
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;

namespace AcademiaDoZe.Application.Services;

public class AcessoAlunoService : IAcessoAlunoService
{
    private readonly Func<IAcessoAlunoRepository> _repoFactory;
    private readonly Func<IAlunoRepository> _alunoRepoFactory;
    private readonly Func<IMatriculaRepository> _matriculaRepoFactory;

    public AcessoAlunoService(
        Func<IAcessoAlunoRepository> repoFactory,
        Func<IAlunoRepository> alunoRepoFactory,
        Func<IMatriculaRepository> matriculaRepoFactory)
    {
        _repoFactory = repoFactory ?? throw new ArgumentNullException(nameof(repoFactory));
        _alunoRepoFactory = alunoRepoFactory ?? throw new ArgumentNullException(nameof(alunoRepoFactory));
        _matriculaRepoFactory = matriculaRepoFactory ?? throw new ArgumentNullException(nameof(matriculaRepoFactory));
    }

    public async Task<AcessoAlunoDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var acesso = await _repoFactory().ObterPorId(id, cancellationToken);
        return acesso?.ToDto();
    }

    public async Task<IEnumerable<AcessoAlunoDto>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        var acessos = await _repoFactory().ObterTodos(cancellationToken);
        return [.. acessos.Select(a => a.ToDto())];
    }

    public async Task<IEnumerable<AcessoAlunoDto>> ObterPorAlunoPeriodoAsync(int alunoId, DateOnly inicio, DateOnly fim, CancellationToken cancellationToken = default)
    {
        var acessos = await _repoFactory().ObterPorAlunoPeriodo(alunoId, inicio, fim, cancellationToken);
        return [.. acessos.Select(a => a.ToDto())];
    }

    public async Task<IEnumerable<AcessoAlunoDto>> ObterPorDataAsync(DateOnly data, CancellationToken cancellationToken = default)
    {
        var acessos = await _repoFactory().ObterTodos(cancellationToken);
        return [.. acessos.Where(a => a.Data == data).Select(a => a.ToDto())];
    }

    public async Task<AcessoAlunoDto?> ObterAcessoAbertoAsync(int alunoId, CancellationToken cancellationToken = default)
    {
        var acesso = await _repoFactory().ObterAcessoAbertoPorAluno(alunoId, cancellationToken);
        return acesso?.ToDto();
    }

    public async Task<bool> EstaNaAcademiaAsync(int alunoId, CancellationToken cancellationToken = default)
    {
        return await _repoFactory().EstaNaAcademia(alunoId, cancellationToken);
    }

    public async Task<AcessoResultadoDto> RegistrarEntradaAsync(int alunoId, CancellationToken cancellationToken = default)
    {
        var aluno = await _alunoRepoFactory().ObterPorId(alunoId, cancellationToken);
        if (aluno == null)
        {
            return AcessoResultadoDto.Negado($"Aluno com ID {alunoId} não encontrado.");
        }

        // Regra: o aluno só entra se possuir matrícula ativa.
        var matriculaAtiva = await _matriculaRepoFactory().ObterMatriculaAtivaPorAluno(alunoId, cancellationToken);
        if (matriculaAtiva == null)
        {
            return AcessoResultadoDto.Negado("Acesso negado: o aluno não possui matrícula ativa.");
        }

        if (await _repoFactory().EstaNaAcademia(alunoId, cancellationToken))
        {
            return AcessoResultadoDto.Negado("O aluno já está na academia. Registre a saída antes de uma nova entrada.");
        }

        var agora = DateTime.Now;
        var resultado = AcessoAluno.Criar(0, aluno, DateOnly.FromDateTime(agora), TimeOnly.FromDateTime(agora));
        if (resultado.IsFailure)
        {
            return AcessoResultadoDto.Negado(string.Join(", ", resultado.Notifications.Select(n => n.Mensagem)));
        }

        await _repoFactory().Adicionar(resultado.Value!, cancellationToken);

        // Regra: na entrada, mostrar quanto tempo ainda resta de plano.
        int diasRestantes = matriculaAtiva.DataFim.DayNumber - DateOnly.FromDateTime(agora).DayNumber;

        return AcessoResultadoDto.Permitido(
            "Entrada Autorizada",
            $"Entrada autorizada. Dias restantes no plano: {diasRestantes} dia(s).");
    }

    public async Task<AcessoResultadoDto> RegistrarSaidaAsync(int alunoId, CancellationToken cancellationToken = default)
    {
        var acessoAberto = await _repoFactory().ObterAcessoAbertoPorAluno(alunoId, cancellationToken);
        if (acessoAberto == null)
        {
            return AcessoResultadoDto.Negado("Não há entrada em aberto para este aluno.");
        }

        var agora = TimeOnly.FromDateTime(DateTime.Now);
        if (agora < acessoAberto.Entrada)
        {
            return AcessoResultadoDto.Negado("A hora de saída não pode ser anterior à hora de entrada.");
        }

        acessoAberto.RegistrarSaida(agora);
        await _repoFactory().Atualizar(acessoAberto, cancellationToken);

        // Regra: na saída, mostrar o tempo que o aluno permaneceu na academia.
        var permanencia = agora.ToTimeSpan() - acessoAberto.Entrada.ToTimeSpan();

        return AcessoResultadoDto.Permitido(
            "Saída Registrada",
            $"Saída registrada. Tempo de permanência: {permanencia:hh\\:mm\\:ss}.");
    }

    public async Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        var acesso = await _repoFactory().ObterPorId(id, cancellationToken);
        if (acesso == null)
        {
            return false;
        }

        return await _repoFactory().Remover(id, cancellationToken);
    }
}
