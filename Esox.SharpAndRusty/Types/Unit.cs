namespace Esox.SharpAndRusty.Types
{
    /// <summary>
    /// A type that represents the absence of a value, inspired by Rust's unit type ().
    /// This type is useful when you need a Result type but have no meaningful value to return on success,
    /// such as operations that succeed or fail but don't produce a result.
    /// </summary>
    /// <remarks>
    /// <para>
    /// In Rust, the unit type () is used to represent the absence of a value.
    /// It's commonly used as the success type in Result&lt;(), E&gt; for operations that either
    /// succeed (with no value) or fail with an error.
    /// </para>
    /// <para>
    /// This C# implementation provides a singleton instance and implements structural equality,
    /// making it safe to use in comparisons and as a generic type parameter.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Operation that succeeds with no value or fails with an error
    /// public Result&lt;Unit, string&gt; ValidateInput(string input)
    /// {
    ///     if (string.IsNullOrEmpty(input))
    ///         return Result&lt;Unit, string&gt;.Err("Input cannot be empty");
    ///     
    ///     return Result&lt;Unit, string&gt;.Ok(Unit.Value);
    /// }
    /// 
    /// // Using the result
    /// var result = ValidateInput(userInput);
    /// result.Match(
    ///     success: _ => Console.WriteLine("Validation succeeded"),
    ///     failure: error => Console.WriteLine($"Validation failed: {error}")
    /// );
    /// 
    /// // With LINQ syntax
    /// var result = from _ in ValidateInput(userInput)
    ///              from __ in ProcessInput()
    ///              select Unit.Value;
    /// </code>
    /// </example>
    public readonly struct Unit : IEquatable<Unit>, IComparable<Unit>
    {
        /// <summary>
        /// Gets the singleton instance of the Unit type.
        /// This is the only instance you need to use throughout your application.
        /// </summary>
        public static readonly Unit Value = default;

        /// <summary>
        /// Determines whether the specified Unit value is equal to the current Unit value.
        /// All Unit values are always equal since the type represents the absence of a value.
        /// </summary>
        public bool Equals(Unit other) => true;

        /// <summary>
        /// Determines whether the specified object is a Unit value equal to the current Unit value.
        /// </summary>
        public override bool Equals(object? obj) => obj is Unit;

        /// <summary>
        /// Returns a hash code for this Unit value.
        /// All Unit values return the same hash code since they are all equal.
        /// </summary>
        public override int GetHashCode() => 0;

        /// <summary>
        /// Returns a string representation of the Unit value.
        /// </summary>
        public override string ToString() => "()";

        /// <summary>
        /// Compares the current Unit value with another Unit value.
        /// All Unit values are equal, so this always returns 0.
        /// </summary>
        public int CompareTo(Unit other) => 0;

        /// <summary>
        /// Determines whether two Unit values are equal.
        /// All Unit values are always equal.
        /// </summary>
        public static bool operator ==(Unit left, Unit right) => true;

        /// <summary>
        /// Determines whether two Unit values are not equal.
        /// All Unit values are always equal, so this always returns false.
        /// </summary>
        public static bool operator !=(Unit left, Unit right) => false;

        /// <summary>
        /// Determines whether one Unit value is less than another.
        /// All Unit values are equal, so this always returns false.
        /// </summary>
        public static bool operator <(Unit left, Unit right) => false;

        /// <summary>
        /// Determines whether one Unit value is greater than another.
        /// All Unit values are equal, so this always returns false.
        /// </summary>
        public static bool operator >(Unit left, Unit right) => false;

        /// <summary>
        /// Determines whether one Unit value is less than or equal to another.
        /// All Unit values are equal, so this always returns true.
        /// </summary>
        public static bool operator <=(Unit left, Unit right) => true;

        /// <summary>
        /// Determines whether one Unit value is greater than or equal to another.
        /// All Unit values are equal, so this always returns true.
        /// </summary>
        public static bool operator >=(Unit left, Unit right) => true;
    }
}
