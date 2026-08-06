// gabriel geremias vieira
using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Exceptions;

namespace AcademiaDoZe.Domain.Entities;

public class AcessoAluno : Entity, IAggregateRoot
{
    public Aluno Aluno { get; private set; }
    public DateOnly Data { get; private set; }
    public TimeOnly Entrada { get; private set; }
    public TimeOnly? Saida { get; private set; }

    private AcessoAluno(int id, Aluno aluno, DateOnly data, TimeOnly entrada, TimeOnly? saida)
        : base(id)
    {
        Aluno = aluno;
        Data = data;
        Entrada = entrada;
        Saida = saida;
    }

    public static Result<AcessoAluno> Criar(int id, Aluno aluno, DateOnly data, TimeOnly entrada)
    {
        if (aluno is null)
            throw new DomainException("ALUNO_OBRIGATORIO");

        return Result<AcessoAluno>.Success(new AcessoAluno(id, aluno, data, entrada, null));
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
