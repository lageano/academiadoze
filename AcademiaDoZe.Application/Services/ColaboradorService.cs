// gabriel geremias vieira
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Application.Security;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Application.Services;

public class ColaboradorService : IColaboradorService
{
    private readonly Func<IColaboradorRepository> _repoFactory;
    private readonly Func<ILogradouroRepository> _logradouroRepoFactory;

    public ColaboradorService(Func<IColaboradorRepository> repoFactory, Func<ILogradouroRepository> logradouroRepoFactory)
    {
        _repoFactory = repoFactory ?? throw new ArgumentNullException(nameof(repoFactory));
        _logradouroRepoFactory = logradouroRepoFactory ?? throw new ArgumentNullException(nameof(logradouroRepoFactory));
    }

    public async Task<bool> CpfJaExisteAsync(string cpf, int? id = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cpf)) return false;

        var cpfResult = Cpf.Criar(cpf);
        if (cpfResult.IsFailure) return false;

        return await _repoFactory().CpfJaExiste(cpfResult.Value!, id, cancellationToken);
    }

    public async Task<bool> EmailJaExisteAsync(string email, int? id = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;

        var emailResult = Email.Criar(email);
        if (emailResult.IsFailure) return false;

        return await _repoFactory().EmailJaExiste(emailResult.Value!, id, cancellationToken);
    }

    public async Task<ColaboradorDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var colaborador = await _repoFactory().ObterPorId(id, cancellationToken);
        return colaborador?.ToDto();
    }

    public async Task<IEnumerable<ColaboradorDto>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        var colaboradores = await _repoFactory().ObterTodos(cancellationToken);
        return [.. colaboradores.Select(c => c.ToDto())];
    }

    public async Task<ColaboradorDto?> ObterPorCpfAsync(string cpf, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            throw new ArgumentException("CPF não pode ser vazio.", nameof(cpf));

        var cpfResult = Cpf.Criar(cpf);
        if (cpfResult.IsFailure)
            throw new ArgumentException($"CPF inválido: {string.Join(", ", cpfResult.Notifications.Select(n => n.Mensagem))}", nameof(cpf));

        var colaborador = await _repoFactory().ObterPorCpf(cpfResult.Value!, cancellationToken);
        return colaborador?.ToDto();
    }

    public async Task<ColaboradorDto?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email não pode ser vazio.", nameof(email));

        var emailResult = Email.Criar(email);
        if (emailResult.IsFailure)
            throw new ArgumentException($"Email inválido: {string.Join(", ", emailResult.Notifications.Select(n => n.Mensagem))}", nameof(email));

        var colaborador = await _repoFactory().ObterPorEmail(emailResult.Value!, cancellationToken);
        return colaborador?.ToDto();
    }

    public async Task<IEnumerable<ColaboradorDto>> ObterPorTipoAsync(AppColaboradorTipo tipo, CancellationToken cancellationToken = default)
    {
        var colaboradores = await _repoFactory().ObterPorTipo(tipo.ToDomain(), cancellationToken);
        return [.. colaboradores.Select(c => c.ToDto())];
    }

    public async Task<IEnumerable<ColaboradorDto>> ObterPorVinculoAsync(AppColaboradorVinculo vinculo, CancellationToken cancellationToken = default)
    {
        var colaboradores = await _repoFactory().ObterPorVinculo(vinculo.ToDomain(), cancellationToken);
        return [.. colaboradores.Select(c => c.ToDto())];
    }

    public async Task<ColaboradorDto> AdicionarAsync(ColaboradorDto colaboradorDto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(colaboradorDto);

        var cpfResult = Cpf.Criar(colaboradorDto.Cpf);
        if (cpfResult.IsFailure)
        {
            throw new ArgumentException($"CPF inválido: {string.Join(", ", cpfResult.Notifications.Select(n => n.Mensagem))}", nameof(colaboradorDto));
        }

        if (await _repoFactory().CpfJaExiste(cpfResult.Value!, null, cancellationToken))
        {
            throw new InvalidOperationException($"Já existe um colaborador cadastrado com o CPF {colaboradorDto.Cpf}.");
        }

        if (!string.IsNullOrWhiteSpace(colaboradorDto.Email))
        {
            var emailResult = Email.Criar(colaboradorDto.Email);
            if (emailResult.IsFailure)
            {
                throw new ArgumentException($"Email inválido: {string.Join(", ", emailResult.Notifications.Select(n => n.Mensagem))}", nameof(colaboradorDto));
            }

            if (await _repoFactory().EmailJaExiste(emailResult.Value!, null, cancellationToken))
            {
                throw new InvalidOperationException($"Já existe um colaborador cadastrado com o Email {colaboradorDto.Email}.");
            }
        }

        if (!string.IsNullOrWhiteSpace(colaboradorDto.Senha))
        {
            var senhaValidacao = Senha.Criar(colaboradorDto.Senha);
            if (senhaValidacao.IsFailure)
            {
                throw new ArgumentException($"Senha não atende aos requisitos mínimos: {string.Join(", ", senhaValidacao.Notifications.Select(n => n.Mensagem))}", nameof(colaboradorDto));
            }

            colaboradorDto.Senha = PasswordHasher.Hash(colaboradorDto.Senha);
        }

        var logradouro = await ObterLogradouroAsync(colaboradorDto.Endereco?.Id ?? 0, cancellationToken);

        var colaborador = colaboradorDto.ToEntity(logradouro);
        var adicionado = await _repoFactory().Adicionar(colaborador, cancellationToken);
        return adicionado.ToDto();
    }

    public async Task<ColaboradorDto> AtualizarAsync(ColaboradorDto colaboradorDto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(colaboradorDto);

        var colaboradorExistente = await _repoFactory().ObterPorId(colaboradorDto.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Colaborador com ID {colaboradorDto.Id} não encontrado.");

        var cpfResult = Cpf.Criar(colaboradorDto.Cpf);
        if (cpfResult.IsFailure)
        {
            throw new ArgumentException($"CPF inválido: {string.Join(", ", cpfResult.Notifications.Select(n => n.Mensagem))}", nameof(colaboradorDto));
        }

        if (await _repoFactory().CpfJaExiste(cpfResult.Value!, colaboradorDto.Id, cancellationToken))
        {
            throw new InvalidOperationException($"Já existe outro colaborador cadastrado com o CPF {colaboradorDto.Cpf}.");
        }

        if (!string.IsNullOrWhiteSpace(colaboradorDto.Email)
            && !string.Equals(colaboradorDto.Email, colaboradorExistente.Email?.Valor, StringComparison.OrdinalIgnoreCase))
        {
            var emailResult = Email.Criar(colaboradorDto.Email);
            if (emailResult.IsFailure)
            {
                throw new ArgumentException($"Email inválido: {string.Join(", ", emailResult.Notifications.Select(n => n.Mensagem))}", nameof(colaboradorDto));
            }

            if (await _repoFactory().EmailJaExiste(emailResult.Value!, colaboradorDto.Id, cancellationToken))
            {
                throw new InvalidOperationException($"Já existe outro colaborador cadastrado com o Email {colaboradorDto.Email}.");
            }
        }

        if (!string.IsNullOrWhiteSpace(colaboradorDto.Senha))
        {
            var senhaValidacao = Senha.Criar(colaboradorDto.Senha);
            if (senhaValidacao.IsFailure)
            {
                throw new ArgumentException($"Senha não atende aos requisitos mínimos: {string.Join(", ", senhaValidacao.Notifications.Select(n => n.Mensagem))}", nameof(colaboradorDto));
            }

            colaboradorDto.Senha = PasswordHasher.Hash(colaboradorDto.Senha);
        }

        int logradouroId = (colaboradorDto.Endereco != null && colaboradorDto.Endereco.Id > 0)
            ? colaboradorDto.Endereco.Id
            : colaboradorExistente.Endereco.Logradouro.Id;

        var logradouro = await ObterLogradouroAsync(logradouroId, cancellationToken);

        var colaboradorAtualizado = colaboradorExistente.UpdateFromDto(colaboradorDto, logradouro);
        var atualizado = await _repoFactory().Atualizar(colaboradorAtualizado, cancellationToken);
        return atualizado.ToDto();
    }

    public async Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        var colaborador = await _repoFactory().ObterPorId(id, cancellationToken);
        if (colaborador == null)
        {
            return false;
        }

        return await _repoFactory().Remover(id, cancellationToken);
    }

    public async Task<bool> TrocarSenhaAsync(int id, string novaSenha, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(novaSenha))
            throw new ArgumentException("Nova senha não pode ser vazia.", nameof(novaSenha));

        var validacaoSenha = Senha.Criar(novaSenha);
        if (validacaoSenha.IsFailure)
        {
            throw new ArgumentException($"Nova senha inválida: {string.Join(", ", validacaoSenha.Notifications.Select(n => n.Mensagem))}", nameof(novaSenha));
        }

        var hash = PasswordHasher.Hash(novaSenha);
        var senhaHashVO = Senha.Criar(hash);
        if (senhaHashVO.IsFailure)
        {
            throw new InvalidOperationException("Falha ao gerar hash da nova senha.");
        }

        return await _repoFactory().TrocarSenha(id, senhaHashVO.Value!, cancellationToken);
    }

    // O endereço do colaborador exige a entidade Logradouro completa, então ela é resolvida pelo repositório.
    private async Task<Domain.Entities.Logradouro> ObterLogradouroAsync(int logradouroId, CancellationToken cancellationToken)
    {
        if (logradouroId <= 0)
            throw new ArgumentException("Logradouro/Endereço é obrigatório para o colaborador.", nameof(logradouroId));

        return await _logradouroRepoFactory().ObterPorId(logradouroId, cancellationToken)
            ?? throw new KeyNotFoundException($"Logradouro com ID {logradouroId} não encontrado.");
    }
}
