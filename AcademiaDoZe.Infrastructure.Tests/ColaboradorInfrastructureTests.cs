/*
 * - Task<Colaborador?> ObterPorId(int id, CancellationToken cancellationToken = default)
 * - Task<IEnumerable<Colaborador>> ObterTodos(CancellationToken cancellationToken = default)
 * - Task<Colaborador> Adicionar(Colaborador entity, CancellationToken cancellationToken = default)
 * - Task<Colaborador> Atualizar(Colaborador entity, CancellationToken cancellationToken = default)
 * - Task<bool> Remover(int id, CancellationToken cancellationToken = default)
 * - Task<Colaborador?> ObterPorCpf(Cpf cpf, CancellationToken cancellationToken = default)
 * - Task<Colaborador?> ObterPorEmail(Email email, CancellationToken cancellationToken = default)
 * - Task<bool> CpfJaExiste(Cpf cpf, int? id = null, CancellationToken cancellationToken = default)
 * - Task<bool> EmailJaExiste(Email email, int? id = null, CancellationToken cancellationToken = default)
 * - Task<IEnumerable<Colaborador>> ObterPorTipo(ColaboradorTipo tipo, CancellationToken cancellationToken = default)
 * - Task<IEnumerable<Colaborador>> ObterPorVinculo(ColaboradorVinculo vinculo, CancellationToken cancellationToken = default)
 * - Task<bool> TrocarSenha(int id, Senha novaSenha, CancellationToken cancellationToken = default)
 */

using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Exceptions;
using AcademiaDoZe.Infrastructure.Repositories;

namespace AcademiaDoZe.Infrastructure.Tests;

public class ColaboradorInfrastructureTests : TestBase
{
    private readonly LogradouroRepository _logradouroRepo;
    private readonly ColaboradorRepository _colaboradorRepo;

    public ColaboradorInfrastructureTests()
    {
        _logradouroRepo = new LogradouroRepository(ConnectionString, DatabaseType);
        _colaboradorRepo = new ColaboradorRepository(ConnectionString, DatabaseType);
    }

    // Nome = nome do aluno, Complemento = sobrenome, Senha = contém a sigla do SGBD (exigências da entrega).
    internal static async Task<Colaborador> CriarEInserirColaboradorAsync(ColaboradorRepository colaboradorRepo, LogradouroRepository logradouroRepo)
    {
        var logradouro = await LogradouroInfrastructureTests.CriarEInserirLogradouroAsync(logradouroRepo);
        var foto = Arquivo.Criar(new byte[] { 5, 6, 7, 8 }).Value!;

        var colaboradorResult = Colaborador.Criar(
            id: 0,
            nome: NomeParaEntrega,
            cpf: GerarCpf(),
            dataNascimento: new DateOnly(1995, 5, 15),
            telefone: GerarTelefone(),
            email: GerarEmail(),
            logradouro: logradouro,
            numero: "200",
            complemento: SobrenomeParaEntrega,
            senha: SenhaParaEntrega,
            foto: foto,
            dataAdmissao: new DateOnly(2023, 1, 1),
            tipo: ColaboradorTipo.Instrutor,
            vinculo: ColaboradorVinculo.CLT
        );

        if (colaboradorResult.IsFailure)
        {
            throw new Exception($"Falha ao criar Colaborador: {string.Join(", ", colaboradorResult.Notifications.Select(n => n.Mensagem))}");
        }

        return await colaboradorRepo.Adicionar(colaboradorResult.Value!);
    }

    [Fact]
    public async Task Colaborador_Adicionar_E_ObterPorId_Sucesso()
    {
        var colaborador = await CriarEInserirColaboradorAsync(_colaboradorRepo, _logradouroRepo);

        Assert.NotNull(colaborador);
        Assert.True(colaborador.Id > 0);

        var obtido = await _colaboradorRepo.ObterPorId(colaborador.Id);
        Assert.NotNull(obtido);
        Assert.Equal(colaborador.Id, obtido.Id);
        Assert.Equal(colaborador.Cpf.Valor, obtido.Cpf.Valor);
        Assert.Equal(NomeParaEntrega, obtido.Nome);
        Assert.Equal(SobrenomeParaEntrega, obtido.Endereco.Complemento);
        Assert.Equal(SenhaParaEntrega, obtido.Senha.Valor);
        Assert.Equal(colaborador.Email!.Valor, obtido.Email!.Valor);
        Assert.Equal(colaborador.Tipo, obtido.Tipo);
        Assert.Equal(colaborador.Vinculo, obtido.Vinculo);
        Assert.NotNull(obtido.Endereco);
        Assert.Equal(colaborador.Endereco.Logradouro.Id, obtido.Endereco.Logradouro.Id);
    }

    [Fact]
    public async Task Colaborador_ObterPorId_RetornaNuloQuandoInexistente()
    {
        var obtido = await _colaboradorRepo.ObterPorId(999999);
        Assert.Null(obtido);
    }

    [Fact]
    public async Task Colaborador_ObterTodos_Sucesso()
    {
        await CriarEInserirColaboradorAsync(_colaboradorRepo, _logradouroRepo);

        var todos = await _colaboradorRepo.ObterTodos();

        Assert.NotNull(todos);
        Assert.NotEmpty(todos);
    }

    [Fact]
    public async Task Colaborador_Atualizar_Sucesso()
    {
        var colaborador = await CriarEInserirColaboradorAsync(_colaboradorRepo, _logradouroRepo);
        var novoNome = $"{NomeParaEntrega} Editado";

        var colaboradorAtualizado = Colaborador.Criar(
            id: colaborador.Id,
            nome: novoNome,
            cpf: colaborador.Cpf.Valor,
            dataNascimento: colaborador.DataNascimento,
            telefone: colaborador.Telefone.Valor,
            email: colaborador.Email!.Valor,
            logradouro: colaborador.Endereco.Logradouro,
            numero: "300",
            complemento: SobrenomeParaEntrega,
            senha: colaborador.Senha.Valor,
            foto: colaborador.Foto,
            dataAdmissao: colaborador.DataAdmissao,
            tipo: ColaboradorTipo.Administrador,
            vinculo: ColaboradorVinculo.CLT
        ).Value!;

        var resultado = await _colaboradorRepo.Atualizar(colaboradorAtualizado);

        Assert.NotNull(resultado);
        Assert.Equal(novoNome, resultado.Nome);
        Assert.Equal(ColaboradorTipo.Administrador, resultado.Tipo);

        var noBanco = await _colaboradorRepo.ObterPorId(colaborador.Id);
        Assert.NotNull(noBanco);
        Assert.Equal(novoNome, noBanco.Nome);
        Assert.Equal(ColaboradorTipo.Administrador, noBanco.Tipo);
        Assert.Equal("300", noBanco.Endereco.Numero);
    }

    [Fact]
    public async Task Colaborador_Atualizar_LancaExcecaoQuandoInexistente()
    {
        var logradouro = await LogradouroInfrastructureTests.CriarEInserirLogradouroAsync(_logradouroRepo);
        var foto = Arquivo.Criar(new byte[] { 1, 2 }).Value!;

        var colaboradorInexistente = Colaborador.Criar(
            id: 999999,
            nome: NomeParaEntrega,
            cpf: GerarCpf(),
            dataNascimento: new DateOnly(1990, 1, 1),
            telefone: GerarTelefone(),
            email: GerarEmail(),
            logradouro: logradouro,
            numero: "1",
            complemento: SobrenomeParaEntrega,
            senha: SenhaParaEntrega,
            foto: foto,
            dataAdmissao: new DateOnly(2020, 1, 1),
            tipo: ColaboradorTipo.Atendente,
            vinculo: ColaboradorVinculo.CLT
        ).Value!;

        var ex = await Assert.ThrowsAsync<InfrastructureException>(() => _colaboradorRepo.Atualizar(colaboradorInexistente));
        Assert.Equal("REGISTRO_NAO_ENCONTRADO", ex.ErrorCode);
    }

    [Fact]
    public async Task Colaborador_Remover_Sucesso()
    {
        var colaborador = await CriarEInserirColaboradorAsync(_colaboradorRepo, _logradouroRepo);

        var removido = await _colaboradorRepo.Remover(colaborador.Id);
        Assert.True(removido);

        var noBanco = await _colaboradorRepo.ObterPorId(colaborador.Id);
        Assert.Null(noBanco);
    }

    [Fact]
    public async Task Colaborador_Remover_RetornaFalseQuandoInexistente()
    {
        var removido = await _colaboradorRepo.Remover(999999);
        Assert.False(removido);
    }

    [Fact]
    public async Task Colaborador_ObterPorCpf_SucessoENulo()
    {
        var colaborador = await CriarEInserirColaboradorAsync(_colaboradorRepo, _logradouroRepo);

        var obtido = await _colaboradorRepo.ObterPorCpf(colaborador.Cpf);
        Assert.NotNull(obtido);
        Assert.Equal(colaborador.Id, obtido.Id);

        var cpfInexistente = Cpf.Criar(GerarCpf()).Value!;
        var naoObtido = await _colaboradorRepo.ObterPorCpf(cpfInexistente);
        Assert.Null(naoObtido);
    }

    [Fact]
    public async Task Colaborador_ObterPorEmail_SucessoENulo()
    {
        var colaborador = await CriarEInserirColaboradorAsync(_colaboradorRepo, _logradouroRepo);

        var obtido = await _colaboradorRepo.ObterPorEmail(colaborador.Email!);
        Assert.NotNull(obtido);
        Assert.Equal(colaborador.Id, obtido.Id);

        var emailInexistente = Email.Criar(GerarEmail()).Value!;
        var naoObtido = await _colaboradorRepo.ObterPorEmail(emailInexistente);
        Assert.Null(naoObtido);
    }

    [Fact]
    public async Task Colaborador_CpfJaExiste_ValidacaoCorreta()
    {
        var colaborador = await CriarEInserirColaboradorAsync(_colaboradorRepo, _logradouroRepo);

        var existe = await _colaboradorRepo.CpfJaExiste(colaborador.Cpf);
        Assert.True(existe);

        var existeIgnorandoId = await _colaboradorRepo.CpfJaExiste(colaborador.Cpf, colaborador.Id);
        Assert.False(existeIgnorandoId);

        var cpfInedito = Cpf.Criar(GerarCpf()).Value!;
        var existeInedito = await _colaboradorRepo.CpfJaExiste(cpfInedito);
        Assert.False(existeInedito);
    }

    [Fact]
    public async Task Colaborador_EmailJaExiste_ValidacaoCorreta()
    {
        var colaborador = await CriarEInserirColaboradorAsync(_colaboradorRepo, _logradouroRepo);

        var existe = await _colaboradorRepo.EmailJaExiste(colaborador.Email!);
        Assert.True(existe);

        var existeIgnorandoId = await _colaboradorRepo.EmailJaExiste(colaborador.Email!, colaborador.Id);
        Assert.False(existeIgnorandoId);

        var emailInedito = Email.Criar(GerarEmail()).Value!;
        var existeInedito = await _colaboradorRepo.EmailJaExiste(emailInedito);
        Assert.False(existeInedito);
    }

    [Fact]
    public async Task Colaborador_ObterPorTipo_FiltragemCorreta()
    {
        var colaborador = await CriarEInserirColaboradorAsync(_colaboradorRepo, _logradouroRepo);

        var resultados = await _colaboradorRepo.ObterPorTipo(colaborador.Tipo);
        Assert.NotNull(resultados);
        Assert.Contains(resultados, c => c.Id == colaborador.Id);
    }

    [Fact]
    public async Task Colaborador_ObterPorVinculo_FiltragemCorreta()
    {
        var colaborador = await CriarEInserirColaboradorAsync(_colaboradorRepo, _logradouroRepo);

        var resultados = await _colaboradorRepo.ObterPorVinculo(colaborador.Vinculo);
        Assert.NotNull(resultados);
        Assert.Contains(resultados, c => c.Id == colaborador.Id);
    }

    [Fact]
    public async Task Colaborador_TrocarSenha_SucessoEFalha()
    {
        var colaborador = await CriarEInserirColaboradorAsync(_colaboradorRepo, _logradouroRepo);
        var novaSenhaTexto = $"NovaSenha123{NomeSgbdParaEntrega}";
        var novaSenha = Senha.Criar(novaSenhaTexto).Value!;

        var alterou = await _colaboradorRepo.TrocarSenha(colaborador.Id, novaSenha);
        Assert.True(alterou);

        var atualizado = await _colaboradorRepo.ObterPorId(colaborador.Id);
        Assert.NotNull(atualizado);
        Assert.Equal(novaSenhaTexto, atualizado.Senha.Valor);

        var alterouInexistente = await _colaboradorRepo.TrocarSenha(999999, novaSenha);
        Assert.False(alterouInexistente);
    }
}
