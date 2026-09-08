using AcademiaDoZe.Infrastructure.Data;

[assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly, DisableTestParallelization = true)]

namespace AcademiaDoZe.Infrastructure.Tests;

public abstract class TestBase
{
    // Altere o SGBD alvo dos testes trocando apenas a constante abaixo:
    private const DatabaseType SelectedDatabaseType = DatabaseType.MySql;

    protected string ConnectionString { get; }
    protected DatabaseType DatabaseType { get; }

    // Dados exigidos pela entrega do Laboratório prático.
    protected const string NomeParaEntrega = "Gabriel";
    protected const string SobrenomeParaEntrega = "Geremias Vieira";

    // Sigla do SGBD no formato exigido pelo laboratório (usada no nome do logradouro e na senha).
    protected static string NomeSgbdParaEntrega => SelectedDatabaseType switch
    {
        DatabaseType.SqlServer => "SQLServer",
        DatabaseType.MySql => "MySQL",
        DatabaseType.Sqlite => "SQLite",
        _ => throw new ArgumentOutOfRangeException(nameof(SelectedDatabaseType), SelectedDatabaseType, "SGBD não suportado para testes.")
    };

    // A senha deve conter a sigla do SGBD utilizado no momento do teste.
    protected static string SenhaParaEntrega => $"Senha123{NomeSgbdParaEntrega}";

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
    protected static string GerarEmail() => $"user_{Guid.NewGuid().ToString("N")[..8]}@test.com";
    protected static string GerarTelefone() => (49990000000L + ((DateTime.UtcNow.Ticks % 8000000000L)) + Interlocked.Increment(ref _counter)).ToString("D11")[..11];

    // O Value Object Cpf valida os dígitos verificadores, então geramos os 9 primeiros dígitos e calculamos os 2 finais.
    protected static string GerarCpf()
    {
        var raiz = (100000000L + ((DateTime.UtcNow.Ticks + Interlocked.Increment(ref _counter) + Random.Shared.Next(1, 999999)) % 800000000L)).ToString("D9");
        var digitos = raiz.Select(c => c - '0').ToList();

        digitos.Add(CalcularDigitoVerificador(digitos, 9));
        digitos.Add(CalcularDigitoVerificador(digitos, 10));

        return string.Concat(digitos);
    }

    private static int CalcularDigitoVerificador(IReadOnlyList<int> digitos, int quantidade)
    {
        var soma = 0;
        var peso = quantidade + 1;

        for (var i = 0; i < quantidade; i++)
            soma += digitos[i] * peso--;

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }
    #endregion
}
