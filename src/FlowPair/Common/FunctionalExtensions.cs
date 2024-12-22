using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Ciandt.FlowTools.FlowPair.Common;

public static class FunctionalExtensions
{
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
        Func<TValue, bool> predicate,
        Func<TError> error)
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
        Func<TValue, bool> predicate,
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
