namespace CommandParser.Validation;

internal delegate void Validator(in object? value, ValidationContext context);

/// <summary>
///     A delegate that defines the signature for a validation method that can be associated with a command, argument, or option.
/// </summary>
/// <typeparam name="T">
///     The type of the value being validated. For a command-level validator, this would typically be
///     the command's model type. For an argument or option-level validator, this would be the type of
///     the argument or option's value.
/// </typeparam>
/// <param name="value">
///     The value to validate.
/// </param>
/// <param name="context">
///     The validation context used for registering errors.
/// </param>
public delegate void Validator<T>(in T? value, ValidationContext context);
