// gabriel geremias vieira
using AcademiaDoZe.Presentation.AppMaui.Resources.Strings;
using System.ComponentModel;
using System.Globalization;

namespace AcademiaDoZe.Presentation.AppMaui.Helpers;

// Notifica a interface quando o idioma muda, para que os textos e os formatos
// culturais sejam recarregados sem precisar reiniciar a aplicação.
public class LocalizationManager : INotifyPropertyChanged
{
    public static LocalizationManager Instance { get; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    // A cultura inicial vem da preferência salva; sem preferência, usa a do sistema.
    private LocalizationManager() => SetCulture(Preferences.Get("Cultura", ObterCulturaPadrao()));

    // Descobre o idioma do sistema operacional e cai para pt-BR quando não há tradução.
    public static string ObterCulturaPadrao()
    {
        var uiName = CultureInfo.InstalledUICulture.Name;

        if (uiName.StartsWith("en", StringComparison.OrdinalIgnoreCase)) return "en-US";
        if (uiName.StartsWith("es", StringComparison.OrdinalIgnoreCase)) return "es-ES";

        return "pt-BR";
    }

    // Indexador usado pela markup extension {helpers:Translate chave}
    public string this[string chave] => AppResources.ResourceManager.GetString(chave, CultureInfo.CurrentUICulture) ?? chave;

    // Formatos de data da cultura atual, usados nos DatePicker
    public string FormatoDataCurta => CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;

    public string FormatoDataLonga => CultureInfo.CurrentCulture.DateTimeFormat.LongDatePattern;

    public CultureInfo CurrentCulture => CultureInfo.CurrentUICulture;

    public void SetCulture(string strCultura)
    {
        if (strCultura != "en-US" && strCultura != "es-ES" && strCultura != "pt-BR")
            strCultura = "pt-BR";

        // Guarda a escolha para a próxima abertura do aplicativo
        Preferences.Set("Cultura", strCultura);

        var ci = new CultureInfo(strCultura);

        // CurrentCulture afeta datas, números e moeda; CurrentUICulture afeta os textos
        Thread.CurrentThread.CurrentCulture = ci;
        Thread.CurrentThread.CurrentUICulture = ci;
        CultureInfo.DefaultThreadCurrentCulture = ci;
        CultureInfo.DefaultThreadCurrentUICulture = ci;
        AppResources.Culture = ci;

#if WINDOWS
        try
        {
            Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = strCultura;
        }
        catch
        {
            // Ambientes restritos (sandbox) podem recusar a troca; o resto da aplicação segue normalmente.
        }
#endif

        // null avisa que TODAS as propriedades mudaram, forçando a interface a recarregar
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FormatoDataCurta)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FormatoDataLonga)));
    }

    public void SetCulture(CultureInfo cultura) => SetCulture(cultura.Name);
}
