// gabriel geremias vieira
using System.ComponentModel;

namespace AcademiaDoZe.Presentation.AppMaui.Helpers;

// Markup extension para internacionalização direta no XAML: {helpers:Translate strTitulo}
// ou, para formatos culturais, {helpers:Translate FormatoDataLonga}.
// Usa Binding no LocalizationManager, então a troca de idioma reflete na hora.
[ContentProperty(nameof(Key))]
public class TranslateExtension : IMarkupExtension<BindingBase>
{
    public string Key { get; set; } = string.Empty;

    public BindingBase ProvideValue(IServiceProvider serviceProvider)
    {
        // As chaves de formato apontam para propriedades; as demais, para o indexador do .resx
        var path = Key switch
        {
            nameof(LocalizationManager.FormatoDataCurta) => nameof(LocalizationManager.FormatoDataCurta),
            nameof(LocalizationManager.FormatoDataLonga) => nameof(LocalizationManager.FormatoDataLonga),
            _ => $"[{Key}]"
        };

        return new Binding
        {
            Mode = BindingMode.OneWay,
            Path = path,
            Source = LocalizationManager.Instance
        };
    }

    object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
        => ProvideValue(serviceProvider);
}
