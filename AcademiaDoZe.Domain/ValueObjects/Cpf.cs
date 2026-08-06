// gabriel geremias vieira
using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Services;

namespace AcademiaDoZe.Domain.ValueObjects;

public record Cpf
{
    public string Valor { get; }

    private Cpf(string valor)
    {
        Valor = valor;
    }

    public static Result<Cpf> Criar(string valor)
    {
        if (NormalizadoService.TextoVazioOuNulo(valor))
            return Result<Cpf>.Failure("Cpf", "CPF_OBRIGATORIO");

        var textoLimpo = NormalizadoService.LimparEDigitos(valor);

        if (textoLimpo.Length != 11)
            return Result<Cpf>.Failure("Cpf", "CPF_DIGITOS");

        if (TodosDigitosIguais(textoLimpo))
            return Result<Cpf>.Failure("Cpf", "CPF_INVALIDO");

        if (!PossuiDigitosVerificadoresValidos(textoLimpo))
            return Result<Cpf>.Failure("Cpf", "CPF_INVALIDO");

        return Result<Cpf>.Success(new Cpf(textoLimpo));
    }

    private static bool TodosDigitosIguais(string digitos) => digitos.Distinct().Count() == 1;

    private static bool PossuiDigitosVerificadoresValidos(string digitos)
    {
        var numeros = digitos.Select(c => c - '0').ToArray();

        var primeiroDv = CalcularDigitoVerificador(numeros, 9);
        if (primeiroDv != numeros[9])
            return false;

        var segundoDv = CalcularDigitoVerificador(numeros, 10);
        return segundoDv == numeros[10];
    }

    private static int CalcularDigitoVerificador(int[] numeros, int quantidade)
    {
        var soma = 0;
        var peso = quantidade + 1;

        for (var i = 0; i < quantidade; i++)
            soma += numeros[i] * peso--;

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }

    public override string ToString() => Valor;
}
