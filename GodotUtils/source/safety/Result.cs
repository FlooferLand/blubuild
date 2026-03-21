using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace GodotUtils;

/// <summary>
/// Safety type that either holds a value or an error <br/>
/// Similar (and compatible) with <see cref="Result{TOk,TErr}"/>, but this uses a <see cref="string"/> for the error by default
/// </summary>
/// <typeparam name="TOk">The value that might be held inside the type</typeparam>
[SuppressMessage("ReSharper", "UnusedType.Global")]
public class Result<TOk> : Result<TOk, String> {
    protected Result(TOk value, String error, Boolean isOk) : base(value, error, isOk) { }
    
    /** Stores a value */
    public new static Result<TOk> Ok(TOk value) {
        return new Result<TOk>(value, null!, true);
    }
    
    /** Stores an error */
    public new static Result<TOk> Err(String error) {
        return new Result<TOk>(default!, error, false);
    }
}

/// <summary>
/// Safety type that either holds a value or an error
/// </summary>
/// <typeparam name="TOk">The value that might be held inside the type</typeparam>
/// <typeparam name="TErr">The error that might be held inside the type</typeparam>
[SuppressMessage("ReSharper", "UnusedType.Global")]
public class Result<TOk, TErr> {
    readonly TOk value;
    readonly TErr error;
    readonly bool isOk;

    public bool IsOk => (value != null && isOk);

    #region Constructors
    protected Result(TOk value, TErr error, bool isOk) {
        this.value = value;
        this.error = error;
        this.isOk = isOk;
    }
    
    /** Stores a value */
    public static Result<TOk, TErr> Ok(TOk value) {
        return new Result<TOk, TErr>(value, default!, true);
    }
    
    /** Stores an error */
    public static Result<TOk, TErr> Err(TErr error) {
        return new Result<TOk, TErr>(default!, error, false);
    }
    
    /** Stores a value if the condition is true, otherwise it stores an error */
    public static Result<TOk, TErr> Conditional(bool condition, TOk onTrue, TErr onFalse) {
        return condition ? Ok(onTrue) : Err(onFalse);
    }
    #endregion

    #region Getting the value
    public bool LetOk(out TOk val) {
        val = value!;
        return IsOk;
    }
    
    public delegate TOk MapSomeValue(TOk val);
    public Result<TOk, TErr> MapOk(MapSomeValue mapper) {
        return LetOk(out var v) ? Ok(mapper(v)) : Err(error!);
    }
    
    public TOk Expect(string message) {
        return IsOk ? value! : throw new Exception(message);
    }

    public TOk Unwrap() {
        return value ?? throw new InvalidOperationException("Unwrap called on an error: " + error);
    }
    
    public TOk UnwrapOr(TOk @default) {
        return value ?? @default;
    }
    #endregion
    
    #region Getting the error
    public bool LetErr(out TErr err) {
        err = error!;
        return !IsOk;
    }
    #endregion
}
