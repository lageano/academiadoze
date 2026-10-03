using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Exceptions;
using System.Data;
using System.Data.Common;

namespace AcademiaDoZe.Infrastructure.Repositories;

public class AcessoAlunoRepository : BaseRepository, IAcessoAlunoRepository
{
    public AcessoAlunoRepository(string connectionString, DatabaseType databaseType) : base(connectionString, databaseType)
    {
    }

    private static string BaseSelectQuery => @"
        SELECT
            ac.id_acesso_aluno, ac.aluno_id, ac.data, ac.entrada, ac.saida,
            a.id_aluno, a.cpf, a.nome AS aluno_nome, a.nascimento, a.telefone, a.email, a.logradouro_id, a.numero, a.complemento, a.senha, a.foto,
            l.id_logradouro, l.cep, l.nome AS logradouro_nome, l.bairro, l.cidade, l.estado, l.pais
        FROM tb_acesso_aluno ac
        INNER JOIN tb_aluno a ON ac.aluno_id = a.id_aluno
        INNER JOIN tb_logradouro l ON a.logradouro_id = l.id_logradouro";

    public async Task<AcessoAluno?> ObterPorId(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelectQuery} WHERE ac.id_acesso_aluno = @Id";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Id", id, DbType.Int32);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_POR_ID", $"Erro ao obter acesso do aluno por ID {id}: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<AcessoAluno>> ObterTodos(CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelectQuery} ORDER BY ac.data DESC, ac.entrada DESC";
            await using var command = await CreateCommandAsync(query, cancellationToken);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var acessos = new List<AcessoAluno>();
            while (await reader.ReadAsync(cancellationToken))
            {
                acessos.Add(Map(reader));
            }

            return acessos;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_TODOS", $"Erro ao obter todos os acessos de alunos: {ex.Message}", ex);
        }
    }

    public static AcessoAluno Map(DbDataReader reader)
    {
        int id = 0;
        try
        {
            id = reader.GetInt32Value("id_acesso_aluno");
            DateOnly data = reader.GetDateOnlyValue("data");
            TimeOnly entrada = reader.GetTimeOnlyValue("entrada");
            TimeOnly? saida = reader.GetNullableTimeOnly("saida");

            var aluno = AlunoRepository.Map(reader, "aluno_nome");

            var result = AcessoAluno.Criar(id, aluno, data, entrada);
            if (result.IsFailure)
            {
                throw new InfrastructureException("ERRO_DOMINIO_MAPEAMENTO", $"Erro de domínio ao mapear acesso do aluno ID {id}: {string.Join(", ", result.Notifications.Select(n => n.Mensagem))}");
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
            throw new InfrastructureException("ERRO_MAPEAMENTO_ACESSO_ALUNO", $"Erro ao mapear dados do acesso do aluno ID {id}: {ex.Message}", ex);
        }
    }

    public async Task<AcessoAluno> Adicionar(AcessoAluno entity, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = FormatInsertQuery("INSERT INTO tb_acesso_aluno (aluno_id, data, entrada, saida) VALUES (@AlunoId, @Data, @Entrada, @Saida)");

            await using var command = await CreateCommandAsync(query, cancellationToken);
            AdicionarParametrosComuns(command, entity);

            int id = await command.ExecuteScalarIdAsync("ERRO_ADICIONAR_ACESSO_ALUNO", "Falha ao obter ID inserido para o acesso do aluno.", cancellationToken);
            var idProperty = typeof(Entity).GetProperty("Id");
            idProperty?.SetValue(entity, id);

            return entity;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_ADICIONAR_ACESSO_ALUNO", $"Erro ao adicionar acesso do aluno: {ex.Message}", ex);
        }
    }

    public async Task<AcessoAluno> Atualizar(AcessoAluno entity, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = "UPDATE tb_acesso_aluno SET aluno_id = @AlunoId, data = @Data, entrada = @Entrada, saida = @Saida WHERE id_acesso_aluno = @Id";

            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Id", entity.Id, DbType.Int32);
            AdicionarParametrosComuns(command, entity);

            int rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);

            if (rowsAffected == 0)
            {
                throw new InfrastructureException("REGISTRO_NAO_ENCONTRADO", $"Nenhum acesso de aluno encontrado com o ID {entity.Id} para atualização.");
            }

            return entity;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_ATUALIZAR_ACESSO_ALUNO", $"Erro ao atualizar acesso do aluno ID {entity.Id}: {ex.Message}", ex);
        }
    }

    private static void AdicionarParametrosComuns(DbCommand command, AcessoAluno entity)
    {
        command.AddParameter("@AlunoId", entity.Aluno.Id, DbType.Int32);
        command.AddParameter("@Data", entity.Data.ToDateTime(TimeOnly.MinValue), DbType.Date);
        command.AddParameter("@Entrada", entity.Entrada.ToTimeSpan(), DbType.Time);
        command.AddParameter("@Saida", (object?)entity.Saida?.ToTimeSpan() ?? DBNull.Value, DbType.Time);
    }

    public async Task<bool> Remover(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = "DELETE FROM tb_acesso_aluno WHERE id_acesso_aluno = @Id";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Id", id, DbType.Int32);

            var result = await command.ExecuteNonQueryAsync(cancellationToken);
            return result > 0;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_REMOVER_ACESSO_ALUNO", $"Erro ao remover acesso do aluno ID {id}: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<AcessoAluno>> ObterPorAlunoPeriodo(int alunoId, DateOnly inicio, DateOnly fim, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelectQuery} WHERE ac.aluno_id = @AlunoId AND ac.data >= @Inicio AND ac.data <= @Fim ORDER BY ac.data DESC, ac.entrada DESC";
            await using var command = await CreateCommandAsync(query, cancellationToken);

            command.AddParameter("@AlunoId", alunoId, DbType.Int32);
            command.AddParameter("@Inicio", inicio.ToDateTime(TimeOnly.MinValue), DbType.Date);
            command.AddParameter("@Fim", fim.ToDateTime(TimeOnly.MinValue), DbType.Date);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var acessos = new List<AcessoAluno>();
            while (await reader.ReadAsync(cancellationToken))
            {
                acessos.Add(Map(reader));
            }

            return acessos;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_POR_ALUNO_PERIODO", $"Erro ao obter acessos do aluno ID {alunoId} no período: {ex.Message}", ex);
        }
    }

    // Acesso aberto é aquele que teve entrada registrada mas ainda não teve saída.
    public async Task<AcessoAluno?> ObterAcessoAbertoPorAluno(int alunoId, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelectQuery} WHERE ac.aluno_id = @AlunoId AND ac.saida IS NULL ORDER BY ac.data DESC, ac.entrada DESC";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@AlunoId", alunoId, DbType.Int32);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_ACESSO_ABERTO", $"Erro ao obter acesso aberto do aluno ID {alunoId}: {ex.Message}", ex);
        }
    }

    public async Task<bool> EstaNaAcademia(int alunoId, CancellationToken cancellationToken = default)
    {
        var acessoAberto = await ObterAcessoAbertoPorAluno(alunoId, cancellationToken);
        return acessoAberto != null;
    }
}
