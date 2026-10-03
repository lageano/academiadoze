// gabriel geremias vieira
using AcademiaDoZe.Presentation.AppMaui.Resources.Strings;
using System.ComponentModel;
using System.Globalization;

namespace AcademiaDoZe.Presentation.AppMaui.Helpers;

// Permite trocar o idioma em runtime: ao mudar a cultura, notifica os bindings para recarregarem os textos.
public class LocalizationResourceManager : INotifyPropertyChanged
{
    public static LocalizationResourceManager Instance { get; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    private LocalizationResourceManager()
    {
    }

    // Indexador usado pela markup extension {helpers:Translate chave}
    public string this[string chave] => AppResources.ResourceManager.GetString(chave, CultureInfo.CurrentUICulture) ?? chave;

    public CultureInfo CurrentCulture => CultureInfo.CurrentUICulture;

    public void SetCulture(CultureInfo culture)
    {
        CultureInfo.CurrentUICulture = culture;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        AppResources.Culture = culture;

        // string.Empty faz o binding reavaliar todas as propriedades do indexador
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
    }
}
