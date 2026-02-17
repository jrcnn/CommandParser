namespace CommandParser.Validation;

/// <summary>
///     Provides an interface to register validation errors for a symbol.
/// </summary>
public class ValidationContext
{
    private readonly List<string> errors = [];

    /// <summary>
    ///     Gets the name of the symbol being validated.
    ///     You do not need to include this name in the error messages you register with this context,
    ///     as it will be automatically included in the final error output.
    /// </summary>
    public string SymbolName { get; }

    internal ValidationContext(string symbolName)
    {
        SymbolName = symbolName;
    }

    /// <summary>
    ///     Gets a read-only list of validation errors that have been registered for the symbol associated with this context.
    /// </summary>
    public IReadOnlyList<string> Errors => errors.AsReadOnly();

    /// <summary>
    ///     Registers a validation error for the symbol associated with this context.
    /// </summary>
    /// <param name="message">The error message.</param>
    public void AddError(string message)
        => errors.Add(message);

    internal bool HasErrors()
        => errors.Count > 0;
}
