/*
 * - Task<Matricula?> ObterPorId(int id, CancellationToken cancellationToken = default)
 * - Task<IEnumerable<Matricula>> ObterTodos(CancellationToken cancellationToken = default)
 * - Task<Matricula> Adicionar(Matricula entity, CancellationToken cancellationToken = default)
 * - Task<Matricula> Atualizar(Matricula entity, CancellationToken cancellationToken = default)
 * - Task<bool> Remover(int id, CancellationToken cancellationToken = default)
 * - Task<IEnumerable<Matricula>> ObterPorAluno(int alunoId, CancellationToken cancellationToken = default)
 * - Task<Matricula?> ObterMatriculaAtivaPorAluno(int alunoId, CancellationToken cancellationToken = default)
 * - Task<bool> PossuiMatriculaAtiva(int alunoId, CancellationToken cancellationToken = default)
 * - Task<IEnumerable<Matricula>> ObterAtivas(int alunoId = 0, CancellationToken cancellationToken = default)
 * - Task<IEnumerable<Matricula>> ObterVencendoEmDias(int dias, CancellationToken cancellationToken = default)
 * - Task<IEnumerable<Matricula>> ObterPorPlano(MatriculaPlano plano, CancellationToken cancellationToken = default)
 */

using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Exceptions;
using AcademiaDoZe.Infrastructure.Repositories;

namespace AcademiaDoZe.Infrastructure.Tests;

public class MatriculaInfrastructureTests : TestBase
{
    private readonly LogradouroRepository _logradouroRepo;
    private readonly AlunoRepository _alunoRepo;
    private readonly MatriculaRepository _matriculaRepo;

    public MatriculaInfrastructureTests()
    {
        _logradouroRepo = new LogradouroRepository(ConnectionString, DatabaseType);
        _alunoRepo = new AlunoRepository(ConnectionString, DatabaseType);
        _matriculaRepo = new MatriculaRepository(ConnectionString, DatabaseType);
    }

    // Exigências da entrega: objetivo = nome do aluno, obs_restricao = contém a sigla do SGBD utilizado.
    private static string ObjetivoParaEntrega => $"{NomeParaEntrega} {SobrenomeParaEntrega}";

    private static string ObsRestricaoParaEntrega(string? detalhe = null) =>
        string.IsNullOrWhiteSpace(detalhe) ? NomeSgbdParaEntrega : $"{detalhe} - {NomeSgbdParaEntrega}";

    private static DateOnly CalcularDataFim(DateOnly dataInicio, MatriculaPlano plano) => plano switch
    {
        MatriculaPlano.Mensal => dataInicio.AddMonths(1),
        MatriculaPlano.Trimestral => dataInicio.AddMonths(3),
        MatriculaPlano.Semestral => dataInicio.AddMonths(6),
        MatriculaPlano.Anual => dataInicio.AddYears(1),
        _ => throw new ArgumentOutOfRangeException(nameof(plano), plano, "Plano não suportado.")
    };

    private async Task<Matricula> CriarEInserirMatriculaAsync(
        Aluno aluno,
        MatriculaPlano plano = MatriculaPlano.Mensal,
        DateOnly? dataInicio = null,
        MatriculaRestricoes restricoes = MatriculaRestricoes.None,
        string? obsRestricao = null,
        Arquivo? laudo = null)
    {
        var inicio = dataInicio ?? DateOnly.FromDateTime(DateTime.Today);

        // com restrições médicas registradas o laudo é obrigatório pela regra de domínio
        if (restricoes != MatriculaRestricoes.None && laudo == null)
        {
            laudo = Arquivo.Criar(new byte[] { 1, 2, 3, 4 }).Value;
        }

        var matriculaResult = Matricula.Criar(
            id: 0,
            aluno: aluno,
            plano: plano,
            dataInicio: inicio,
            dataFim: CalcularDataFim(inicio, plano),
            objetivo: ObjetivoParaEntrega,
            restricoes: restricoes,
            observacoesRestricoes: ObsRestricaoParaEntrega(obsRestricao),
            laudoMedico: laudo
        );

        if (matriculaResult.IsFailure)
        {
            throw new Exception($"Falha ao criar Matricula no Helper: {string.Join(", ", matriculaResult.Notifications.Select(n => n.Mensagem))}");
        }

        return await _matriculaRepo.Adicionar(matriculaResult.Value!);
    }

    [Fact]
    public async Task Matricula_Adicionar_E_ObterPorId_Sucesso()
    {
        var aluno = await AlunoInfrastructureTests.CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo);
        var restricoesComb = MatriculaRestricoes.Diabetes | MatriculaRestricoes.PressaoAlta;
        var laudo = Arquivo.Criar(new byte[] { 100, 101, 102 }).Value!;

        var inserida = await CriarEInserirMatriculaAsync(
            aluno: aluno,
            plano: MatriculaPlano.Mensal,
            restricoes: restricoesComb,
            obsRestricao: "Usar medicação ao acordar",
            laudo: laudo
        );

        Assert.NotNull(inserida);
        Assert.True(inserida.Id > 0);
        Assert.Equal(aluno.Id, inserida.AlunoId);
        Assert.Equal(MatriculaPlano.Mensal, inserida.Plano);
        Assert.Equal(restricoesComb, inserida.Restricoes);
        Assert.True(inserida.Restricoes.HasFlag(MatriculaRestricoes.Diabetes));
        Assert.True(inserida.Restricoes.HasFlag(MatriculaRestricoes.PressaoAlta));

        var obtida = await _matriculaRepo.ObterPorId(inserida.Id);
        Assert.NotNull(obtida);
        Assert.Equal(inserida.Id, obtida.Id);
        Assert.Equal(aluno.Id, obtida.AlunoId);
        Assert.Equal(MatriculaPlano.Mensal, obtida.Plano);
        Assert.Equal(restricoesComb, obtida.Restricoes);
        Assert.Equal(ObjetivoParaEntrega, obtida.Objetivo);
        Assert.Equal(ObsRestricaoParaEntrega("Usar medicação ao acordar"), obtida.ObservacoesRestricoes);
        Assert.NotNull(obtida.LaudoMedico);
        Assert.Equal(laudo.Conteudo, obtida.LaudoMedico.Conteudo);
    }

    [Fact]
    public async Task Matricula_RestricoesMedicas_ComVariacoesMultiplaEscolha_PersisteEObtemCorretamente()
    {
        var aluno = await AlunoInfrastructureTests.CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo);
        var laudo = Arquivo.Criar(new byte[] { 10, 20, 30, 40, 50 }).Value!;

        // Combinação de múltipla escolha: Pressão Alta + Labirintite + Problemas Respiratórios + Remédio Contínuo
        var restricoesMultiplas = MatriculaRestricoes.PressaoAlta
                                | MatriculaRestricoes.Labirintite
                                | MatriculaRestricoes.ProblemasRespiratorios
                                | MatriculaRestricoes.RemedioContinuo;

        var matricula = await CriarEInserirMatriculaAsync(
            aluno: aluno,
            plano: MatriculaPlano.Semestral,
            restricoes: restricoesMultiplas,
            obsRestricao: "Evitar exercícios de alto impacto e hipertensão",
            laudo: laudo
        );

        var obtida = await _matriculaRepo.ObterPorId(matricula.Id);

        Assert.NotNull(obtida);
        Assert.Equal(restricoesMultiplas, obtida.Restricoes);

        // verificação bitwise das flags selecionadas (múltipla escolha)
        Assert.True(obtida.Restricoes.HasFlag(MatriculaRestricoes.PressaoAlta));
        Assert.True(obtida.Restricoes.HasFlag(MatriculaRestricoes.Labirintite));
        Assert.True(obtida.Restricoes.HasFlag(MatriculaRestricoes.ProblemasRespiratorios));
        Assert.True(obtida.Restricoes.HasFlag(MatriculaRestricoes.RemedioContinuo));

        // verificação de que flags não marcadas retornam false
        Assert.False(obtida.Restricoes.HasFlag(MatriculaRestricoes.Diabetes));
        Assert.False(obtida.Restricoes.HasFlag(MatriculaRestricoes.Alergias));

        Assert.Equal(ObsRestricaoParaEntrega("Evitar exercícios de alto impacto e hipertensão"), obtida.ObservacoesRestricoes);
        Assert.NotNull(obtida.LaudoMedico);
        Assert.Equal(laudo.Conteudo, obtida.LaudoMedico.Conteudo);
    }

    [Fact]
    public async Task Matricula_ObterPorId_RetornaNuloQuandoInexistente()
    {
        var obtida = await _matriculaRepo.ObterPorId(999999);
        Assert.Null(obtida);
    }

    [Fact]
    public async Task Matricula_ObterTodos_Sucesso()
    {
        var aluno = await AlunoInfrastructureTests.CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo);
        await CriarEInserirMatriculaAsync(aluno);

        var todas = await _matriculaRepo.ObterTodos();

        Assert.NotNull(todas);
        Assert.NotEmpty(todas);
    }

    [Fact]
    public async Task Matricula_Atualizar_Sucesso()
    {
        var aluno = await AlunoInfrastructureTests.CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo);
        var inserida = await CriarEInserirMatriculaAsync(aluno, MatriculaPlano.Mensal, restricoes: MatriculaRestricoes.Alergias);

        var novasRestricoes = MatriculaRestricoes.Alergias | MatriculaRestricoes.Diabetes | MatriculaRestricoes.Labirintite;
        var laudoAtualizado = Arquivo.Criar(new byte[] { 99, 88, 77 }).Value!;

        var matriculaAtualizada = Matricula.Criar(
            id: inserida.Id,
            aluno: aluno,
            plano: MatriculaPlano.Anual,
            dataInicio: inserida.DataInicio,
            dataFim: CalcularDataFim(inserida.DataInicio, MatriculaPlano.Anual),
            objetivo: ObjetivoParaEntrega,
            restricoes: novasRestricoes,
            observacoesRestricoes: ObsRestricaoParaEntrega("Restrição médica atualizada com novas opções"),
            laudoMedico: laudoAtualizado
        ).Value!;

        var resultado = await _matriculaRepo.Atualizar(matriculaAtualizada);

        Assert.NotNull(resultado);
        Assert.Equal(MatriculaPlano.Anual, resultado.Plano);
        Assert.Equal(ObjetivoParaEntrega, resultado.Objetivo);
        Assert.Equal(novasRestricoes, resultado.Restricoes);

        var noBanco = await _matriculaRepo.ObterPorId(inserida.Id);
        Assert.NotNull(noBanco);
        Assert.Equal(MatriculaPlano.Anual, noBanco.Plano);
        Assert.Equal(ObjetivoParaEntrega, noBanco.Objetivo);
        Assert.Equal(novasRestricoes, noBanco.Restricoes);
        Assert.True(noBanco.Restricoes.HasFlag(MatriculaRestricoes.Alergias));
        Assert.True(noBanco.Restricoes.HasFlag(MatriculaRestricoes.Diabetes));
        Assert.True(noBanco.Restricoes.HasFlag(MatriculaRestricoes.Labirintite));
    }

    [Fact]
    public async Task Matricula_Atualizar_LancaExcecaoQuandoInexistente()
    {
        var aluno = await AlunoInfrastructureTests.CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo);
        var inicio = DateOnly.FromDateTime(DateTime.Today);

        var matriculaInexistente = Matricula.Criar(
            id: 999999,
            aluno: aluno,
            plano: MatriculaPlano.Mensal,
            dataInicio: inicio,
            dataFim: CalcularDataFim(inicio, MatriculaPlano.Mensal),
            objetivo: ObjetivoParaEntrega,
            restricoes: MatriculaRestricoes.None,
            observacoesRestricoes: ObsRestricaoParaEntrega(),
            laudoMedico: null
        ).Value!;

        var ex = await Assert.ThrowsAsync<InfrastructureException>(() => _matriculaRepo.Atualizar(matriculaInexistente));
        Assert.Equal("REGISTRO_NAO_ENCONTRADO", ex.ErrorCode);
    }

    [Fact]
    public async Task Matricula_Remover_Sucesso()
    {
        var aluno = await AlunoInfrastructureTests.CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo);
        var inserida = await CriarEInserirMatriculaAsync(aluno);

        var removida = await _matriculaRepo.Remover(inserida.Id);
        Assert.True(removida);

        var noBanco = await _matriculaRepo.ObterPorId(inserida.Id);
        Assert.Null(noBanco);
    }

    [Fact]
    public async Task Matricula_Remover_RetornaFalseQuandoInexistente()
    {
        var removida = await _matriculaRepo.Remover(999999);
        Assert.False(removida);
    }

    [Fact]
    public async Task Matricula_ObterPorAluno_FiltragemCorreta()
    {
        var aluno = await AlunoInfrastructureTests.CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo);
        await CriarEInserirMatriculaAsync(aluno);

        var matriculas = await _matriculaRepo.ObterPorAluno(aluno.Id);

        Assert.NotNull(matriculas);
        Assert.NotEmpty(matriculas);
        Assert.All(matriculas, m => Assert.Equal(aluno.Id, m.AlunoId));
    }

    [Fact]
    public async Task Matricula_ObterMatriculaAtivaPorAluno_E_PossuiMatriculaAtiva()
    {
        var aluno = await AlunoInfrastructureTests.CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo);

        var possuiAntes = await _matriculaRepo.PossuiMatriculaAtiva(aluno.Id);
        Assert.False(possuiAntes);

        await CriarEInserirMatriculaAsync(aluno, MatriculaPlano.Mensal, DateOnly.FromDateTime(DateTime.Today));

        var possuiDepois = await _matriculaRepo.PossuiMatriculaAtiva(aluno.Id);
        Assert.True(possuiDepois);

        var ativa = await _matriculaRepo.ObterMatriculaAtivaPorAluno(aluno.Id);
        Assert.NotNull(ativa);
        Assert.Equal(aluno.Id, ativa.AlunoId);
    }

    [Fact]
    public async Task Matricula_ObterAtivas_FiltragemCorreta()
    {
        var aluno = await AlunoInfrastructureTests.CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo);
        await CriarEInserirMatriculaAsync(aluno, MatriculaPlano.Semestral, DateOnly.FromDateTime(DateTime.Today));

        var ativasGeral = await _matriculaRepo.ObterAtivas();
        Assert.NotNull(ativasGeral);
        Assert.NotEmpty(ativasGeral);

        var ativasPorAluno = await _matriculaRepo.ObterAtivas(aluno.Id);
        Assert.NotNull(ativasPorAluno);
        Assert.NotEmpty(ativasPorAluno);
        Assert.All(ativasPorAluno, m => Assert.Equal(aluno.Id, m.AlunoId));
    }

    [Fact]
    public async Task Matricula_ObterVencendoEmDias_RetornaMatriculasProximasDoVencimento()
    {
        var aluno = await AlunoInfrastructureTests.CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo);
        var inicio = DateOnly.FromDateTime(DateTime.Today.AddDays(-25));
        await CriarEInserirMatriculaAsync(aluno, MatriculaPlano.Mensal, inicio);

        var vencendoEm30Dias = await _matriculaRepo.ObterVencendoEmDias(30);

        Assert.NotNull(vencendoEm30Dias);
        Assert.Contains(vencendoEm30Dias, m => m.AlunoId == aluno.Id);
    }

    [Fact]
    public async Task Matricula_ObterPorPlano_FiltragemCorreta()
    {
        var aluno = await AlunoInfrastructureTests.CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo);
        await CriarEInserirMatriculaAsync(aluno, MatriculaPlano.Trimestral);

        var trimestrais = await _matriculaRepo.ObterPorPlano(MatriculaPlano.Trimestral);

        Assert.NotNull(trimestrais);
        Assert.Contains(trimestrais, m => m.AlunoId == aluno.Id && m.Plano == MatriculaPlano.Trimestral);
    }
}
