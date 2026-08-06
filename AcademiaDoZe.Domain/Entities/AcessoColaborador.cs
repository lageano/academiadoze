// gabriel geremias vieira
using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Exceptions;

namespace AcademiaDoZe.Domain.Entities;

public class AcessoColaborador : Entity, IAggregateRoot
{
    public Colaborador Colaborador { get; private set; }
    public DateOnly Data { get; private set; }
    public TimeOnly Entrada { get; private set; }
    public TimeOnly? Saida { get; private set; }

    private AcessoColaborador(int id, Colaborador colaborador, DateOnly data, TimeOnly entrada, TimeOnly? saida)
        : base(id)
    {
        Colaborador = colaborador;
        Data = data;
        Entrada = entrada;
        Saida = saida;
    }

    public static Result<AcessoColaborador> Criar(int id, Colaborador colaborador, DateOnly data, TimeOnly entrada)
    {
        if (colaborador is null)
            throw new DomainException("COLABORADOR_OBRIGATORIO");

        return Result<AcessoColaborador>.Success(new AcessoColaborador(id, colaborador, data, entrada, null));
    }

    public void RegistrarSaida(TimeOnly saida)
    {
        if (Saida is not null)
            throw new DomainException("SAIDA_JA_REGISTRADA");

        if (saida < Entrada)
            throw new DomainException("SAIDA_ANTERIOR_ENTRADA");

        Saida = saida;
    }
}
