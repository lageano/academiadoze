// gabriel geremias vieira
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Exceptions;

namespace AcademiaDoZe.Domain.Tests.Entities;

public class LogradouroTests
{
    [Theory(DisplayName = "Logradouro: nome vazio -> NOME_OBRIGATORIO")]
    [InlineData("")]
    [InlineData(" ")]
    public void Deve_Falhar_Criacao_Quando_NomeVazio(string nome)
    {
        var result = Logradouro.Criar(1, "12345-678", nome, "Bairro", "Cidade", "SP", "Brasil");

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "NOME_OBRIGATORIO");
    }

    [Fact(DisplayName = "Logradouro: bairro vazio -> BAIRRO_OBRIGATORIO")]
    public void Deve_Falhar_Criacao_Quando_BairroVazio()
    {
        var result = Logradouro.Criar(1, "12345-678", "Rua", "", "Cidade", "SP", "Brasil");

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "BAIRRO_OBRIGATORIO");
    }

    [Fact(DisplayName = "Logradouro: cidade vazia -> CIDADE_OBRIGATORIA")]
    public void Deve_Falhar_Criacao_Quando_CidadeVazia()
    {
        var result = Logradouro.Criar(1, "12345-678", "Rua", "Bairro", "", "SP", "Brasil");

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "CIDADE_OBRIGATORIA");
    }

    [Fact(DisplayName = "Logradouro: estado vazio -> ESTADO_OBRIGATORIO")]
    public void Deve_Falhar_Criacao_Quando_EstadoVazio()
    {
        var result = Logradouro.Criar(1, "12345-678", "Rua", "Bairro", "Cidade", "", "Brasil");

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "ESTADO_OBRIGATORIO");
    }

    [Fact(DisplayName = "Logradouro: país vazio -> PAIS_OBRIGATORIO")]
    public void Deve_Falhar_Criacao_Quando_PaisVazio()
    {
        var result = Logradouro.Criar(1, "12345-678", "Rua", "Bairro", "Cidade", "SP", "");

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "PAIS_OBRIGATORIO");
    }

    [Fact(DisplayName = "Logradouro: cep inválido propaga notificação -> CEP_DIGITOS")]
    public void Deve_Falhar_Criacao_Quando_CepInvalido()
    {
        var result = Logradouro.Criar(1, "123", "Rua", "Bairro", "Cidade", "SP", "Brasil");

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "CEP_DIGITOS");
    }

    [Theory(DisplayName = "Logradouro: normaliza estado removendo espaços e para maiúsculo")]
    [InlineData(" s p ", "SP")]
    [InlineData(" sp ", "SP")]
    [InlineData("rj", "RJ")]
    public void Deve_Normalizar_Estado_Quando_InputContemEspacos(string inputEstado, string expected)
    {
        var result = Logradouro.Criar(1, "12345-678", "Rua", "Bairro", "Cidade", inputEstado, "Brasil");

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value!.Estado);
    }

    [Theory(DisplayName = "Logradouro: id negativo lança DomainException")]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Deve_Lancar_DomainException_Quando_IdNegativo(int id)
    {
        Assert.Throws<DomainException>(() =>
            Logradouro.Criar(id, "12345-678", "Rua", "Bairro", "Cidade", "SP", "Brasil"));
    }

    [Theory(DisplayName = "Logradouro: id zero ou positivo cria com sucesso")]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(50)]
    public void Deve_Criar_Com_Sucesso_Quando_IdValido(int id)
    {
        var result = Logradouro.Criar(id, "12345-678", "Rua", "Bairro", "Cidade", "SP", "Brasil");

        Assert.True(result.IsSuccess);
        Assert.Equal(id, result.Value!.Id);
    }
}
