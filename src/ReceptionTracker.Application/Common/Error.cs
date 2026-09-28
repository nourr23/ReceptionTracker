namespace ReceptionTracker.Application.Common;

public enum ErrorType
{
    NotFound,
    Validation,
    Conflict
}

/// <summary>An expected failure of a use case (unlike exceptions, which are for bugs).</summary>
public sealed record Error(string Code, string Message, ErrorType Type)
{
    public static Error NotFound(string code, string message) => new(code, message, ErrorType.NotFound);
}
