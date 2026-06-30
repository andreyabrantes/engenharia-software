namespace BoraAli.Core.Exceptions;

/// <summary>
/// Exceção para recurso não encontrado (404)
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

/// <summary>
/// Exceção para requisição inválida (400)
/// </summary>
public class BadRequestException : Exception
{
    public BadRequestException(string message) : base(message) { }
}
