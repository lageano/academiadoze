// gabriel geremias vieira
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Tests.ValueObjects;

public class SenhaTests
{
    [Theory(DisplayName = "Senha: obrigatória -> SENHA_OBRIGATORIA")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Deve_Falhar_Criacao_Quando_SenhaNulaOuVazia(string? input)
    {
        var result = Senha.Criar(input!);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "SENHA_OBRIGATORIA");
    }

    [Theory(DisplayName = "Senha: menor que o tamanho mínimo -> SENHA_TAMANHO_MINIMO")]
    [InlineData("Ab1")]
    [InlineData("a1b2")]
    public void Deve_Falhar_Criacao_Quando_SenhaMenorQueTamanhoMinimo(string input)
    {
        var result = Senha.Criar(input);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "SENHA_TAMANHO_MINIMO");
    }

    [Theory(DisplayName = "Senha: sem letra ou sem número -> SENHA_LETRA_NUMERO")]
    [InlineData("abcdef")]
    [InlineData("123456")]
    public void Deve_Falhar_Criacao_Quando_SenhaSemLetraOuSemNumero(string input)
    {
        var result = Senha.Criar(input);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "SENHA_LETRA_NUMERO");
    }

    [Theory(DisplayName = "Senha: válida com letra e número")]
    [InlineData("Abcde1")]
    [InlineData("abc123")]
    [InlineData("Senha@123")]
    public void Deve_Criar_Senha_Quando_Valida(string input)
    {
        var result = Senha.Criar(input);

        Assert.True(result.IsSuccess);
        Assert.Equal(input, result.Value!.Valor);
    }
}
