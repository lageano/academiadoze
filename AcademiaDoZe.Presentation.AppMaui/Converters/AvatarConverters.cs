// gabriel geremias vieira
using System.Globalization;

namespace AcademiaDoZe.Presentation.AppMaui.Converters;

// Extrai as iniciais do nome para o avatar, como fazem a maioria dos apps quando não há foto.
public sealed class IniciaisConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string nome || string.IsNullOrWhiteSpace(nome)) return "?";

        var partes = nome.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return partes.Length switch
        {
            0 => "?",
            1 => partes[0][..1].ToUpperInvariant(),
            _ => string.Concat(partes[0][..1], partes[^1][..1]).ToUpperInvariant()
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

// Dá a cada pessoa uma cor estável de avatar derivada do próprio nome.
public sealed class NomeParaCorConverter : IValueConverter
{
    private static readonly Color[] Paleta =
    [
        Color.FromArgb("#6D4AFF"), Color.FromArgb("#2E7D32"), Color.FromArgb("#C2410C"),
        Color.FromArgb("#0E7490"), Color.FromArgb("#9D174D"), Color.FromArgb("#4338CA"),
        Color.FromArgb("#B45309"), Color.FromArgb("#065F46")
    ];

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string texto || string.IsNullOrWhiteSpace(texto)) return Paleta[0];

        int soma = texto.Sum(c => c);
        return Paleta[Math.Abs(soma) % Paleta.Length];
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

// Mostra o avatar com iniciais somente quando a pessoa não tem foto cadastrada.
public sealed class SemFotoConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var bytes = value switch
        {
            AcademiaDoZe.Application.DTOs.ArquivoDto arquivo => arquivo.Conteudo,
            byte[] conteudo => conteudo,
            _ => null
        };

        return bytes is null || bytes.Length == 0;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

// Apresenta o CPF na máscara 000.000.000-00 sem mexer no valor armazenado.
public sealed class CpfFormatadoConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var digitos = new string((value as string ?? string.Empty).Where(char.IsDigit).ToArray());

        return digitos.Length == 11
            ? $"CPF {digitos[..3]}.{digitos[3..6]}.{digitos[6..9]}-{digitos[9..]}"
            : digitos;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

// Apresenta o telefone como (00) 00000-0000, aceitando também números de 10 dígitos.
public sealed class TelefoneFormatadoConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var digitos = new string((value as string ?? string.Empty).Where(char.IsDigit).ToArray());

        return digitos.Length switch
        {
            11 => $"({digitos[..2]}) {digitos[2..7]}-{digitos[7..]}",
            10 => $"({digitos[..2]}) {digitos[2..6]}-{digitos[6..]}",
            _ => digitos
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
