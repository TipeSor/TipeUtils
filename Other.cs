using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace TipeUtils
{
    public readonly struct Unit
    {
        public static readonly Unit Value;
        public override string ToString() => "()";
    }

    public abstract record Result<TValue, TError>
    {
        private Result() { }

        [MemberNotNullWhen(true, nameof(Value))]
        [MemberNotNullWhen(false, nameof(Error))]
        public bool IsOk => this is OkResult;

        [MemberNotNullWhen(true, nameof(Error))]
        [MemberNotNullWhen(false, nameof(Value))]
        public bool IsError => this is ErrResult;

        public TValue? Value => this is OkResult res ? res.V : default;
        public TError? Error => this is ErrResult res ? res.E : default;

        public T Match<T>(Func<TValue, T> onSuccess, Func<TError, T> onError)
            => this switch
            {
                OkResult(TValue v) => onSuccess(v),
                ErrResult(TError e) => onError(e),
                _ => throw new UnreachableException()
            };

        public void Match(Action<TValue> onSuccess, Action<TError> onError)
        {
            switch (this)
            {
                case OkResult(TValue v): onSuccess(v); break;
                case ErrResult(TError e): onError(e); break;
                default:
                    throw new UnreachableException();
            }
        }

        public Result<T, TError> Bind<T>(Func<TValue, Result<T, TError>> func)
            => Match(onSuccess: v => func(v), onError: Result<T, TError>.Err);

        public Result<T, TError> Map<T>(Func<TValue, T> func)
            => Match(onSuccess: v => Result<T, TError>.Ok(func(v)), onError: Result<T, TError>.Err);

        public Result<TValue, T> MapError<T>(Func<TError, T> func)
            => Match(onError: e => Result<TValue, T>.Err(func(e)), onSuccess: Result<TValue, T>.Ok);

        public Result<T, TError> Cast<T>(TError error)
            => Bind(obj => obj is T value
                        ? Result<T, TError>.Ok(value)
                        : Result<T, TError>.Err(error));

        public static Result<TValue, TError> Ok(TValue value) => new OkResult(value);
        public static Result<TValue, TError> Err(TError value) => new ErrResult(value);

        private sealed record OkResult(TValue V) : Result<TValue, TError>
        { public override string ToString() => $"Result {{ Value = {V} }}"; }
        private sealed record ErrResult(TError E) : Result<TValue, TError>
        { public override string ToString() => $"Result {{ Error = {E} }}"; }
    }

    public static class Extenstions
    {
        extension(Result<object, string> result)
        {
            public Result<T, string> Cast<T>()
                => result.Cast<T>($"Failed to cast result to {typeof(T)}.");
        }

        extension<TError>(Result<Unit, TError> result)
        {
            public static Result<Unit, TError> Ok()
                => Result<Unit, TError>.Ok(Unit.Value);
        }

        extension<TError>(Result<string, TError> result)
        {
            public static Result<string, TError> Ok(object value)
                => Result<string, TError>.Ok(value?.ToString() ?? "null");
        }
    }
}
