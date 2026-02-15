namespace Test.ParserProviderTestData;

internal sealed class ParseParsableStringOnly
{
    public int Value { get; }

    private ParseParsableStringOnly(int value)
    {
        Value = value;
    }

    public static ParseParsableStringOnly Parse(string s)
    {
        return new ParseParsableStringOnly(int.Parse(s));
    }
}
