// gabriel geremias vieira
using AcademiaDoZe.Infrastructure.Data;

namespace AcademiaDoZe.Presentation.AppMaui.Configuration;

// Centraliza a escolha do SGBD e a string de conexão usada pela aplicação.
public static class AppSettings
{
    public const DatabaseType BancoSelecionado = DatabaseType.MySql;

    public static string ConnectionString => BancoSelecionado switch
    {
        DatabaseType.SqlServer => "Server=localhost;Database=db_academia_do_ze;User Id=gabriel;Password=malucaodo1;TrustServerCertificate=True;Encrypt=True;",
        DatabaseType.MySql => "Server=localhost;Database=db_academia_do_ze;User Id=coelho;Password=abcBolinhas12345;",
        DatabaseType.Sqlite => CriarConnectionStringSqlite(),
        _ => throw new ArgumentOutOfRangeException(nameof(BancoSelecionado), BancoSelecionado, "SGBD não suportado.")
    };

    private static string CriarConnectionStringSqlite()
    {
        var diretorio = Path.Combine(FileSystem.AppDataDirectory, "AcademiaDoZe");
        Directory.CreateDirectory(diretorio);
        return $"Data Source={Path.Combine(diretorio, "db_academia_do_ze.db")};Cache=Shared;";
    }
}
