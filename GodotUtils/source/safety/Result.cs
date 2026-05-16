using System.Diagnostics.CodeAnalysis;

namespace GodotUtils;

/// Should not be used outside Result.cs
internal interface IResult<TOk, TErr> {
    [MemberNotNullWhen(true, nameof(Value))]
    public bool IsOk { get; }

    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsErr { get; }

    public TOk? Value { get; }
    public TErr? Error { get; }

    public bool LetOk(out TOk val);
    public Result<TOk, TErr> MapOk(Func<TOk, TOk> mapper);
    public TOk Expect(string message);
    public TOk Unwrap();
    public TOk UnwrapOr(TOk @default);
    public bool LetErr(out TErr err);
}

public readonly record struct OkResult<TOk>(TOk Value);
public readonly record struct ErrResult<TErr>(TErr Value);

public static class Result {
    public static OkResult<TOk> Ok<TOk>(TOk value) => new(value);
    public static ErrResult<TErr> Err<TErr>(TErr value) => new(value);
}

/// <summary> Like <see cref="GodotUtils.Result{TOk,???}"/> but with strings for errors </summary>
[SuppressMessage("ReSharper", "UnusedType.Global")]
public readonly record struct Result<TOk> : IResult<TOk, string> {
    readonly Result<TOk, string> inner;

    public bool IsOk => inner.IsOk;
    public bool IsErr => inner.IsErr;

    public TOk? Value => inner.Value;
    public string? Error => inner.Error;

    Result(Result<TOk, string> inner) => this.inner = inner;

    public static implicit operator Result<TOk>(OkResult<TOk> ok) => Ok(ok.Value);
    public static implicit operator Result<TOk>(ErrResult<string> err) => Err(err.Value);

    public static Result<TOk> Ok(TOk value) => new(Result<TOk, string>.Ok(value));
    public static Result<TOk> Err(string error) => new(Result<TOk, string>.Err(error));
    public static Result<TOk> Conditional(bool condition, TOk onTrue, string onFalse) => condition ? Ok(onTrue) : Err(onFalse);
    public Result<TOk, string> MapOk(Func<TOk, TOk> mapper) => inner.MapOk(mapper);
    public TOk Expect(string message) => inner.Expect(message);
    public TOk Unwrap() => inner.Unwrap();
    public TOk UnwrapOr(TOk @default) => inner.UnwrapOr(@default);

    public bool Try([NotNullWhen(true)] out TOk? val, [NotNullWhen(false)] out string? err) => inner.Handle(out val, out err);
    public bool LetOk([NotNullWhen(true)] out TOk val) => inner.LetOk(out val);
    public bool LetErr(out string err) => inner.LetErr(out err);

    [Obsolete("Use LetOk and LetErr instead")]
    public void Deconstruct(out TOk? val, out string? err) {
        val = Value;
        err = Error;
    }
}

/// <summary> Safety type that either holds a value or an error </summary>
/// <typeparam name="TOk">The value that might be held inside the type</typeparam>
/// <typeparam name="TErr">The error that might be held inside the type</typeparam>
[SuppressMessage("ReSharper", "UnusedType.Global")]
public readonly record struct Result<TOk, TErr> : IResult<TOk, TErr> {
    public bool IsOk { get; }
    public bool IsErr => !IsOk;

    public TOk? Value => IsOk ? field : default;
    public TErr? Error => IsErr ? field : default;

    Result(TOk value, TErr error, bool isOk) {
        Value = value;
        Error = error;
        IsOk = isOk;
    }

    public static implicit operator Result<TOk, TErr>(TOk value) => Ok(value);
    public static implicit operator Result<TOk, TErr>(TErr error) => Err(error);
    public static implicit operator Result<TOk, TErr>(OkResult<TOk> ok) => Ok(ok.Value);
    public static implicit operator Result<TOk, TErr>(ErrResult<TErr> err) => Err(err.Value);

    #region Constructors
    /** Stores a value */
    public static Result<TOk, TErr> Ok(TOk value) => new(value, default!, true);

    /** Stores an error */
    public static Result<TOk, TErr> Err(TErr error) => new(default!, error, false);

    /** Stores a value if the condition is true, otherwise it stores an error */
    public static Result<TOk, TErr> Conditional(bool condition, TOk onTrue, TErr onFalse) =>
        condition ? Ok(onTrue) : Err(onFalse);
    #endregion

    #region Getting the value
    public bool LetOk([NotNullWhen(true)] out TOk val) {
        val = Value!;
        return IsOk;
    }

    public Result<TOk, TErr> MapOk(Func<TOk, TOk> mapper) =>
        LetOk(out var v) ? Ok(mapper(v)) : Err(Error!);

    public TOk Expect(string message) =>
        IsOk ? Value! : throw new Exception(message);

    public TOk Unwrap() =>
        Value ?? throw new InvalidOperationException("Unwrap called on an error: " + Error);

    public TOk UnwrapOr(TOk @default) => Value ?? @default;
    #endregion

    #region Getting the error
    public bool LetErr([NotNullWhen(true)] out TErr err) {
        err = Error!;
        return !IsOk;
    }
    #endregion

    public bool Handle([NotNullWhen(true)] out TOk? val, [NotNullWhen(false)] out TErr? err) {
        if (IsOk) {
            val = Value!;
            err = default;
            return true;
        }
        val = default;
        err = Error!;
        return false;
    }
}
