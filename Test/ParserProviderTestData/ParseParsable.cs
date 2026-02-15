namespace Test.ParserProviderTestData;

internal sealed class ParseParsable
{
    public int Value { get; }

    private ParseParsable(int value)
    {
        Value = value;
    }

    public static ParseParsable Parse(string s, IFormatProvider? provider)
    {
        return new ParseParsable(int.Parse(s, provider));
    }
}
