using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Capyndex.Shared.Guards;

public interface IGuard;

public class Guard : IGuard
{
    public static IGuard Against { get; } = new Guard();

    private Guard() { }
}

public static class ApplicationGuardExtensions
{
    public static T NotFound<TKey, T>(this IGuard guard,
                                      [NotNull] TKey key,
                                      [NotNull][ValidatedNotNull] T? input,
                                      [CallerArgumentExpression("input")] string? parameterName = null)
        where TKey : struct
    {
        if (input is null)
        {
            throw new NotFoundException(key.ToString()!, parameterName!);
        }

        return input;
    }

    public static T NotFound<T>(this IGuard guard,
                                [ValidatedNotNull] string key,
                                [NotNull][ValidatedNotNull] T? input,
                                [CallerArgumentExpression("input")] string? parameterName = null)
    {
        if (input is null)
        {
            throw new NotFoundException(key, parameterName!);
        }

        return input;
    }

    public static Result FailedResult(this IGuard guard, Result result, string error)
    {
        if (!result.Succeeded)
        {
            throw new ValidationException(error);
        }

        return result;
    }
}

[AttributeUsage(AttributeTargets.Parameter)]
public class ValidatedNotNullAttribute : Attribute { }

public class NotFoundException : Exception
{
    public string Key { get; }
    public string ParameterName { get; }

    public NotFoundException(string key, string parameterName)
        : base($"Resource with key '{key}' was not found for parameter '{parameterName}'")
    {
        Key = key;
        ParameterName = parameterName;
    }
}

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
}

public class Result
{
    public bool Succeeded { get; private set; }
    public static Result Success => new() { Succeeded = true };
    public static Result Failure => new() { Succeeded = false };
}