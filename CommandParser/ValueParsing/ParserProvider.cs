using System.Numerics;

namespace CommandParser.ValueParsing;

internal static class ParserProvider
{
    public static Dictionary<Type, CliValueParser> Cache = new()
    {
        // the most common defaults are provided to avoid reflection for these
        { typeof(String),         (input, _)        => input },
        { typeof(Char),           (input, _)        => Char.Parse(input) },
        { typeof(Boolean),        (input, _)        => Boolean.Parse(input) },
        { typeof(SByte),          (input, provider) => SByte.Parse(input, provider) },
        { typeof(Byte),           (input, provider) => Byte.Parse(input, provider) },
        { typeof(Int16),          (input, provider) => Int16.Parse(input, provider) },
        { typeof(Int32),          (input, provider) => Int32.Parse(input, provider) },
        { typeof(Int64),          (input, provider) => Int64.Parse(input, provider) },
        { typeof(Int128),         (input, provider) => Int128.Parse(input, provider) },
        { typeof(UInt16),         (input, provider) => UInt16.Parse(input, provider) },
        { typeof(UInt32),         (input, provider) => UInt32.Parse(input, provider) },
        { typeof(UInt64),         (input, provider) => UInt64.Parse(input, provider) },
        { typeof(UInt128),        (input, provider) => UInt128.Parse(input, provider) },
        { typeof(BigInteger),     (input, provider) => BigInteger.Parse(input, provider) },
        { typeof(Half),           (input, provider) => Half.Parse(input, provider) },
        { typeof(Single),         (input, provider) => Single.Parse(input, provider) },
        { typeof(Double),         (input, provider) => Double.Parse(input, provider) },
        { typeof(Decimal),        (input, provider) => Decimal.Parse(input, provider) },
        { typeof(DateTime),       (input, provider) => DateTime.Parse(input, provider) },
        { typeof(DateOnly),       (input, provider) => DateOnly.Parse(input, provider) },
        { typeof(TimeOnly),       (input, provider) => TimeOnly.Parse(input, provider) },
        { typeof(DateTimeOffset), (input, provider) => DateTimeOffset.Parse(input, provider) },
        { typeof(TimeSpan),       (input, provider) => TimeSpan.Parse(input, provider) },
        { typeof(Guid),           (input, provider) => Guid.Parse(input, provider) },
        { typeof(Uri),            (input, _)        => new Uri(input) },
        { typeof(Version),        (input, _)        => Version.Parse(input) },
    };
}
