// gabriel geremias vieira
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Tests;

public class CpfTests
{
    [Theory(DisplayName = "Cpf: nulo/vazio/espaços -> CPF_OBRIGATORIO")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Deve_Falhar_Criacao_Quando_CpfNuloOuVazio(string? input)
    {
        var result = Cpf.Criar(input!);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "CPF_OBRIGATORIO");
    }

    [Theory(DisplayName = "Cpf: quantidade de dígitos incorreta -> CPF_DIGITOS")]
    [InlineData("123")]
    [InlineData("abc")]
    [InlineData("123456789012")]
    public void Deve_Falhar_Criacao_Quando_CpfComQuantidadeDigitosInvalida(string input)
    {
        var result = Cpf.Criar(input);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "CPF_DIGITOS");
    }

    [Theory(DisplayName = "Cpf: todos os dígitos iguais -> CPF_INVALIDO")]
    [InlineData("00000000000")]
    [InlineData("11111111111")]
    public void Deve_Falhar_Criacao_Quando_CpfComDigitosRepetidos(string input)
    {
        var result = Cpf.Criar(input);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "CPF_INVALIDO");
    }

    [Theory(DisplayName = "Cpf: dígito verificador incorreto -> CPF_INVALIDO")]
    [InlineData("123.456.789-00")]
    [InlineData("529.982.247-00")]
    public void Deve_Falhar_Criacao_Quando_CpfComDigitoVerificadorInvalido(string input)
    {
        var result = Cpf.Criar(input);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "CPF_INVALIDO");
    }

    [Theory(DisplayName = "Cpf: formatos válidos (com e sem pontuação)")]
    [InlineData("529.982.247-25", "52998224725")]
    [InlineData("52998224725", "52998224725")]
    [InlineData("111.444.777-35", "11144477735")]
    public void Deve_Criar_Cpf_Quando_Valido(string input, string expected)
    {
        var result = Cpf.Criar(input);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value!.Valor);
    }
}
