// gabriel geremias vieira
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Tests;

public class CepTests
{
    [Theory(DisplayName = "Cep: obrigatório -> CEP_OBRIGATORIO")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Deve_Falhar_Criacao_Quando_CepNuloOuVazio(string? input)
    {
        var result = Cep.Criar(input!);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "CEP_OBRIGATORIO");
    }

    [Theory(DisplayName = "Cep: dígitos inválidos -> CEP_DIGITOS")]
    [InlineData("123")]
    [InlineData("12-345")]
    [InlineData("123456789")]
    public void Deve_Falhar_Criacao_Quando_CepDigitosInvalidos(string input)
    {
        var result = Cep.Criar(input);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "CEP_DIGITOS");
    }

    [Theory(DisplayName = "Cep: formatos válidos (com e sem hífen)")]
    [InlineData("12345-678")]
    [InlineData("12345678")]
    public void Deve_Criar_Cep_Quando_Valido(string input)
    {
        var result = Cep.Criar(input);

        Assert.True(result.IsSuccess);
        Assert.Equal("12345678", result.Value!.Valor);
    }
}
