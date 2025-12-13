namespace Esox.SharpAndRusty.Types
{
    /// <summary>
    /// Represents a type with only one value, similar to Rust's () type or F#'s unit type.
    /// Used to represent the absence of a meaningful return value in Result types.
    /// </summary>
    /// <remarks>
    /// This type is particularly useful for operations that can fail but don't return 
    /// a meaningful value on success. For example, a write operation might return 
    /// Result&lt;Unit, Error&gt; to indicate success or failure without additional data.
    /// </remarks>
    /// <example>
    /// <code>
    /// public Result&lt;Unit, Error&gt; SaveToFile(string path, string content)
    /// {
    ///     try
    ///     {
    ///         File.WriteAllText(path, content);
    ///         return Result&lt;Unit, Error&gt;.Ok(Unit.Value);
    ///     }
    ///     catch (Exception ex)
    ///     {
    ///         return Result&lt;Unit, Error&gt;.Err(Error.FromException(ex));
    ///     }
    /// }
    /// 
    /// // Usage
    /// var result = SaveToFile("config.json", "{}");
    /// result.Match(
    ///     success: _ =&gt; Console.WriteLine("Saved successfully"),
    ///     failure: error =&gt; Console.WriteLine($"Failed: {error.Message}")
    /// );
    /// </code>
    /// </example>
    public readonly struct Unit : IEquatable<Unit>
    {
        /// <summary>
        /// The single instance of the Unit type.
        /// </summary>
        public static readonly Unit Value = default;

        /// <summary>
        /// Determines whether the specified Unit is equal to the current Unit.
        /// Since all Unit values are equal, this always returns true.
        /// </summary>
        public bool Equals(Unit other)
        {
            return true;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current Unit.
        /// </summary>
        public override bool Equals(object? obj)
        {
            return obj is Unit;
        }

        /// <summary>
        /// Returns the hash code for this Unit.
        /// Since all Unit values are equal, they all return the same hash code.
        /// </summary>
        public override int GetHashCode()
        {
            return 0;
        }

        /// <summary>
        /// Returns a string representation of this Unit.
        /// </summary>
        public override string ToString()
        {
            return "()";
        }

        /// <summary>
        /// Determines whether two Unit values are equal.
        /// Since all Unit values are equal, this always returns true.
        /// </summary>
        public static bool operator ==(Unit left, Unit right)
        {
            return true;
        }

        /// <summary>
        /// Determines whether two Unit values are not equal.
        /// Since all Unit values are equal, this always returns false.
        /// </summary>
        public static bool operator !=(Unit left, Unit right)
        {
            return false;
        }
    }
}
