// gabriel geremias vieira
using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.Exceptions;
using AcademiaDoZe.Domain.Services;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Entities;

public class Matricula : Entity, IAggregateRoot
{
    private const int IdadeMinimaExigeLaudo = 12;
    private const int IdadeMaximaExigeLaudo = 16;

    public Aluno Aluno { get; private set; }
    public int AlunoId => Aluno.Id;
    public MatriculaPlano Plano { get; private set; }
    public DateOnly DataInicio { get; private set; }
    public DateOnly DataFim { get; private set; }
    public string Objetivo { get; private set; }
    public MatriculaRestricoes Restricoes { get; private set; }
    public string ObservacoesRestricoes { get; private set; }
    public Arquivo? LaudoMedico { get; private set; }

    private Matricula(
        int id,
        Aluno aluno,
        MatriculaPlano plano,
        DateOnly dataInicio,
        DateOnly dataFim,
        string objetivo,
        MatriculaRestricoes restricoes,
        string observacoesRestricoes,
        Arquivo? laudoMedico)
        : base(id)
    {
        Aluno = aluno;
        Plano = plano;
        DataInicio = dataInicio;
        DataFim = dataFim;
        Objetivo = objetivo;
        Restricoes = restricoes;
        ObservacoesRestricoes = observacoesRestricoes;
        LaudoMedico = laudoMedico;
    }

    public static Result<Matricula> Criar(
        int id,
        Aluno aluno,
        MatriculaPlano plano,
        DateOnly dataInicio,
        DateOnly dataFim,
        string objetivo,
        MatriculaRestricoes restricoes,
        string? observacoesRestricoes,
        Arquivo? laudoMedico)
    {
        if (aluno is null)
            throw new DomainException("ALUNO_OBRIGATORIO");

        var notifications = new List<Notification>();

        if (!Enum.IsDefined(plano))
            notifications.Add(new Notification("Plano", "PLANO_INVALIDO"));

        if (dataInicio == default)
            notifications.Add(new Notification("DataInicio", "DATA_INICIO_OBRIGATORIA"));

        if (dataFim <= dataInicio)
            notifications.Add(new Notification("DataFim", "DATA_FIM_ANTERIOR_INICIO"));

        if (NormalizadoService.TextoVazioOuNulo(objetivo))
            notifications.Add(new Notification("Objetivo", "OBJETIVO_OBRIGATORIO"));
        else
            objetivo = NormalizadoService.LimparEspacos(objetivo);

        var idade = CalcularIdade(aluno.DataNascimento, dataInicio);
        var exigeLaudoPorIdade = idade >= IdadeMinimaExigeLaudo && idade <= IdadeMaximaExigeLaudo;
        var exigeLaudoPorRestricao = restricoes != MatriculaRestricoes.None;

        if ((exigeLaudoPorIdade || exigeLaudoPorRestricao) && laudoMedico is null)
            notifications.Add(new Notification("LaudoMedico", "LAUDO_MEDICO_OBRIGATORIO"));

        var observacoesLimpas = NormalizadoService.LimparEspacos(observacoesRestricoes);

        if (notifications.Count != 0)
            return Result<Matricula>.Failure(notifications);

        var matricula = new Matricula(
            id, aluno, plano, dataInicio, dataFim, objetivo, restricoes, observacoesLimpas, laudoMedico);

        return Result<Matricula>.Success(matricula);
    }

    private static int CalcularIdade(DateOnly nascimento, DateOnly dataReferencia)
    {
        var idade = dataReferencia.Year - nascimento.Year;
        if (nascimento > dataReferencia.AddYears(-idade))
            idade--;

        return idade;
    }
}
