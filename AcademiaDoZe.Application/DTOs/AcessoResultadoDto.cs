// gabriel geremias vieira
namespace AcademiaDoZe.Application.DTOs;

// Retorno das operações da catraca: diz se liberou e a mensagem que a interface deve exibir.
public class AcessoResultadoDto
{
    public required bool Autorizado { get; set; }
    public required string Titulo { get; set; }
    public required string Mensagem { get; set; }

    public static AcessoResultadoDto Negado(string mensagem) => new()
    {
        Autorizado = false,
        Titulo = "Acesso Negado",
        Mensagem = mensagem
    };

    public static AcessoResultadoDto Permitido(string titulo, string mensagem) => new()
    {
        Autorizado = true,
        Titulo = titulo,
        Mensagem = mensagem
    };
}
