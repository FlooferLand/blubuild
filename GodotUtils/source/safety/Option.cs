using System;
using System.Diagnostics;

namespace Project;

// ReSharper disable UnusedMember.Global
public readonly struct Option<T> {
    private readonly T value;

    public bool HasValue { get; }

    #region Constructors
    private Option(T value, bool hasValue) {
        this.HasValue = hasValue;
        this.value = value;
    }
    
    public Option() {
        HasValue = false;
        value = default;
    }
    
    /** Stores a value; stores nothing is the value is <c>null</c> */
    public static Option<T> Some(T value) {
        return value == null
            ? new Option<T>()
            : new Option<T>(value, true)!;
    }
    
    /** Stores nothing */
    public static Option<T> None() {
        return new Option<T>();
    }
    
    /** Stores a value if the condition is true, otherwise it stores nothing */
    public static Option<T> Conditional(bool condition, T onTrue) {
        return condition ? Some(onTrue) : None();
    }
    #endregion

    #region Getting the value
    public bool LetSome(out T val) {
        val = value!;
        return HasValue;
    }
    
    public delegate T MapSomeValue(T val);
    public Option<T> MapSome(MapSomeValue mapper) {
        var val = Some(value);
        return val.LetSome(out var v) ? Some(mapper(v)) : None();
    }
    
    public T Expect(string message) {
        return value ?? throw new Exception(message);
    }

    public T Unwrap() {
        Debug.Assert(value != null, nameof(value) + " != null");
        return value;
    }
    
    public T UnwrapOr(T @default) {
        return value ?? @default;
    }
    #endregion

    public override string ToString() {
        return HasValue ? $"Some({value})" : "None";
    }
}
