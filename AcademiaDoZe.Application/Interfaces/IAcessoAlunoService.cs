// gabriel geremias vieira
using AcademiaDoZe.Application.DTOs;

namespace AcademiaDoZe.Application.Interfaces;

/// <summary>
/// Contrato de serviço para o controle de entrada e saída de Alunos (catraca).
/// </summary>
public interface IAcessoAlunoService
{
    /// <summary>
    /// Obtém um registro de acesso pelo seu ID.
    /// </summary>
    Task<AcessoAlunoDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém todos os registros de acesso de alunos.
    /// </summary>
    Task<IEnumerable<AcessoAlunoDto>> ObterTodosAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém os acessos de um aluno dentro de um período.
    /// </summary>
    Task<IEnumerable<AcessoAlunoDto>> ObterPorAlunoPeriodoAsync(int alunoId, DateOnly inicio, DateOnly fim, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém os acessos registrados na data informada.
    /// </summary>
    Task<IEnumerable<AcessoAlunoDto>> ObterPorDataAsync(DateOnly data, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém o acesso em aberto do aluno, ou seja, com entrada registrada e sem saída.
    /// </summary>
    Task<AcessoAlunoDto?> ObterAcessoAbertoAsync(int alunoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Indica se o aluno está dentro da academia no momento.
    /// </summary>
    Task<bool> EstaNaAcademiaAsync(int alunoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registra a entrada do aluno, validando se ele possui matrícula ativa.
    /// </summary>
    Task<AcessoResultadoDto> RegistrarEntradaAsync(int alunoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registra a saída do aluno e informa o tempo de permanência.
    /// </summary>
    Task<AcessoResultadoDto> RegistrarSaidaAsync(int alunoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove um registro de acesso pelo ID.
    /// </summary>
    Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default);
}
