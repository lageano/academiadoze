// gabriel geremias vieira
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Domain.Entities;

namespace AcademiaDoZe.Application.Mappings;

public static class AcessoMappingExtensions
{
    public static AcessoAlunoDto ToDto(this AcessoAluno acesso)
    {
        ArgumentNullException.ThrowIfNull(acesso);

        return new AcessoAlunoDto
        {
            Id = acesso.Id,
            Aluno = acesso.Aluno.ToDto(),
            Data = acesso.Data,
            Entrada = acesso.Entrada,
            Saida = acesso.Saida
        };
    }

    public static AcessoColaboradorDto ToDto(this AcessoColaborador acesso)
    {
        ArgumentNullException.ThrowIfNull(acesso);

        return new AcessoColaboradorDto
        {
            Id = acesso.Id,
            Colaborador = acesso.Colaborador.ToDto(),
            Data = acesso.Data,
            Entrada = acesso.Entrada,
            Saida = acesso.Saida
        };
    }
}
