// gabriel geremias vieira
namespace AcademiaDoZe.Domain.Exceptions;

// classe base para exceções de domínio, usadas apenas quando a operação não pode prosseguir (Fail-Fast)
public sealed class DomainException(string message) : Exception(message)
{
}
