// gabriel geremias vieira
using AcademiaDoZe.Domain.Entities;

namespace AcademiaDoZe.Domain.Repositories;

public interface IAcessoColaboradorRepository : IRepository<AcessoColaborador>
{
    Task<IEnumerable<AcessoColaborador>> ObterPorColaboradorPeriodo(int colaboradorId, DateOnly inicio, DateOnly fim, CancellationToken cancellationToken = default);
    Task<AcessoColaborador?> ObterAcessoAbertoPorColaborador(int colaboradorId, CancellationToken cancellationToken = default);
    Task<TimeSpan> ObterHorasTrabalhadasNoDia(int colaboradorId, DateOnly data, CancellationToken cancellationToken = default);
}
