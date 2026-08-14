// gabriel geremias vieira
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;

namespace AcademiaDoZe.Domain.Tests;

public class ColaboradorTests
{
    private static Logradouro GetValidLogradouro() =>
        Logradouro.Criar(1, "12345-678", "Rua Teste", "Bairro", "Cidade", "SP", "Brasil").Value!;

    private static Domain.Common.Result<Colaborador> CriarColaborador(
        DateOnly dataAdmissao, ColaboradorTipo tipo, ColaboradorVinculo vinculo) =>
        Colaborador.Criar(
            1, "Fulano", "529.982.247-25", DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
            "(11) 91234-5678", "user@example.com", GetValidLogradouro(), "123", "", "Abcdef1", null,
            dataAdmissao, tipo, vinculo);

    [Fact(DisplayName = "Colaborador: data de admissão padrão -> DATA_ADMISSAO_OBRIGATORIA")]
    public void Deve_Falhar_Criacao_Quando_DataAdmissaoPadrao()
    {
        var result = CriarColaborador(default, ColaboradorTipo.Atendente, ColaboradorVinculo.CLT);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "DATA_ADMISSAO_OBRIGATORIA");
    }

    [Fact(DisplayName = "Colaborador: data de admissão futura -> DATA_ADMISSAO_MAIOR_ATUAL")]
    public void Deve_Falhar_Criacao_Quando_DataAdmissaoFutura()
    {
        var data = DateOnly.FromDateTime(DateTime.Today.AddDays(1));

        var result = CriarColaborador(data, ColaboradorTipo.Atendente, ColaboradorVinculo.CLT);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "DATA_ADMISSAO_MAIOR_ATUAL");
    }

    [Fact(DisplayName = "Colaborador: Administrador com vínculo Estágio -> ADMINISTRADOR_CLT_INVALIDO")]
    public void Deve_Falhar_Criacao_Quando_AdministradorComVinculoEstagio()
    {
        var data = DateOnly.FromDateTime(DateTime.Today.AddYears(-1));

        var result = CriarColaborador(data, ColaboradorTipo.Administrador, ColaboradorVinculo.Estagio);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "ADMINISTRADOR_CLT_INVALIDO");
    }

    [Fact(DisplayName = "Colaborador: Administrador com vínculo CLT cria com sucesso")]
    public void Deve_Criar_Com_Sucesso_Quando_AdministradorComVinculoClt()
    {
        var data = DateOnly.FromDateTime(DateTime.Today.AddYears(-1));

        var result = CriarColaborador(data, ColaboradorTipo.Administrador, ColaboradorVinculo.CLT);

        Assert.True(result.IsSuccess);
        Assert.Equal(ColaboradorTipo.Administrador, result.Value!.Tipo);
    }

    [Theory(DisplayName = "Colaborador: tipo inválido -> TIPO_COLABORADOR_INVALIDO")]
    [InlineData(999)]
    [InlineData(-1)]
    public void Deve_Falhar_Criacao_Quando_TipoInvalido(int tipoValue)
    {
        var data = DateOnly.FromDateTime(DateTime.Today.AddYears(-1));
        var tipo = (ColaboradorTipo)tipoValue;

        var result = CriarColaborador(data, tipo, ColaboradorVinculo.CLT);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "TIPO_COLABORADOR_INVALIDO");
    }

    [Fact(DisplayName = "Colaborador: vínculo inválido -> VINCULO_COLABORADOR_INVALIDO")]
    public void Deve_Falhar_Criacao_Quando_VinculoInvalido()
    {
        var data = DateOnly.FromDateTime(DateTime.Today.AddYears(-1));
        var vinculo = (ColaboradorVinculo)999;

        var result = CriarColaborador(data, ColaboradorTipo.Atendente, vinculo);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "VINCULO_COLABORADOR_INVALIDO");
    }

    [Theory(DisplayName = "Colaborador: Atendente ou Instrutor com CLT ou Estágio criam com sucesso")]
    [InlineData(ColaboradorTipo.Atendente, ColaboradorVinculo.CLT)]
    [InlineData(ColaboradorTipo.Atendente, ColaboradorVinculo.Estagio)]
    [InlineData(ColaboradorTipo.Instrutor, ColaboradorVinculo.CLT)]
    [InlineData(ColaboradorTipo.Instrutor, ColaboradorVinculo.Estagio)]
    public void Deve_Criar_Com_Sucesso_Quando_TipoEVinculoValidos(ColaboradorTipo tipo, ColaboradorVinculo vinculo)
    {
        var data = DateOnly.FromDateTime(DateTime.Today.AddYears(-1));

        var result = CriarColaborador(data, tipo, vinculo);

        Assert.True(result.IsSuccess);
        Assert.Equal(tipo, result.Value!.Tipo);
        Assert.Equal(vinculo, result.Value.Vinculo);
    }
}
