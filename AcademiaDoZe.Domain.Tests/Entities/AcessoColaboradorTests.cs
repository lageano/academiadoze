// gabriel geremias vieira
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.Exceptions;

namespace AcademiaDoZe.Domain.Tests.Entities;

public class AcessoColaboradorTests
{
    private static Logradouro GetValidLogradouro() =>
        Logradouro.Criar(1, "12345-678", "Rua Teste", "Bairro", "Cidade", "SP", "Brasil").Value!;

    private static Colaborador GetValidColaborador() => Colaborador.Criar(
        1, "Fulano", "529.982.247-25", DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
        "(11) 91234-5678", "user@example.com", GetValidLogradouro(), "123", "", "Abcdef1", null,
        DateOnly.FromDateTime(DateTime.Today.AddYears(-1)), ColaboradorTipo.Atendente, ColaboradorVinculo.CLT).Value!;

    [Fact(DisplayName = "AcessoColaborador: colaborador nulo lança DomainException")]
    public void Deve_Lancar_DomainException_Quando_ColaboradorNulo()
    {
        Assert.Throws<DomainException>(() =>
            AcessoColaborador.Criar(1, null!, DateOnly.FromDateTime(DateTime.Today), new TimeOnly(10, 0)));
    }

    [Fact(DisplayName = "AcessoColaborador: criação bem-sucedida sem saída registrada")]
    public void Deve_Criar_Com_Sucesso_SemSaidaRegistrada()
    {
        var colaborador = GetValidColaborador();
        var data = DateOnly.FromDateTime(DateTime.Today);
        var entrada = new TimeOnly(8, 0);

        var result = AcessoColaborador.Criar(1, colaborador, data, entrada);

        Assert.True(result.IsSuccess);
        Assert.Equal(colaborador, result.Value!.Colaborador);
        Assert.Equal(data, result.Value.Data);
        Assert.Equal(entrada, result.Value.Entrada);
        Assert.Null(result.Value.Saida);
    }

    [Fact(DisplayName = "AcessoColaborador: RegistrarSaida com sucesso quando posterior à entrada")]
    public void Deve_RegistrarSaida_Quando_PosteriorAEntrada()
    {
        var colaborador = GetValidColaborador();
        var acesso = AcessoColaborador.Criar(1, colaborador, DateOnly.FromDateTime(DateTime.Today), new TimeOnly(8, 0)).Value!;

        acesso.RegistrarSaida(new TimeOnly(17, 0));

        Assert.Equal(new TimeOnly(17, 0), acesso.Saida);
    }

    [Fact(DisplayName = "AcessoColaborador: RegistrarSaida duas vezes lança DomainException")]
    public void Deve_Lancar_DomainException_Quando_SaidaJaRegistrada()
    {
        var colaborador = GetValidColaborador();
        var acesso = AcessoColaborador.Criar(1, colaborador, DateOnly.FromDateTime(DateTime.Today), new TimeOnly(8, 0)).Value!;
        acesso.RegistrarSaida(new TimeOnly(12, 0));

        Assert.Throws<DomainException>(() => acesso.RegistrarSaida(new TimeOnly(17, 0)));
    }

    [Fact(DisplayName = "AcessoColaborador: RegistrarSaida anterior à entrada lança DomainException")]
    public void Deve_Lancar_DomainException_Quando_SaidaAnteriorAEntrada()
    {
        var colaborador = GetValidColaborador();
        var acesso = AcessoColaborador.Criar(1, colaborador, DateOnly.FromDateTime(DateTime.Today), new TimeOnly(10, 0)).Value!;

        Assert.Throws<DomainException>(() => acesso.RegistrarSaida(new TimeOnly(9, 0)));
    }

    [Theory(DisplayName = "AcessoColaborador: id negativo lança DomainException")]
    [InlineData(-1)]
    [InlineData(-5)]
    public void Deve_Lancar_DomainException_Quando_IdNegativo(int id)
    {
        var colaborador = GetValidColaborador();

        Assert.Throws<DomainException>(() =>
            AcessoColaborador.Criar(id, colaborador, DateOnly.FromDateTime(DateTime.Today), new TimeOnly(8, 0)));
    }
}
