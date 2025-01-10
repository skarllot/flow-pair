using System.Diagnostics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Raiqub.LlmTools.FlowPair.Common;

public static class FunctionalExtensions
{
    /// <summary>
    /// Executes a side effect on a <see cref="Result{TOk,TErr}"/> regardless of its state and returns the original <see cref="Result{TOk,TErr}"/>.
    /// </summary>
    /// <typeparam name="TOk">The type of the value in case of success.</typeparam>
    /// <typeparam name="TErr">The type of the error in case of failure.</typeparam>
    /// <param name="result">The <see cref="Result{TOk,TErr}"/> to perform the side effect on.</param>
    /// <param name="callback">The <see cref="Action{T}"/> to execute as a side effect.</param>
    /// <returns>The original <see cref="Result{TOk,TErr}"/>, unmodified.</returns>
    /// <remarks>
    /// This method is useful for performing operations like logging or debugging without altering the <see cref="Result{TOk,TErr}"/>.
    /// The <paramref name="callback"/> is executed regardless of whether the <see cref="Result{TOk,TErr}"/> is in a success or failure state.
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = Ok&lt;int, string&gt;(42)
    ///     .DoBoth(r => Console.WriteLine($"Result state: {r.IsOk}"));
    /// // Output: "Result state: True"
    /// // result still contains Ok(42)
    /// </code>
    /// </example>
    public static Result<TOk, TErr> DoBoth<TOk, TErr>(
        this Result<TOk, TErr> result,
        Action<Result<TOk, TErr>> callback)
        where TOk : notnull
        where TErr : notnull
    {
        callback(result);
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
