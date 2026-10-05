// gabriel geremias vieira
using AcademiaDoZe.Infrastructure.Data;

namespace AcademiaDoZe.Presentation.AppMaui.Configuration;

// Centraliza a escolha do SGBD e a string de conexão usada pela aplicação.
// No Windows o app fala com o MySQL da máquina; no Android, onde não existe
// um servidor local, ele usa SQLite e o DbInitializer cria o banco no primeiro uso.
public static class AppSettings
{
#if ANDROID
    public const DatabaseType BancoSelecionado = DatabaseType.Sqlite;
#else
    public const DatabaseType BancoSelecionado = DatabaseType.MySql;
#endif

    // Trocar por "192.168.x.x" (IP da máquina na rede) caso o celular precise
    // acessar o MySQL do computador em vez do SQLite local.
    private const string Servidor = "localhost";

    public static string ConnectionString => BancoSelecionado switch
    {
        DatabaseType.SqlServer => $"Server={Servidor};Database=db_academia_do_ze;User Id=gabriel;Password=malucaodo1;TrustServerCertificate=True;Encrypt=True;",
        DatabaseType.MySql => $"Server={Servidor};Database=db_academia_do_ze;User Id=coelho;Password=abcBolinhas12345;",
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
