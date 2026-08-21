using AcademiaDoZe.Infrastructure.Data;

[assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly, DisableTestParallelization = true)]

namespace AcademiaDoZe.Infrastructure.Tests;

public abstract class TestBase
{
    // Altere o SGBD alvo dos testes trocando apenas a constante abaixo:
    private const DatabaseType SelectedDatabaseType = DatabaseType.SqlServer;

    protected string ConnectionString { get; }
    protected DatabaseType DatabaseType { get; }

    // Nome do SGBD no formato exigido pelo laboratório para o campo "cidade" do registro de entrega.
    protected string NomeSgbdParaEntrega => DatabaseType switch
    {
        DatabaseType.SqlServer => "SQLServer",
        DatabaseType.MySql => "MySQL",
        DatabaseType.Sqlite => "SQLite",
        _ => throw new ArgumentOutOfRangeException(nameof(DatabaseType), DatabaseType, "SGBD não suportado para testes.")
    };

    protected TestBase()
    {
        DatabaseType = SelectedDatabaseType;

        // Ajuste a ConnectionString com caminhos e credenciais válidas
        ConnectionString = DatabaseType switch
        {
            DatabaseType.SqlServer => "Server=localhost;Database=db_academia_do_ze;User Id=gabriel;Password=malucaodo1;TrustServerCertificate=True;Encrypt=True;",
            DatabaseType.MySql => "Server=localhost;Database=db_academia_do_ze;User Id=coelho;Password=abcBolinhas12345;",
            DatabaseType.Sqlite => CriarConnectionStringSqlite(),
            _ => throw new ArgumentOutOfRangeException(nameof(DatabaseType), DatabaseType, "SGBD não suportado para testes.")
        };
    }

    private static string CriarConnectionStringSqlite()
    {
        var diretorio = @"C:\AcademiaDoZe";
        Directory.CreateDirectory(diretorio);
        var caminhoBanco = Path.Combine(diretorio, "db_academia_do_ze.db");
        return $"Data Source={caminhoBanco};Cache=Shared;";
    }

    #region Geradores de dados aleatórios
    private static int _counter = 10000;
    protected static string GerarCep() => (80000000 + ((int)(DateTime.UtcNow.Ticks % 8000000)) + Interlocked.Increment(ref _counter)).ToString("D8")[..8];
    protected static string GerarCpf() => (10000000000L + ((DateTime.UtcNow.Ticks % 80000000000L)) + Interlocked.Increment(ref _counter)).ToString("D11")[..11];
    protected static string GerarEmail() => $"user_{Guid.NewGuid().ToString("N")[..8]}@test.com";
    protected static string GerarTelefone() => (49990000000L + ((DateTime.UtcNow.Ticks % 8000000000L)) + Interlocked.Increment(ref _counter)).ToString("D11")[..11];
    #endregion
}
