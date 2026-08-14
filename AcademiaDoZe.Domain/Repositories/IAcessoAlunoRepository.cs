// gabriel geremias vieira
using AcademiaDoZe.Domain.Entities;

namespace AcademiaDoZe.Domain.Repositories;

public interface IAcessoAlunoRepository : IRepository<AcessoAluno>
{
    Task<IEnumerable<AcessoAluno>> ObterPorAlunoPeriodo(int alunoId, DateOnly inicio, DateOnly fim, CancellationToken cancellationToken = default);
    Task<AcessoAluno?> ObterAcessoAbertoPorAluno(int alunoId, CancellationToken cancellationToken = default);
    Task<bool> EstaNaAcademia(int alunoId, CancellationToken cancellationToken = default);
}
