// gabriel geremias vieira
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Tests.ValueObjects;

public class TelefoneTests
{
    [Theory(DisplayName = "Telefone: obrigatório -> TELEFONE_OBRIGATORIO")]
    [InlineData(null)]
    [InlineData("")]
    public void Deve_Falhar_Criacao_Quando_TelefoneNuloOuVazio(string? input)
    {
        var result = Telefone.Criar(input!);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "TELEFONE_OBRIGATORIO");
    }

    [Theory(DisplayName = "Telefone: dígitos inválidos -> TELEFONE_DIGITOS")]
    [InlineData("1234")]
    [InlineData("(1)2345")]
    public void Deve_Falhar_Criacao_Quando_TelefoneDigitosInvalidos(string input)
    {
        var result = Telefone.Criar(input);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "TELEFONE_DIGITOS");
    }

    [Theory(DisplayName = "Telefone: formatos válidos (com e sem formatação)")]
    [InlineData("(11) 91234-5678")]
    [InlineData("11912345678")]
    public void Deve_Criar_Telefone_Quando_Valido(string input)
    {
        var result = Telefone.Criar(input);

        Assert.True(result.IsSuccess);
        Assert.Equal("11912345678", result.Value!.Valor);
    }
}
