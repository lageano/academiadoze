using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Exceptions;
using System.Data;
using System.Data.Common;

namespace AcademiaDoZe.Infrastructure.Repositories;

public class AcessoColaboradorRepository : BaseRepository, IAcessoColaboradorRepository
{
    public AcessoColaboradorRepository(string connectionString, DatabaseType databaseType) : base(connectionString, databaseType)
    {
    }

    private static string BaseSelectQuery => @"
        SELECT
            ac.id_acesso_colaborador, ac.colaborador_id, ac.data, ac.entrada, ac.saida,
            c.id_colaborador, c.cpf, c.nome, c.nascimento, c.telefone, c.email,
            c.logradouro_id, c.numero, c.complemento, c.senha, c.foto, c.admissao, c.tipo, c.vinculo,
            l.id_logradouro, l.cep, l.nome AS logradouro_nome, l.bairro, l.cidade, l.estado, l.pais
        FROM tb_acesso_colaborador ac
        INNER JOIN tb_colaborador c ON ac.colaborador_id = c.id_colaborador
        INNER JOIN tb_logradouro l ON c.logradouro_id = l.id_logradouro";

    public async Task<AcessoColaborador?> ObterPorId(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelectQuery} WHERE ac.id_acesso_colaborador = @Id";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Id", id, DbType.Int32);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_POR_ID", $"Erro ao obter acesso do colaborador por ID {id}: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<AcessoColaborador>> ObterTodos(CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelectQuery} ORDER BY ac.data DESC, ac.entrada DESC";
            await using var command = await CreateCommandAsync(query, cancellationToken);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var acessos = new List<AcessoColaborador>();
            while (await reader.ReadAsync(cancellationToken))
            {
                acessos.Add(Map(reader));
            }

            return acessos;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_TODOS", $"Erro ao obter todos os acessos de colaboradores: {ex.Message}", ex);
        }
    }

    public static AcessoColaborador Map(DbDataReader reader)
    {
        int id = 0;
        try
        {
            id = reader.GetInt32Value("id_acesso_colaborador");
            DateOnly data = reader.GetDateOnlyValue("data");
            TimeOnly entrada = reader.GetTimeOnlyValue("entrada");
            TimeOnly? saida = reader.GetNullableTimeOnly("saida");

            var colaborador = ColaboradorRepository.Map(reader);

            var result = AcessoColaborador.Criar(id, colaborador, data, entrada);
            if (result.IsFailure)
            {
                throw new InfrastructureException("ERRO_DOMINIO_MAPEAMENTO", $"Erro de domínio ao mapear acesso do colaborador ID {id}: {string.Join(", ", result.Notifications.Select(n => n.Mensagem))}");
            }

            var acesso = result.Value!;
            if (saida.HasValue)
            {
                acesso.RegistrarSaida(saida.Value);
            }

            return acesso;
        }
        catch (Exception ex) when (ex is not InfrastructureException)
        {
            throw new InfrastructureException("ERRO_MAPEAMENTO_ACESSO_COLABORADOR", $"Erro ao mapear dados do acesso do colaborador ID {id}: {ex.Message}", ex);
        }
    }

    public async Task<AcessoColaborador> Adicionar(AcessoColaborador entity, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = FormatInsertQuery("INSERT INTO tb_acesso_colaborador (colaborador_id, data, entrada, saida) VALUES (@ColaboradorId, @Data, @Entrada, @Saida)");

            await using var command = await CreateCommandAsync(query, cancellationToken);
            AdicionarParametrosComuns(command, entity);

            int id = await command.ExecuteScalarIdAsync("ERRO_ADICIONAR_ACESSO_COLABORADOR", "Falha ao obter ID inserido para o acesso do colaborador.", cancellationToken);
            var idProperty = typeof(Entity).GetProperty("Id");
            idProperty?.SetValue(entity, id);

            return entity;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_ADICIONAR_ACESSO_COLABORADOR", $"Erro ao adicionar acesso do colaborador: {ex.Message}", ex);
        }
    }

    public async Task<AcessoColaborador> Atualizar(AcessoColaborador entity, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = "UPDATE tb_acesso_colaborador SET colaborador_id = @ColaboradorId, data = @Data, entrada = @Entrada, saida = @Saida WHERE id_acesso_colaborador = @Id";

            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Id", entity.Id, DbType.Int32);
            AdicionarParametrosComuns(command, entity);

            int rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);

            if (rowsAffected == 0)
            {
                throw new InfrastructureException("REGISTRO_NAO_ENCONTRADO", $"Nenhum acesso de colaborador encontrado com o ID {entity.Id} para atualização.");
            }

            return entity;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_ATUALIZAR_ACESSO_COLABORADOR", $"Erro ao atualizar acesso do colaborador ID {entity.Id}: {ex.Message}", ex);
        }
    }

    private static void AdicionarParametrosComuns(DbCommand command, AcessoColaborador entity)
    {
        command.AddParameter("@ColaboradorId", entity.Colaborador.Id, DbType.Int32);
        command.AddParameter("@Data", entity.Data.ToDateTime(TimeOnly.MinValue), DbType.Date);
        command.AddParameter("@Entrada", entity.Entrada.ToTimeSpan(), DbType.Time);
        command.AddParameter("@Saida", (object?)entity.Saida?.ToTimeSpan() ?? DBNull.Value, DbType.Time);
    }

    public async Task<bool> Remover(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = "DELETE FROM tb_acesso_colaborador WHERE id_acesso_colaborador = @Id";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Id", id, DbType.Int32);

            var result = await command.ExecuteNonQueryAsync(cancellationToken);
            return result > 0;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_REMOVER_ACESSO_COLABORADOR", $"Erro ao remover acesso do colaborador ID {id}: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<AcessoColaborador>> ObterPorColaboradorPeriodo(int colaboradorId, DateOnly inicio, DateOnly fim, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelectQuery} WHERE ac.colaborador_id = @ColaboradorId AND ac.data >= @Inicio AND ac.data <= @Fim ORDER BY ac.data DESC, ac.entrada DESC";
            await using var command = await CreateCommandAsync(query, cancellationToken);

            command.AddParameter("@ColaboradorId", colaboradorId, DbType.Int32);
            command.AddParameter("@Inicio", inicio.ToDateTime(TimeOnly.MinValue), DbType.Date);
            command.AddParameter("@Fim", fim.ToDateTime(TimeOnly.MinValue), DbType.Date);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var acessos = new List<AcessoColaborador>();
            while (await reader.ReadAsync(cancellationToken))
            {
                acessos.Add(Map(reader));
            }

            return acessos;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_POR_COLABORADOR_PERIODO", $"Erro ao obter acessos do colaborador ID {colaboradorId} no período: {ex.Message}", ex);
        }
    }

    // Acesso aberto é aquele que teve entrada registrada mas ainda não teve saída.
    public async Task<AcessoColaborador?> ObterAcessoAbertoPorColaborador(int colaboradorId, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelectQuery} WHERE ac.colaborador_id = @ColaboradorId AND ac.saida IS NULL ORDER BY ac.data DESC, ac.entrada DESC";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@ColaboradorId", colaboradorId, DbType.Int32);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_ACESSO_ABERTO", $"Erro ao obter acesso aberto do colaborador ID {colaboradorId}: {ex.Message}", ex);
        }
    }

    // Soma todos os períodos fechados do dia, conforme a regra de limite de jornada (8h CLT / 6h estágio).
    public async Task<TimeSpan> ObterHorasTrabalhadasNoDia(int colaboradorId, DateOnly data, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = "SELECT entrada, saida FROM tb_acesso_colaborador WHERE colaborador_id = @ColaboradorId AND data = @Data AND saida IS NOT NULL";
            await using var command = await CreateCommandAsync(query, cancellationToken);

            command.AddParameter("@ColaboradorId", colaboradorId, DbType.Int32);
            command.AddParameter("@Data", data.ToDateTime(TimeOnly.MinValue), DbType.Date);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var total = TimeSpan.Zero;
            while (await reader.ReadAsync(cancellationToken))
            {
                var entrada = reader.GetTimeOnlyValue("entrada");
                var saida = reader.GetTimeOnlyValue("saida");
                total += saida.ToTimeSpan() - entrada.ToTimeSpan();
            }

            return total;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_HORAS_TRABALHADAS", $"Erro ao obter horas trabalhadas do colaborador ID {colaboradorId}: {ex.Message}", ex);
        }
    }
}
