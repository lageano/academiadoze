// gabriel geremias vieira
using AcademiaDoZe.Infrastructure.Data;

namespace AcademiaDoZe.Presentation.AppMaui.Configuration;

// Centraliza a escolha do SGBD e a string de conexão usada pela aplicação.
// Windows e Android usam o mesmo banco MySQL, para que os dois mostrem os mesmos dados.
// No Android "localhost" seria o proprio celular, entao o servidor e o IP do
// computador na rede local; os dois precisam estar no mesmo Wi-Fi.
public static class AppSettings
{
    public const DatabaseType BancoSelecionado = DatabaseType.MySql;

#if ANDROID
    private const string Servidor = "192.168.0.8";
#else
    private const string Servidor = "localhost";
#endif

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
