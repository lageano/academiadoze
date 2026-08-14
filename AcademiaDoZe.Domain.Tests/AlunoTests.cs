// gabriel geremias vieira
using AcademiaDoZe.Domain.Entities;

namespace AcademiaDoZe.Domain.Tests;

public class AlunoTests
{
    private static Logradouro GetValidLogradouro() =>
        Logradouro.Criar(1, "12345-678", "Rua Teste", "Bairro", "Cidade", "SP", "Brasil").Value!;

    [Theory(DisplayName = "Aluno: criação bem-sucedida com nomes válidos (trim aplicado)")]
    [InlineData(" João da Silva ")]
    [InlineData("Maria")]
    public void Deve_Criar_Com_Sucesso_Quando_NomeValido(string nome)
    {
        var result = Aluno.Criar(
            1, nome, "529.982.247-25", DateOnly.FromDateTime(DateTime.Today.AddYears(-25)),
            "(11) 91234-5678", "user@example.com", GetValidLogradouro(), "123", "", "Abcdef1", null);

        Assert.True(result.IsSuccess);
        Assert.Equal(nome.Trim(), result.Value!.Nome);
    }

    [Theory(DisplayName = "Aluno: nome vazio -> NOME_OBRIGATORIO")]
    [InlineData("")]
    [InlineData(" ")]
    public void Deve_Falhar_Criacao_Quando_NomeVazio(string nome)
    {
        var result = Aluno.Criar(
            1, nome, "529.982.247-25", DateOnly.FromDateTime(DateTime.Today.AddYears(-25)),
            "(11) 91234-5678", "user@example.com", GetValidLogradouro(), "123", "", "Abcdef1", null);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "NOME_OBRIGATORIO");
    }

    [Fact(DisplayName = "Aluno: data de nascimento padrão -> DATA_NASCIMENTO_OBRIGATORIA")]
    public void Deve_Falhar_Criacao_Quando_DataNascimentoPadrao()
    {
        var result = Aluno.Criar(
            1, "João", "529.982.247-25", default,
            "(11) 91234-5678", "user@example.com", GetValidLogradouro(), "123", "", "Abcdef1", null);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "DATA_NASCIMENTO_OBRIGATORIA");
    }

    [Fact(DisplayName = "Aluno: data de nascimento futura -> DATA_NASCIMENTO_FUTURA_INVALIDA")]
    public void Deve_Falhar_Criacao_Quando_DataNascimentoFutura()
    {
        var result = Aluno.Criar(
            1, "João", "529.982.247-25", DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            "(11) 91234-5678", "user@example.com", GetValidLogradouro(), "123", "", "Abcdef1", null);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "DATA_NASCIMENTO_FUTURA_INVALIDA");
    }

    [Fact(DisplayName = "Aluno: cpf inválido propaga notificação -> CPF_INVALIDO")]
    public void Deve_Falhar_Criacao_Quando_CpfInvalido()
    {
        var result = Aluno.Criar(
            1, "João", "111.111.111-11", DateOnly.FromDateTime(DateTime.Today.AddYears(-25)),
            "(11) 91234-5678", "user@example.com", GetValidLogradouro(), "123", "", "Abcdef1", null);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "CPF_INVALIDO");
    }

    [Fact(DisplayName = "Aluno: telefone inválido propaga notificação -> TELEFONE_DIGITOS")]
    public void Deve_Falhar_Criacao_Quando_TelefoneInvalido()
    {
        var result = Aluno.Criar(
            1, "João", "529.982.247-25", DateOnly.FromDateTime(DateTime.Today.AddYears(-25)),
            "1234", "user@example.com", GetValidLogradouro(), "123", "", "Abcdef1", null);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "TELEFONE_DIGITOS");
    }

    [Fact(DisplayName = "Aluno: email inválido propaga notificação -> EMAIL_FORMATO")]
    public void Deve_Falhar_Criacao_Quando_EmailInvalido()
    {
        var result = Aluno.Criar(
            1, "João", "529.982.247-25", DateOnly.FromDateTime(DateTime.Today.AddYears(-25)),
            "(11) 91234-5678", "email-invalido", GetValidLogradouro(), "123", "", "Abcdef1", null);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "EMAIL_FORMATO");
    }

    [Fact(DisplayName = "Aluno: email nulo é opcional e cria com sucesso")]
    public void Deve_Criar_Com_Sucesso_Quando_EmailNulo()
    {
        var result = Aluno.Criar(
            1, "João", "529.982.247-25", DateOnly.FromDateTime(DateTime.Today.AddYears(-25)),
            "(11) 91234-5678", null, GetValidLogradouro(), "123", "", "Abcdef1", null);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value!.Email);
    }

    [Fact(DisplayName = "Aluno: senha inválida propaga notificação -> SENHA_TAMANHO_MINIMO")]
    public void Deve_Falhar_Criacao_Quando_SenhaInvalida()
    {
        var result = Aluno.Criar(
            1, "João", "529.982.247-25", DateOnly.FromDateTime(DateTime.Today.AddYears(-25)),
            "(11) 91234-5678", "user@example.com", GetValidLogradouro(), "123", "", "ab1", null);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "SENHA_TAMANHO_MINIMO");
    }

    [Fact(DisplayName = "Aluno: número de endereço obrigatório propaga notificação -> NUMERO_OBRIGATORIO")]
    public void Deve_Falhar_Criacao_Quando_NumeroEnderecoVazio()
    {
        var result = Aluno.Criar(
            1, "João", "529.982.247-25", DateOnly.FromDateTime(DateTime.Today.AddYears(-25)),
            "(11) 91234-5678", "user@example.com", GetValidLogradouro(), "", "", "Abcdef1", null);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "NUMERO_OBRIGATORIO");
    }

    [Fact(DisplayName = "Aluno: múltiplos campos inválidos acumulam notificações")]
    public void Deve_Acumular_Notificacoes_Quando_MultiplosCamposInvalidos()
    {
        var result = Aluno.Criar(
            1, "", "111.111.111-11", default,
            "1234", "invalido", GetValidLogradouro(), "", "", "ab", null);

        Assert.True(result.IsFailure);
        Assert.True(result.Notifications.Count >= 6);
    }
}
