// gabriel geremias vieira
using AcademiaDoZe.Application.Enums;

namespace AcademiaDoZe.Presentation.AppMaui.Configuration;

// Centraliza as credenciais de banco da aplicação.
// Os valores ficam salvos em Preferences e podem ser alterados em tempo de execução
// pela tela de Configurações; os padrões abaixo valem na primeira abertura.
public static class AppSettings
{
    // No Android "localhost" seria o proprio celular, entao o padrao e o IP do
    // computador na rede local; os dois precisam estar no mesmo Wi-Fi.
#if ANDROID
    private const string ServidorPadrao = "192.168.0.8";
#else
    private const string ServidorPadrao = "localhost";
#endif

    private const AppDatabaseType TipoPadrao = AppDatabaseType.MySql;
    private const string BancoPadrao = "db_academia_do_ze";
    private const string UsuarioPadrao = "coelho";
    private const string SenhaPadrao = "abcBolinhas12345";

    public static AppDatabaseType BancoSelecionado
    {
        get => Enum.TryParse<AppDatabaseType>(Preferences.Get("BancoTipo", TipoPadrao.ToString()), out var tipo)
            ? tipo
            : TipoPadrao;
        set => Preferences.Set("BancoTipo", value.ToString());
    }

    public static string Servidor
    {
        get => Preferences.Get("BancoServidor", ServidorPadrao);
        set => Preferences.Set("BancoServidor", value ?? string.Empty);
    }

    public static string Banco
    {
        get => Preferences.Get("BancoNome", BancoPadrao);
        set => Preferences.Set("BancoNome", value ?? string.Empty);
    }

    public static string Usuario
    {
        get => Preferences.Get("BancoUsuario", UsuarioPadrao);
        set => Preferences.Set("BancoUsuario", value ?? string.Empty);
    }

    public static string Senha
    {
        get => Preferences.Get("BancoSenha", SenhaPadrao);
        set => Preferences.Set("BancoSenha", value ?? string.Empty);
    }

    // Caminho do arquivo .db, usado somente pelo SQLite
    public static string CaminhoSqlite
    {
        get => Preferences.Get("BancoCaminhoSqlite", CaminhoSqlitePadrao());
        set => Preferences.Set("BancoCaminhoSqlite", value ?? string.Empty);
    }

    // Trecho livre acrescentado ao fim da string de conexão (SSL, Timeout, Criptografia)
    public static string Complemento
    {
        get => Preferences.Get("BancoComplemento", string.Empty);
        set => Preferences.Set("BancoComplemento", value ?? string.Empty);
    }

    public static string ConnectionString
    {
        get
        {
            var complemento = Complemento?.Trim() ?? string.Empty;

            if (complemento.Length > 0 && !complemento.EndsWith(';'))
                complemento += ";";

            var baseConexao = BancoSelecionado switch
            {
                AppDatabaseType.SqlServer => $"Server={Servidor};Database={Banco};User Id={Usuario};Password={Senha};TrustServerCertificate=True;Encrypt=True;",
                AppDatabaseType.MySql => $"Server={Servidor};Database={Banco};User Id={Usuario};Password={Senha};",
                AppDatabaseType.Sqlite => $"Data Source={CaminhoSqlite};Cache=Shared;",
                _ => throw new ArgumentOutOfRangeException(nameof(BancoSelecionado), BancoSelecionado, "SGBD não suportado.")
            };

            return baseConexao + complemento;
        }
    }

    public static string CaminhoSqlitePadrao()
    {
        var diretorio = Path.Combine(FileSystem.AppDataDirectory, "AcademiaDoZe");
        Directory.CreateDirectory(diretorio);
        return Path.Combine(diretorio, "db_academia_do_ze.db");
    }
}
