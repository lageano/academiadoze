// gabriel geremias vieira
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Exceptions;

namespace AcademiaDoZe.Domain.Tests.Entities;

public class AcessoAlunoTests
{
    private static Logradouro GetValidLogradouro() =>
        Logradouro.Criar(1, "12345-678", "Rua Teste", "Bairro", "Cidade", "SP", "Brasil").Value!;

    private static Aluno GetValidAluno() => Aluno.Criar(
        1, "João", "529.982.247-25", DateOnly.FromDateTime(DateTime.Today.AddYears(-20)),
        "(11) 91234-5678", "user@example.com", GetValidLogradouro(), "123", "", "Abcdef1", null).Value!;

    [Fact(DisplayName = "AcessoAluno: aluno nulo lança DomainException")]
    public void Deve_Lancar_DomainException_Quando_AlunoNulo()
    {
        Assert.Throws<DomainException>(() =>
            AcessoAluno.Criar(1, null!, DateOnly.FromDateTime(DateTime.Today), new TimeOnly(10, 0)));
    }

    [Fact(DisplayName = "AcessoAluno: criação bem-sucedida sem saída registrada")]
    public void Deve_Criar_Com_Sucesso_SemSaidaRegistrada()
    {
        var aluno = GetValidAluno();
        var data = DateOnly.FromDateTime(DateTime.Today);
        var entrada = new TimeOnly(8, 0);

        var result = AcessoAluno.Criar(1, aluno, data, entrada);

        Assert.True(result.IsSuccess);
        Assert.Equal(aluno, result.Value!.Aluno);
        Assert.Equal(data, result.Value.Data);
        Assert.Equal(entrada, result.Value.Entrada);
        Assert.Null(result.Value.Saida);
    }

    [Fact(DisplayName = "AcessoAluno: RegistrarSaida com sucesso quando posterior à entrada")]
    public void Deve_RegistrarSaida_Quando_PosteriorAEntrada()
    {
        var aluno = GetValidAluno();
        var acesso = AcessoAluno.Criar(1, aluno, DateOnly.FromDateTime(DateTime.Today), new TimeOnly(8, 0)).Value!;

        acesso.RegistrarSaida(new TimeOnly(9, 30));

        Assert.Equal(new TimeOnly(9, 30), acesso.Saida);
    }

    [Fact(DisplayName = "AcessoAluno: RegistrarSaida duas vezes lança DomainException")]
    public void Deve_Lancar_DomainException_Quando_SaidaJaRegistrada()
    {
        var aluno = GetValidAluno();
        var acesso = AcessoAluno.Criar(1, aluno, DateOnly.FromDateTime(DateTime.Today), new TimeOnly(8, 0)).Value!;
        acesso.RegistrarSaida(new TimeOnly(9, 0));

        Assert.Throws<DomainException>(() => acesso.RegistrarSaida(new TimeOnly(10, 0)));
    }

    [Fact(DisplayName = "AcessoAluno: RegistrarSaida anterior à entrada lança DomainException")]
    public void Deve_Lancar_DomainException_Quando_SaidaAnteriorAEntrada()
    {
        var aluno = GetValidAluno();
        var acesso = AcessoAluno.Criar(1, aluno, DateOnly.FromDateTime(DateTime.Today), new TimeOnly(10, 0)).Value!;

        Assert.Throws<DomainException>(() => acesso.RegistrarSaida(new TimeOnly(9, 0)));
    }

    [Theory(DisplayName = "AcessoAluno: id negativo lança DomainException")]
    [InlineData(-1)]
    [InlineData(-5)]
    public void Deve_Lancar_DomainException_Quando_IdNegativo(int id)
    {
        var aluno = GetValidAluno();

        Assert.Throws<DomainException>(() =>
            AcessoAluno.Criar(id, aluno, DateOnly.FromDateTime(DateTime.Today), new TimeOnly(8, 0)));
    }
}
