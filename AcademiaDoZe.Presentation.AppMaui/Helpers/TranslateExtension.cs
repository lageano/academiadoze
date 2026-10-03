// gabriel geremias vieira
using System.ComponentModel;

namespace AcademiaDoZe.Presentation.AppMaui.Helpers;

// Markup extension para internacionalização direta no XAML: {helpers:Translate strTitulo}
// Usa Binding no indexador do LocalizationResourceManager, então a troca de idioma reflete na hora.
[ContentProperty(nameof(Key))]
public class TranslateExtension : IMarkupExtension<BindingBase>
{
    public string Key { get; set; } = string.Empty;

    public BindingBase ProvideValue(IServiceProvider serviceProvider)
    {
        return new Binding
        {
            Mode = BindingMode.OneWay,
            Path = $"[{Key}]",
            Source = LocalizationResourceManager.Instance
        };
    }

    object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
        => ProvideValue(serviceProvider);
}
