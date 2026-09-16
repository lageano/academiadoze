// gabriel geremias vieira
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Application.Mappings;

public static class AlunoMappingExtensions
{
    public static AlunoDto ToDto(this Aluno aluno)
    {
        ArgumentNullException.ThrowIfNull(aluno);

        return new AlunoDto
        {
            Id = aluno.Id,
            Nome = aluno.Nome,
            Cpf = aluno.Cpf.Valor,
            DataNascimento = aluno.DataNascimento,
            Telefone = aluno.Telefone.Valor,
            Email = aluno.Email?.Valor,
            Endereco = aluno.Endereco.Logradouro.ToDto(),
            Numero = aluno.Endereco.Numero,
            Complemento = aluno.Endereco.Complemento,
            Senha = null, // a senha nunca deve ser exposta no DTO
            Foto = aluno.Foto?.Conteudo != null ? new ArquivoDto { Conteudo = aluno.Foto.Conteudo } : null
        };
    }

    public static Aluno ToEntity(this AlunoDto alunoDto, Logradouro logradouro)
    {
        ArgumentNullException.ThrowIfNull(alunoDto);
        ArgumentNullException.ThrowIfNull(logradouro);

        Arquivo? foto = null;
        if (alunoDto.Foto?.Conteudo != null)
        {
            var fotoResult = Arquivo.Criar(alunoDto.Foto.Conteudo);
            if (fotoResult.IsSuccess) foto = fotoResult.Value;
        }

        var result = Aluno.Criar(
            alunoDto.Id,
            alunoDto.Nome,
            alunoDto.Cpf,
            alunoDto.DataNascimento,
            alunoDto.Telefone,
            alunoDto.Email,
            logradouro,
            alunoDto.Numero,
            alunoDto.Complemento,
            alunoDto.Senha ?? string.Empty,
            foto
        );

        if (result.IsFailure)
        {
            throw new InvalidOperationException($"Erro de validação ao converter Aluno: {string.Join(", ", result.Notifications.Select(n => n.Mensagem))}");
        }

        return result.Value!;
    }

    public static Aluno UpdateFromDto(this Aluno aluno, AlunoDto alunoDto, Logradouro logradouro)
    {
        ArgumentNullException.ThrowIfNull(aluno);
        ArgumentNullException.ThrowIfNull(alunoDto);
        ArgumentNullException.ThrowIfNull(logradouro);

        Arquivo? foto = aluno.Foto;
        if (alunoDto.Foto?.Conteudo != null)
        {
            var fotoResult = Arquivo.Criar(alunoDto.Foto.Conteudo);
            if (fotoResult.IsSuccess) foto = fotoResult.Value;
        }

        // quando a senha não é informada no DTO, mantém o hash já armazenado
        string senha = string.IsNullOrWhiteSpace(alunoDto.Senha) ? aluno.Senha.Valor : alunoDto.Senha;

        var result = Aluno.Criar(
            aluno.Id,
            alunoDto.Nome ?? aluno.Nome,
            alunoDto.Cpf ?? aluno.Cpf.Valor,
            alunoDto.DataNascimento != default ? alunoDto.DataNascimento : aluno.DataNascimento,
            alunoDto.Telefone ?? aluno.Telefone.Valor,
            alunoDto.Email ?? aluno.Email?.Valor,
            logradouro,
            alunoDto.Numero ?? aluno.Endereco.Numero,
            alunoDto.Complemento ?? aluno.Endereco.Complemento,
            senha,
            foto
        );

        if (result.IsFailure)
        {
            throw new InvalidOperationException($"Erro de validação ao atualizar Aluno: {string.Join(", ", result.Notifications.Select(n => n.Mensagem))}");
        }

        return result.Value!;
    }
}
