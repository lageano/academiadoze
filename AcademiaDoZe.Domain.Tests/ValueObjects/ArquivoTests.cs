// gabriel geremias vieira
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Tests.ValueObjects;

public class ArquivoTests
{
    [Fact(DisplayName = "Arquivo: nulo -> ARQUIVO_OBRIGATORIO")]
    public void Deve_Falhar_Criacao_Quando_ConteudoNulo()
    {
        var result = Arquivo.Criar(null!);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "ARQUIVO_OBRIGATORIO");
    }

    [Fact(DisplayName = "Arquivo: vazio -> ARQUIVO_OBRIGATORIO")]
    public void Deve_Falhar_Criacao_Quando_ConteudoVazio()
    {
        var result = Arquivo.Criar([]);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "ARQUIVO_OBRIGATORIO");
    }

    [Fact(DisplayName = "Arquivo: acima do tamanho máximo -> ARQUIVO_TAMANHO")]
    public void Deve_Falhar_Criacao_Quando_ConteudoExcedeTamanhoMaximo()
    {
        var conteudo = new byte[16 * 1024 * 1024];

        var result = Arquivo.Criar(conteudo);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "ARQUIVO_TAMANHO");
    }

    [Theory(DisplayName = "Arquivo: criação válida preservando o conteúdo")]
    [InlineData(new byte[] { 1, 2, 3 })]
    [InlineData(new byte[] { 255 })]
    public void Deve_Criar_Arquivo_Quando_Valido(byte[] conteudo)
    {
        var result = Arquivo.Criar(conteudo);

        Assert.True(result.IsSuccess);
        Assert.Equal(conteudo, result.Value!.Conteudo);
    }
}
