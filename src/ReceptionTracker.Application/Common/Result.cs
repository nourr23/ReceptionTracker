using System.Diagnostics.CodeAnalysis;

namespace ReceptionTracker.Application.Common;

/// <summary>Either a value or an <see cref="Common.Error"/>, never both.</summary>
public sealed class Result<T>
{
    private Result(T value)
    {
        Value = value;
        IsSuccess = true;
    }

    private Result(Error error)
    {
        Error = error;
    }

    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess { get; }

    public T? Value { get; }

    public Error? Error { get; }

    public TOut Match<TOut>(Func<T, TOut> onSuccess, Func<Error, TOut> onFailure) =>
        IsSuccess ? onSuccess(Value) : onFailure(Error);

    // Lets a method declared as Result<T> simply "return value;" or "return error;".
    public static implicit operator Result<T>(T value) => new(value);

    public static implicit operator Result<T>(Error error) => new(error);
}
