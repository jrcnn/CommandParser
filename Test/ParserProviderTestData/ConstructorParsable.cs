namespace Test.ParserProviderTestData;

internal sealed class ConstructorParsable(string s, IFormatProvider? fp)
{
    public int Value { get; } = int.Parse(s, fp);
}
