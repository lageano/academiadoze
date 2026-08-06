// gabriel geremias vieira
using AcademiaDoZe.Domain.Common;

namespace AcademiaDoZe.Domain.ValueObjects;

public record Arquivo
{
    private const int TamanhoMaximoBytes = 15 * 1024 * 1024;

    public byte[] Conteudo { get; }

    private Arquivo(byte[] conteudo)
    {
        Conteudo = conteudo;
    }

    public static Result<Arquivo> Criar(byte[] conteudo)
    {
        if (conteudo is null || conteudo.Length == 0)
            return Result<Arquivo>.Failure("Arquivo", "ARQUIVO_OBRIGATORIO");

        if (conteudo.Length > TamanhoMaximoBytes)
            return Result<Arquivo>.Failure("Arquivo", "ARQUIVO_TAMANHO");

        return Result<Arquivo>.Success(new Arquivo(conteudo));
    }

    public virtual bool Equals(Arquivo? other) =>
        other is not null && Conteudo.SequenceEqual(other.Conteudo);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var b in Conteudo)
            hash.Add(b);
        return hash.ToHashCode();
    }
}
