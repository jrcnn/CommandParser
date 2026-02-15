namespace Test.ParserProviderTestData;

internal sealed class ConstructorParsableStringOnly(string input)
{
    public int Value { get; } = int.Parse(input);
}
