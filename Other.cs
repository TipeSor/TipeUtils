using System.Diagnostics.CodeAnalysis;

namespace TipeUtils
{
    public readonly struct Unit
    {
        public static readonly Unit Value;
        public override string ToString() => "()";
    }

    public record Result<T>(T? Value, bool HasValue, string Message)
    {
        [MemberNotNullWhen(true, nameof(Value))]
        public bool HasValue { get; init; } = HasValue;

        [MemberNotNullWhen(false, nameof(Value))]
        public bool IsError => !HasValue;

        public bool Is(T value) => HasValue && EqualityComparer<T>.Default.Equals(Value, value);

        public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onError)
            => HasValue ? onSuccess(Value) : onError(Message);

        public Result<TValue> Map<TValue>(Func<T, TValue> map)
            => Match(onSuccess: value => Result<TValue>.Ok(map(value)), onError: Result<TValue>.Error);

        public Result<T> MapError(Func<string, string> map)
            => Match(onSuccess: Result<T>.Ok, onError: msg => Result<T>.Error(map(msg)));

        public static Result<T> Ok(T value) => new(value, true, "");
        public static Result<T> Error(string message) => new(default, false, message);
        public static Result<T> Error(Exception ex) => new(default, false, ex.Message);
    }

    public static class ResultExtension
    {
        public static Result<T> Cast<T>(this Result<object> result)
        {
            if (result.IsError)
                return Result<T>.Error(result.Message);

            if (result.Value is not T typed)
                return Result<T>.Error($"Failed to cast `{result.Value.GetType()}` to `{typeof(T)}`.");

            return Result<T>.Ok(typed);
        }
    }

    public record Result(bool HasValue, string Message)
        : Result<Unit>(HasValue ? Unit.Value : default, HasValue, Message)
    {
        public static Result Ok() => new(true, "");

        public static new Result Error(string message) => new(false, message);
        public static new Result Error(Exception ex) => new(false, ex.Message);
    }
}
