namespace CommandParser.ValueParsing;

internal delegate object? ValueParser(string input, IFormatProvider? formatProvider);

/// <summary>
///     A delegate that defines the signature for a closure that can parse
///     a value of type <typeparamref name="T"/> from <paramref name="input"/>,
///     using the provided <paramref name="formatProvider"/>.
/// </summary>
/// <typeparam name="T">
///     The type of the value being parsed.
/// </typeparam>
/// <param name="input">
///     The string input to parse.
/// </param>
/// <param name="formatProvider">
///     The format provider to use for parsing.
/// </param>
/// <returns>
///     The parsed value of type <typeparamref name="T"/>.
/// </returns>
public delegate T ValueParser<out T>(string input, IFormatProvider? formatProvider);
