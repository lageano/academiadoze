// gabriel geremias vieira
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Tests.ValueObjects;

public class EmailTests
{
    [Theory(DisplayName = "Email: nulo/vazio -> EMAIL_FORMATO")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Deve_Falhar_Criacao_Quando_EmailNuloOuVazio(string? input)
    {
        var result = Email.Criar(input!);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "EMAIL_FORMATO");
    }

    [Theory(DisplayName = "Email: formatos inválidos -> EMAIL_FORMATO")]
    [InlineData("semarroba.com")]
    [InlineData("usuario@")]
    [InlineData("@dominio.com")]
    [InlineData("usuario@.com")]
    [InlineData("usuario@dominio.")]
    [InlineData("usuario@dominio")]
    public void Deve_Falhar_Criacao_Quando_EmailComFormatoInvalido(string input)
    {
        var result = Email.Criar(input);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "EMAIL_FORMATO");
    }

    [Theory(DisplayName = "Email: formatos válidos")]
    [InlineData("usuario@dominio.com")]
    [InlineData("usuario.nome@sub.dominio.com.br")]
    [InlineData(" usuario@dominio.com ")]
    public void Deve_Criar_Email_Quando_Valido(string input)
    {
        var result = Email.Criar(input);

        Assert.True(result.IsSuccess);
        Assert.Equal(input.Trim(), result.Value!.Valor);
    }
}
