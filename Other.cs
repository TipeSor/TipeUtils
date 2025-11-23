using System.Diagnostics.CodeAnalysis;

namespace TipeUtils
{
    public readonly struct Unit
    {
        public static readonly Unit Value;
        public override string ToString() => "()";
    }

    public record Result<T>(T? Value, bool HasValue, string Message)
        where T : notnull
    {
        [MemberNotNullWhen(true, nameof(Value))]
        public bool HasValue { get; init; } = HasValue;

        [MemberNotNullWhen(false, nameof(Value))]
        public bool IsError => !HasValue;

        public bool Is(T value) => HasValue && EqualityComparer<T>.Default.Equals(Value, value);

        public static Result<T> Ok(T value) => new(value, true, "");
        public static Result<T> Error(string message) => new(default, false, message);
        public static Result<T> Error(Exception ex) => new(default, false, ex.Message);
        public static Result<T> Error() => new(default, false, "");

        public static implicit operator Result<T>(Result<object> res)
        {
            if (res.IsError)
                return Result<T>.Error(res.Message);

            if (res.Value is not T typed)
                return Result<T>.Error($"Value ({res.Value}) is not of type {typeof(T)}");

            return Result<T>.Ok(typed);
        }
    }

    public record Result : Result<Unit>
    {
        private Result(bool ok, string message)
            : base(ok ? Unit.Value : default, ok, message) { }

        public static Result Ok()
            => new(true, "");

        public static new Result Error(string message = "") => new(false, message);
    }
}
