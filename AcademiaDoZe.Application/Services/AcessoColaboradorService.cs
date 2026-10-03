// gabriel geremias vieira
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.Repositories;

namespace AcademiaDoZe.Application.Services;

public class AcessoColaboradorService : IAcessoColaboradorService
{
    // Limites de jornada diária definidos pela regra de negócio.
    private static readonly TimeSpan LimiteClt = TimeSpan.FromHours(8);
    private static readonly TimeSpan LimiteEstagio = TimeSpan.FromHours(6);

    private readonly Func<IAcessoColaboradorRepository> _repoFactory;
    private readonly Func<IColaboradorRepository> _colaboradorRepoFactory;

    public AcessoColaboradorService(
        Func<IAcessoColaboradorRepository> repoFactory,
        Func<IColaboradorRepository> colaboradorRepoFactory)
    {
        _repoFactory = repoFactory ?? throw new ArgumentNullException(nameof(repoFactory));
        _colaboradorRepoFactory = colaboradorRepoFactory ?? throw new ArgumentNullException(nameof(colaboradorRepoFactory));
    }

    public async Task<AcessoColaboradorDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var acesso = await _repoFactory().ObterPorId(id, cancellationToken);
        return acesso?.ToDto();
    }

    public async Task<IEnumerable<AcessoColaboradorDto>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        var acessos = await _repoFactory().ObterTodos(cancellationToken);
        return [.. acessos.Select(a => a.ToDto())];
    }

    public async Task<IEnumerable<AcessoColaboradorDto>> ObterPorColaboradorPeriodoAsync(int colaboradorId, DateOnly inicio, DateOnly fim, CancellationToken cancellationToken = default)
    {
        var acessos = await _repoFactory().ObterPorColaboradorPeriodo(colaboradorId, inicio, fim, cancellationToken);
        return [.. acessos.Select(a => a.ToDto())];
    }

    public async Task<IEnumerable<AcessoColaboradorDto>> ObterPorDataAsync(DateOnly data, CancellationToken cancellationToken = default)
    {
        var acessos = await _repoFactory().ObterTodos(cancellationToken);
        return [.. acessos.Where(a => a.Data == data).Select(a => a.ToDto())];
    }

    public async Task<AcessoColaboradorDto?> ObterAcessoAbertoAsync(int colaboradorId, CancellationToken cancellationToken = default)
    {
        var acesso = await _repoFactory().ObterAcessoAbertoPorColaborador(colaboradorId, cancellationToken);
        return acesso?.ToDto();
    }

    public async Task<bool> EstaNaAcademiaAsync(int colaboradorId, CancellationToken cancellationToken = default)
    {
        var acesso = await _repoFactory().ObterAcessoAbertoPorColaborador(colaboradorId, cancellationToken);
        return acesso != null;
    }

    public async Task<TimeSpan> ObterHorasTrabalhadasNoDiaAsync(int colaboradorId, DateOnly data, CancellationToken cancellationToken = default)
    {
        return await _repoFactory().ObterHorasTrabalhadasNoDia(colaboradorId, data, cancellationToken);
    }

    public async Task<AcessoResultadoDto> RegistrarEntradaAsync(int colaboradorId, CancellationToken cancellationToken = default)
    {
        var colaborador = await _colaboradorRepoFactory().ObterPorId(colaboradorId, cancellationToken);
        if (colaborador == null)
        {
            return AcessoResultadoDto.Negado($"Colaborador com ID {colaboradorId} não encontrado.");
        }

        if (await EstaNaAcademiaAsync(colaboradorId, cancellationToken))
        {
            return AcessoResultadoDto.Negado("O colaborador já está na academia. Registre a saída antes de uma nova entrada.");
        }

        var agora = DateTime.Now;
        var hoje = DateOnly.FromDateTime(agora);

        // Regra: validar se já não ultrapassou o limite de 8h (CLT) ou 6h (estágio) no dia.
        var horasTrabalhadas = await _repoFactory().ObterHorasTrabalhadasNoDia(colaboradorId, hoje, cancellationToken);
        var limite = ObterLimiteJornada(colaborador.Vinculo);

        if (horasTrabalhadas >= limite)
        {
            return AcessoResultadoDto.Negado(
                $"Acesso negado: limite de jornada atingido ({limite:hh\\:mm}). Horas trabalhadas hoje: {horasTrabalhadas:hh\\:mm\\:ss}.");
        }

        var resultado = AcessoColaborador.Criar(0, colaborador, hoje, TimeOnly.FromDateTime(agora));
        if (resultado.IsFailure)
        {
            return AcessoResultadoDto.Negado(string.Join(", ", resultado.Notifications.Select(n => n.Mensagem)));
        }

        await _repoFactory().Adicionar(resultado.Value!, cancellationToken);

        return AcessoResultadoDto.Permitido(
            "Entrada Autorizada",
            $"Entrada registrada. Horas trabalhadas hoje até o momento: {horasTrabalhadas:hh\\:mm\\:ss}.");
    }

    public async Task<AcessoResultadoDto> RegistrarSaidaAsync(int colaboradorId, CancellationToken cancellationToken = default)
    {
        var acessoAberto = await _repoFactory().ObterAcessoAbertoPorColaborador(colaboradorId, cancellationToken);
        if (acessoAberto == null)
        {
            return AcessoResultadoDto.Negado("Não há entrada em aberto para este colaborador.");
        }

        var agora = TimeOnly.FromDateTime(DateTime.Now);
        if (agora < acessoAberto.Entrada)
        {
            return AcessoResultadoDto.Negado("A hora de saída não pode ser anterior à hora de entrada.");
        }

        acessoAberto.RegistrarSaida(agora);
        await _repoFactory().Atualizar(acessoAberto, cancellationToken);

        // Regra: na saída, somar todos os registros do dia.
        var totalDoDia = await _repoFactory().ObterHorasTrabalhadasNoDia(colaboradorId, acessoAberto.Data, cancellationToken);
        var permanencia = agora.ToTimeSpan() - acessoAberto.Entrada.ToTimeSpan();

        return AcessoResultadoDto.Permitido(
            "Saída Registrada",
            $"Saída registrada. Permanência: {permanencia:hh\\:mm\\:ss}. Total trabalhado hoje: {totalDoDia:hh\\:mm\\:ss}.");
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

    private static TimeSpan ObterLimiteJornada(ColaboradorVinculo vinculo) => vinculo switch
    {
        ColaboradorVinculo.CLT => LimiteClt,
        ColaboradorVinculo.Estagio => LimiteEstagio,
        _ => LimiteClt
    };
}
