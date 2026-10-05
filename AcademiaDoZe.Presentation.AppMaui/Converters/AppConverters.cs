// gabriel geremias vieira
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using System.Globalization;

namespace AcademiaDoZe.Presentation.AppMaui.Converters;

// Converte o conteúdo binário de uma foto/laudo para ImageSource exibível
public sealed class FotoToImageSourceConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        byte[]? bytes = value switch
        {
            ArquivoDto arquivo => arquivo.Conteudo,
            byte[] conteudo => conteudo,
            _ => null
        };

        if (bytes is null || bytes.Length == 0) return null;

        return ImageSource.FromStream(() => new MemoryStream(bytes));
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

// Exibe o nome amigável de qualquer enum anotado com [Display(Name = "...")]
public sealed class EnumDisplayConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is Enum enumValue ? enumValue.GetDisplayName() : string.Empty;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public sealed class InvertedBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b && !b;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b && !b;
}

public sealed class IsNotNullConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is not null;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

// Pinta o resultado da catraca: verde quando autorizado, vermelho quando negado
public sealed class AutorizadoToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool autorizado && autorizado
            ? Color.FromArgb("#1B5E20")
            : Color.FromArgb("#7F1D1D");

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

// Destaca o segmento ativo da alternância Aluno / Colaborador
public sealed class ModoSelecionadoCorConverter : IValueConverter
{
    // O parametro "inverso" atende o botao do outro modo, que usa o mesmo booleano
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var selecionado = value is bool b && b;
        if (parameter as string == "inverso") selecionado = !selecionado;

        return selecionado ? Color.FromArgb("#6D4AFF") : Colors.Transparent;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

// O botão selecionado fica roxo, então o texto precisa ser branco nos dois temas.
public sealed class ModoSelecionadoTextoConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var selecionado = value is bool b && b;
        if (parameter as string == "inverso") selecionado = !selecionado;

        if (selecionado) return Colors.White;

        return Microsoft.Maui.Controls.Application.Current?.RequestedTheme == AppTheme.Dark
            ? Colors.White
            : Color.FromArgb("#12101C");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

// Texto e cor do selo "dentro/fora da academia"
public sealed class PresencaToTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool dentro && dentro ? "DENTRO DA ACADEMIA" : "FORA DA ACADEMIA";

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

// O DatePicker trabalha com DateTime, enquanto os DTOs usam DateOnly.
public sealed class DateOnlyToDateTimeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is DateOnly data ? data.ToDateTime(TimeOnly.MinValue) : DateTime.Today;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is DateTime dateTime ? DateOnly.FromDateTime(dateTime) : DateOnly.FromDateTime(DateTime.Today);
}

public sealed class TimeSpanToHoraConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value switch
        {
            TimeSpan ts => ts.ToString(@"hh\:mm\:ss"),
            TimeOnly t => t.ToString(@"HH\:mm\:ss"),
            _ => "--:--:--"
        };

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
