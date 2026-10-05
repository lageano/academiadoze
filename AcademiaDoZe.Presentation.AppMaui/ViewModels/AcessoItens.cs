// gabriel geremias vieira
namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

// Representação única de Aluno ou Colaborador para a busca da catraca.
public class PessoaAcessoItem
{
    public required int Id { get; init; }
    public required string Nome { get; init; }
    public required string Cpf { get; init; }
    public byte[]? Foto { get; init; }

    public string CpfFormatado => Cpf.Length == 11
        ? $"{Cpf[..3]}.{Cpf[3..6]}.{Cpf[6..9]}-{Cpf[9..]}"
        : Cpf;

    public override string ToString() => Nome;
}

// Linha da lista "Acessos Registrados Hoje".
public class AcessoHojeItem
{
    public required string Nome { get; init; }
    public required string Descricao { get; init; }
    public required string Hora { get; init; }
}
