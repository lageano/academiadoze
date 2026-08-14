// gabriel geremias vieira
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Tests;

public class EnderecoTests
{
    private static Logradouro GetValidLogradouro() =>
        Logradouro.Criar(1, "12345-678", "Rua Teste", "Bairro", "Cidade", "SP", "Brasil").Value!;

    [Theory(DisplayName = "Endereco: criação válida com número e complemento")]
    [InlineData("10", "Bloco A")]
    [InlineData("1", "")]
    public void Deve_Criar_Endereco_Quando_Valido(string numero, string complemento)
    {
        var logradouro = GetValidLogradouro();

        var result = Endereco.Criar(logradouro, numero, complemento);

        Assert.True(result.IsSuccess);
        Assert.Equal(logradouro, result.Value!.Logradouro);
        Assert.Equal(numero, result.Value.Numero);
        Assert.Equal(complemento, result.Value.Complemento);
    }

    [Fact(DisplayName = "Endereco: logradouro obrigatório -> LOGRADOURO_OBRIGATORIO")]
    public void Deve_Falhar_Criacao_Quando_LogradouroNulo()
    {
        var result = Endereco.Criar(null!, "10", "");

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "LOGRADOURO_OBRIGATORIO");
    }

    [Theory(DisplayName = "Endereco: número obrigatório -> NUMERO_OBRIGATORIO")]
    [InlineData("")]
    [InlineData(" ")]
    public void Deve_Falhar_Criacao_Quando_NumeroVazio(string numero)
    {
        var logradouro = GetValidLogradouro();

        var result = Endereco.Criar(logradouro, numero, "");

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "NUMERO_OBRIGATORIO");
    }

    [Fact(DisplayName = "Endereco: complemento nulo é normalizado para vazio")]
    public void Deve_Normalizar_Complemento_Quando_Nulo()
    {
        var logradouro = GetValidLogradouro();

        var result = Endereco.Criar(logradouro, "10", null);

        Assert.True(result.IsSuccess);
        Assert.Equal(string.Empty, result.Value!.Complemento);
    }
}
