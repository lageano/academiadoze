// gabriel geremias vieira
using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Services;

namespace AcademiaDoZe.Domain.ValueObjects;

public record Senha
{
    private const int TamanhoMinimo = 6;

    public string Valor { get; }

    private Senha(string valor)
    {
        Valor = valor;
    }

    public static Result<Senha> Criar(string valor)
    {
        if (NormalizadoService.TextoVazioOuNulo(valor))
            return Result<Senha>.Failure("Senha", "SENHA_OBRIGATORIA");

        if (valor.Length < TamanhoMinimo)
            return Result<Senha>.Failure("Senha", "SENHA_TAMANHO_MINIMO");

        if (!valor.Any(char.IsLetter) || !valor.Any(char.IsDigit))
            return Result<Senha>.Failure("Senha", "SENHA_LETRA_NUMERO");

        return Result<Senha>.Success(new Senha(valor));
    }

    public override string ToString() => new('*', Valor.Length);
}
