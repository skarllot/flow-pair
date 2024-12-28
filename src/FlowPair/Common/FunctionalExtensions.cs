using System.Diagnostics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Ciandt.FlowTools.FlowPair.Common;

public static class FunctionalExtensions
{
    public static Result<TAccumulate, TError> TryAggregate<TSource, TAccumulate, TError>(
        this IEnumerable<TSource> source,
        TAccumulate seed,
        [InstantHandle] Func<TAccumulate, TSource, Result<TAccumulate, TError>> func)
        where TAccumulate : notnull
        where TError : notnull
    {
        var result = Ok<TAccumulate, TError>(seed);

        foreach (var item in source)
        {
            if (!result.TryGet(out var value, out var error))
            {
                return error;
            }

            result = func(value, item);
        }

        return result;
    }

    /// <summary>
    /// Returns a failure result if the predicate is false. Otherwise, returns a result with the specified value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value in the result.</typeparam>
    /// <typeparam name="TError">The type of the error in the result.</typeparam>
    /// <param name="value">The input value.</param>
    /// <param name="predicate">The predicate to be evaluated on the value.</param>
    /// <param name="error">A function that provides the default error value to return if the check fails.</param>
    /// <returns>A <see cref="Result{TOk,TErr}"/> containing either the original value or the specified error.</returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TValue, TError> Ensure<TValue, TError>(
        this TValue value,
        [InstantHandle] Func<TValue, bool> predicate,
        [InstantHandle] Func<TError> error)
        where TValue : notnull
        where TError : notnull =>
        predicate(value) ? value : error();

    /// <summary>
    /// Returns a failure result if the predicate is false. Otherwise, returns a result with the specified value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value in the result.</typeparam>
    /// <typeparam name="TError">The type of the error in the result.</typeparam>
    /// <param name="value">The input value.</param>
    /// <param name="predicate">The predicate to be evaluated on the value.</param>
    /// <param name="error">The error value to return if the check fails.</param>
    /// <returns>A <see cref="Result{TOk,TErr}"/> containing either the original value or the specified error.</returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TValue, TError> Ensure<TValue, TError>(
        this TValue value,
        [InstantHandle] Func<TValue, bool> predicate,
        TError error)
        where TValue : notnull
        where TError : notnull =>
        predicate(value) ? value : error;

    public static TErr UnwrapErrOr<TOk, TErr>(this Result<TOk, TErr> result, TErr fallback)
        where TOk : notnull
        where TErr : notnull => result.TryGet(out _, out var error) ? fallback : error;

    public static TOk? UnwrapOrNull<TOk, TErr>(this Result<TOk, TErr> result)
        where TOk : class
        where TErr : notnull => result.TryGet(out var ok, out _) ? ok : null;

    public static T? UnwrapOrNull<T>(this Option<T> option)
        where T : class => option.TryGet(out var value) ? value : null;
}
