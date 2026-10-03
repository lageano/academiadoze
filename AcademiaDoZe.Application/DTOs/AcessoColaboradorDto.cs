// gabriel geremias vieira
namespace AcademiaDoZe.Application.DTOs;

public class AcessoColaboradorDto
{
    public int Id { get; set; }
    public required ColaboradorDto Colaborador { get; set; }
    public required DateOnly Data { get; set; }
    public required TimeOnly Entrada { get; set; }
    public TimeOnly? Saida { get; set; }

    public bool EstaAberto => Saida is null;

    public TimeSpan? TempoPermanencia => Saida.HasValue
        ? Saida.Value.ToTimeSpan() - Entrada.ToTimeSpan()
        : null;
}
