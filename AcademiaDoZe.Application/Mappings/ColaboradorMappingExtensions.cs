// gabriel geremias vieira
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Application.Mappings;

public static class ColaboradorMappingExtensions
{
    public static ColaboradorDto ToDto(this Colaborador colaborador)
    {
        ArgumentNullException.ThrowIfNull(colaborador);

        return new ColaboradorDto
        {
            Id = colaborador.Id,
            Nome = colaborador.Nome,
            Cpf = colaborador.Cpf.Valor,
            DataNascimento = colaborador.DataNascimento,
            Telefone = colaborador.Telefone.Valor,
            Email = colaborador.Email?.Valor,
            Endereco = colaborador.Endereco.Logradouro.ToDto(),
            Numero = colaborador.Endereco.Numero,
            Complemento = colaborador.Endereco.Complemento,
            Senha = null, // a senha nunca deve ser exposta no DTO
            Foto = colaborador.Foto?.Conteudo != null ? new ArquivoDto { Conteudo = colaborador.Foto.Conteudo } : null,
            DataAdmissao = colaborador.DataAdmissao,
            Tipo = colaborador.Tipo.ToApplication(),
            Vinculo = colaborador.Vinculo.ToApplication()
        };
    }

    public static Colaborador ToEntity(this ColaboradorDto colaboradorDto, Logradouro logradouro)
    {
        ArgumentNullException.ThrowIfNull(colaboradorDto);
        ArgumentNullException.ThrowIfNull(logradouro);

        Arquivo? foto = null;
        if (colaboradorDto.Foto?.Conteudo != null)
        {
            var fotoResult = Arquivo.Criar(colaboradorDto.Foto.Conteudo);
            if (fotoResult.IsSuccess) foto = fotoResult.Value;
        }

        var result = Colaborador.Criar(
            colaboradorDto.Id,
            colaboradorDto.Nome,
            colaboradorDto.Cpf,
            colaboradorDto.DataNascimento,
            colaboradorDto.Telefone,
            colaboradorDto.Email ?? string.Empty,
            logradouro,
            colaboradorDto.Numero,
            colaboradorDto.Complemento,
            colaboradorDto.Senha ?? string.Empty,
            foto,
            colaboradorDto.DataAdmissao,
            colaboradorDto.Tipo.ToDomain(),
            colaboradorDto.Vinculo.ToDomain()
        );

        if (result.IsFailure)
        {
            throw new InvalidOperationException($"Erro de validação ao converter Colaborador: {string.Join(", ", result.Notifications.Select(n => n.Mensagem))}");
        }

        return result.Value!;
    }

    public static Colaborador UpdateFromDto(this Colaborador colaborador, ColaboradorDto colaboradorDto, Logradouro logradouro)
    {
        ArgumentNullException.ThrowIfNull(colaborador);
        ArgumentNullException.ThrowIfNull(colaboradorDto);
        ArgumentNullException.ThrowIfNull(logradouro);

        Arquivo? foto = colaborador.Foto;
        if (colaboradorDto.Foto?.Conteudo != null)
        {
            var fotoResult = Arquivo.Criar(colaboradorDto.Foto.Conteudo);
            if (fotoResult.IsSuccess) foto = fotoResult.Value;
        }

        // quando a senha não é informada no DTO, mantém o hash já armazenado
        string senha = string.IsNullOrWhiteSpace(colaboradorDto.Senha) ? colaborador.Senha.Valor : colaboradorDto.Senha;

        var result = Colaborador.Criar(
            colaborador.Id,
            colaboradorDto.Nome ?? colaborador.Nome,
            colaboradorDto.Cpf ?? colaborador.Cpf.Valor,
            colaboradorDto.DataNascimento != default ? colaboradorDto.DataNascimento : colaborador.DataNascimento,
            colaboradorDto.Telefone ?? colaborador.Telefone.Valor,
            colaboradorDto.Email ?? colaborador.Email!.Valor,
            logradouro,
            colaboradorDto.Numero ?? colaborador.Endereco.Numero,
            colaboradorDto.Complemento ?? colaborador.Endereco.Complemento,
            senha,
            foto,
            colaboradorDto.DataAdmissao != default ? colaboradorDto.DataAdmissao : colaborador.DataAdmissao,
            colaboradorDto.Tipo.ToDomain(),
            colaboradorDto.Vinculo.ToDomain()
        );

        if (result.IsFailure)
        {
            throw new InvalidOperationException($"Erro de validação ao atualizar Colaborador: {string.Join(", ", result.Notifications.Select(n => n.Mensagem))}");
        }

        return result.Value!;
    }
}
