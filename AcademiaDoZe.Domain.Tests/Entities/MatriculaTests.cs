// gabriel geremias vieira
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.Exceptions;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Tests.Entities;

public class MatriculaTests
{
    private static Logradouro GetValidLogradouro() =>
        Logradouro.Criar(1, "12345-678", "Rua Teste", "Bairro", "Cidade", "SP", "Brasil").Value!;

    private static Arquivo GetValidArquivo() => Arquivo.Criar([1, 2, 3]).Value!;

    private static Aluno GetValidAluno(DateOnly? dataNascimento = null)
    {
        var nascimento = dataNascimento ?? DateOnly.FromDateTime(DateTime.Today.AddYears(-20));
        return Aluno.Criar(
            1, "João da Silva", "529.982.247-25", nascimento, "(11) 91234-5678",
            "user@example.com", GetValidLogradouro(), "123", "", "Abcdef1", null).Value!;
    }

    [Fact(DisplayName = "Matricula: aluno nulo lança DomainException")]
    public void Deve_Lancar_DomainException_Quando_AlunoNulo()
    {
        var inicio = DateOnly.FromDateTime(DateTime.Today);

        Assert.Throws<DomainException>(() =>
            Matricula.Criar(1, null!, MatriculaPlano.Mensal, inicio, inicio.AddMonths(1), "Objetivo", MatriculaRestricoes.None, "", null));
    }

    [Fact(DisplayName = "Matricula: plano inválido -> PLANO_INVALIDO")]
    public void Deve_Falhar_Criacao_Quando_PlanoInvalido()
    {
        var aluno = GetValidAluno();
        var inicio = DateOnly.FromDateTime(DateTime.Today);

        var result = Matricula.Criar(1, aluno, (MatriculaPlano)999, inicio, inicio.AddMonths(1), "Objetivo", MatriculaRestricoes.None, "", null);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "PLANO_INVALIDO");
    }

    [Fact(DisplayName = "Matricula: data de início padrão -> DATA_INICIO_OBRIGATORIA")]
    public void Deve_Falhar_Criacao_Quando_DataInicioPadrao()
    {
        var aluno = GetValidAluno();

        var result = Matricula.Criar(1, aluno, MatriculaPlano.Mensal, default, DateOnly.FromDateTime(DateTime.Today), "Objetivo", MatriculaRestricoes.None, "", null);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "DATA_INICIO_OBRIGATORIA");
    }

    [Theory(DisplayName = "Matricula: data fim anterior ou igual ao início -> DATA_FIM_ANTERIOR_INICIO")]
    [InlineData(0)]
    [InlineData(-1)]
    public void Deve_Falhar_Criacao_Quando_DataFimNaoPosteriorAInicio(int diasAposInicio)
    {
        var aluno = GetValidAluno();
        var inicio = DateOnly.FromDateTime(DateTime.Today);
        var fim = inicio.AddDays(diasAposInicio);

        var result = Matricula.Criar(1, aluno, MatriculaPlano.Mensal, inicio, fim, "Objetivo", MatriculaRestricoes.None, "", null);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "DATA_FIM_ANTERIOR_INICIO");
    }

    [Theory(DisplayName = "Matricula: objetivo vazio -> OBJETIVO_OBRIGATORIO")]
    [InlineData("")]
    [InlineData(" ")]
    public void Deve_Falhar_Criacao_Quando_ObjetivoVazio(string objetivo)
    {
        var aluno = GetValidAluno();
        var inicio = DateOnly.FromDateTime(DateTime.Today);

        var result = Matricula.Criar(1, aluno, MatriculaPlano.Mensal, inicio, inicio.AddMonths(1), objetivo, MatriculaRestricoes.None, "", null);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "OBJETIVO_OBRIGATORIO");
    }

    [Theory(DisplayName = "Matricula: aluno entre 12 e 16 anos exige laudo médico -> LAUDO_MEDICO_OBRIGATORIO")]
    [InlineData(12)]
    [InlineData(14)]
    [InlineData(16)]
    public void Deve_Falhar_Criacao_Quando_IdadeExigeLaudoESemLaudo(int idade)
    {
        var aluno = GetValidAluno(DateOnly.FromDateTime(DateTime.Today.AddYears(-idade)));
        var inicio = DateOnly.FromDateTime(DateTime.Today);

        var result = Matricula.Criar(1, aluno, MatriculaPlano.Mensal, inicio, inicio.AddMonths(1), "Objetivo", MatriculaRestricoes.None, "", null);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "LAUDO_MEDICO_OBRIGATORIO");
    }

    [Theory(DisplayName = "Matricula: fora da faixa 12-16 sem restrições não exige laudo")]
    [InlineData(10)]
    [InlineData(20)]
    public void Deve_Criar_Com_Sucesso_Quando_IdadeForaDaFaixaESemRestricoes(int idade)
    {
        var aluno = GetValidAluno(DateOnly.FromDateTime(DateTime.Today.AddYears(-idade)));
        var inicio = DateOnly.FromDateTime(DateTime.Today);

        var result = Matricula.Criar(1, aluno, MatriculaPlano.Mensal, inicio, inicio.AddMonths(1), "Objetivo", MatriculaRestricoes.None, "", null);

        Assert.True(result.IsSuccess);
    }

    [Fact(DisplayName = "Matricula: com restrição médica e sem laudo -> LAUDO_MEDICO_OBRIGATORIO")]
    public void Deve_Falhar_Criacao_Quando_RestricaoSemLaudo()
    {
        var aluno = GetValidAluno();
        var inicio = DateOnly.FromDateTime(DateTime.Today);

        var result = Matricula.Criar(1, aluno, MatriculaPlano.Mensal, inicio, inicio.AddMonths(1), "Objetivo", MatriculaRestricoes.Diabetes, "obs", null);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "LAUDO_MEDICO_OBRIGATORIO");
    }

    [Fact(DisplayName = "Matricula: com restrição médica e laudo cria com sucesso")]
    public void Deve_Criar_Com_Sucesso_Quando_RestricaoComLaudo()
    {
        var aluno = GetValidAluno();
        var inicio = DateOnly.FromDateTime(DateTime.Today);

        var result = Matricula.Criar(1, aluno, MatriculaPlano.Mensal, inicio, inicio.AddMonths(1), "Objetivo", MatriculaRestricoes.Diabetes, "obs", GetValidArquivo());

        Assert.True(result.IsSuccess);
        Assert.Equal(MatriculaRestricoes.Diabetes, result.Value!.Restricoes);
    }

    [Theory(DisplayName = "Matricula: normaliza observações de restrições removendo espaços extras")]
    [InlineData(" observa  testo ", "observa testo")]
    [InlineData(" obs  outro ", "obs outro")]
    public void Deve_Normalizar_ObservacoesRestricoes_Quando_InputTemEspacosExtras(string input, string expected)
    {
        var aluno = GetValidAluno();
        var inicio = DateOnly.FromDateTime(DateTime.Today);

        var result = Matricula.Criar(1, aluno, MatriculaPlano.Mensal, inicio, inicio.AddMonths(1), "Objetivo", MatriculaRestricoes.Diabetes, input, GetValidArquivo());

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value!.ObservacoesRestricoes);
    }

    [Theory(DisplayName = "Matricula: criação bem-sucedida para diferentes planos")]
    [InlineData(MatriculaPlano.Mensal)]
    [InlineData(MatriculaPlano.Trimestral)]
    [InlineData(MatriculaPlano.Semestral)]
    [InlineData(MatriculaPlano.Anual)]
    public void Deve_Criar_Com_Sucesso_Quando_PlanoValido(MatriculaPlano plano)
    {
        var aluno = GetValidAluno();
        var inicio = DateOnly.FromDateTime(DateTime.Today);

        var result = Matricula.Criar(1, aluno, plano, inicio, inicio.AddMonths(1), "Melhorar condicionamento", MatriculaRestricoes.None, "", null);

        Assert.True(result.IsSuccess);
        Assert.Equal(aluno, result.Value!.Aluno);
        Assert.Equal(plano, result.Value.Plano);
    }
}
