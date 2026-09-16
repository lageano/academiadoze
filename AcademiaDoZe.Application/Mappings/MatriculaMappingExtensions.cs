// gabriel geremias vieira
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Application.Mappings;

public static class MatriculaMappingExtensions
{
    // A data final é derivada do plano contratado, por isso não é informada pela apresentação.
    public static DateOnly CalcularDataFim(DateOnly dataInicio, MatriculaPlano plano) => plano switch
    {
        MatriculaPlano.Mensal => dataInicio.AddMonths(1),
        MatriculaPlano.Trimestral => dataInicio.AddMonths(3),
        MatriculaPlano.Semestral => dataInicio.AddMonths(6),
        MatriculaPlano.Anual => dataInicio.AddYears(1),
        _ => throw new ArgumentOutOfRangeException(nameof(plano), plano, "Plano de matrícula não suportado.")
    };

    public static MatriculaDto ToDto(this Matricula matricula)
    {
        ArgumentNullException.ThrowIfNull(matricula);

        return new MatriculaDto
        {
            Id = matricula.Id,
            AlunoMatricula = matricula.Aluno.ToDto(),
            Plano = matricula.Plano.ToApplication(),
            DataInicio = matricula.DataInicio,
            DataFim = matricula.DataFim,
            Objetivo = matricula.Objetivo,
            RestricoesMedicas = matricula.Restricoes.ToApplication(),
            ObservacoesRestricoes = matricula.ObservacoesRestricoes,
            LaudoMedico = matricula.LaudoMedico != null ? new ArquivoDto { Conteudo = matricula.LaudoMedico.Conteudo } : null
        };
    }

    public static Matricula ToEntity(this MatriculaDto matriculaDto, Aluno aluno)
    {
        ArgumentNullException.ThrowIfNull(matriculaDto);
        ArgumentNullException.ThrowIfNull(aluno);

        Arquivo? laudo = null;
        if (matriculaDto.LaudoMedico?.Conteudo != null)
        {
            var laudoResult = Arquivo.Criar(matriculaDto.LaudoMedico.Conteudo);
            if (laudoResult.IsSuccess) laudo = laudoResult.Value;
        }

        var plano = matriculaDto.Plano.ToDomain();
        var dataFim = matriculaDto.DataFim != default ? matriculaDto.DataFim : CalcularDataFim(matriculaDto.DataInicio, plano);

        var result = Matricula.Criar(
            matriculaDto.Id,
            aluno,
            plano,
            matriculaDto.DataInicio,
            dataFim,
            matriculaDto.Objetivo,
            matriculaDto.RestricoesMedicas.ToDomain(),
            matriculaDto.ObservacoesRestricoes ?? string.Empty,
            laudo
        );

        if (result.IsFailure)
        {
            throw new InvalidOperationException($"Erro de validação ao converter Matricula: {string.Join(", ", result.Notifications.Select(n => n.Mensagem))}");
        }

        return result.Value!;
    }

    public static Matricula UpdateFromDto(this Matricula matricula, MatriculaDto matriculaDto, Aluno aluno)
    {
        ArgumentNullException.ThrowIfNull(matricula);
        ArgumentNullException.ThrowIfNull(matriculaDto);
        ArgumentNullException.ThrowIfNull(aluno);

        Arquivo? laudo = matricula.LaudoMedico;
        if (matriculaDto.LaudoMedico?.Conteudo != null)
        {
            var laudoResult = Arquivo.Criar(matriculaDto.LaudoMedico.Conteudo);
            if (laudoResult.IsSuccess) laudo = laudoResult.Value;
        }

        var plano = matriculaDto.Plano != default ? matriculaDto.Plano.ToDomain() : matricula.Plano;
        var dataInicio = matriculaDto.DataInicio != default ? matriculaDto.DataInicio : matricula.DataInicio;
        var dataFim = matriculaDto.DataFim != default ? matriculaDto.DataFim : CalcularDataFim(dataInicio, plano);

        var result = Matricula.Criar(
            matricula.Id,
            aluno,
            plano,
            dataInicio,
            dataFim,
            matriculaDto.Objetivo ?? matricula.Objetivo,
            matriculaDto.RestricoesMedicas != default ? matriculaDto.RestricoesMedicas.ToDomain() : matricula.Restricoes,
            matriculaDto.ObservacoesRestricoes ?? matricula.ObservacoesRestricoes,
            laudo
        );

        if (result.IsFailure)
        {
            throw new InvalidOperationException($"Erro de validação ao atualizar Matricula: {string.Join(", ", result.Notifications.Select(n => n.Mensagem))}");
        }

        return result.Value!;
    }
}
