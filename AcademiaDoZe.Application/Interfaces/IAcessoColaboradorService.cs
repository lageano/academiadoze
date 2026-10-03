// gabriel geremias vieira
using AcademiaDoZe.Application.DTOs;

namespace AcademiaDoZe.Application.Interfaces;

/// <summary>
/// Contrato de serviço para o controle de entrada e saída de Colaboradores (catraca).
/// </summary>
public interface IAcessoColaboradorService
{
    /// <summary>
    /// Obtém um registro de acesso pelo seu ID.
    /// </summary>
    Task<AcessoColaboradorDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém todos os registros de acesso de colaboradores.
    /// </summary>
    Task<IEnumerable<AcessoColaboradorDto>> ObterTodosAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém os acessos de um colaborador dentro de um período.
    /// </summary>
    Task<IEnumerable<AcessoColaboradorDto>> ObterPorColaboradorPeriodoAsync(int colaboradorId, DateOnly inicio, DateOnly fim, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém os acessos registrados na data informada.
    /// </summary>
    Task<IEnumerable<AcessoColaboradorDto>> ObterPorDataAsync(DateOnly data, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém o acesso em aberto do colaborador, ou seja, com entrada registrada e sem saída.
    /// </summary>
    Task<AcessoColaboradorDto?> ObterAcessoAbertoAsync(int colaboradorId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Indica se o colaborador está dentro da academia no momento.
    /// </summary>
    Task<bool> EstaNaAcademiaAsync(int colaboradorId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém o total de horas já trabalhadas pelo colaborador na data informada.
    /// </summary>
    Task<TimeSpan> ObterHorasTrabalhadasNoDiaAsync(int colaboradorId, DateOnly data, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registra a entrada do colaborador, validando o limite de jornada (8h CLT / 6h estágio).
    /// </summary>
    Task<AcessoResultadoDto> RegistrarEntradaAsync(int colaboradorId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registra a saída do colaborador e informa o total de horas trabalhadas no dia.
    /// </summary>
    Task<AcessoResultadoDto> RegistrarSaidaAsync(int colaboradorId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove um registro de acesso pelo ID.
    /// </summary>
    Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default);
}
