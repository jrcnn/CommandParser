using System.Numerics;
using System.Reflection;

namespace CommandParser.ValueParsing;

internal static class ParserProvider
{
    private static readonly Dictionary<Type, CliValueParser> Cache = new()
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

    public static CliValueParser? GetParser<T>()
        => GetParser(typeof(T));

    public static CliValueParser? GetParser(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        if (Cache.TryGetValue(type, out CliValueParser? parser))
        {
            return parser;
        }

        CliValueParser? newParser = CreateParser(type);
        if (newParser is null)
        {
            return null;
        }

        Cache[type] = newParser;
        return newParser;
    }

    private static CliValueParser? CreateParser(Type type)
    {
        MethodInfo? parseMethod = type.GetMethod(
            "Parse",
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.Static,
            [typeof(string), typeof(IFormatProvider)]);

        if (parseMethod is not null && parseMethod.IsStatic && parseMethod.ReturnType == type)
        {
            return CreateParser(parseMethod, hasFormatProvider: true);
        }

        parseMethod = type.GetMethod(
            "Parse",
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.Static,
            [typeof(string)]);

        if (parseMethod is not null && parseMethod.IsStatic && parseMethod.ReturnType == type)
        {
            return CreateParser(parseMethod);
        }

        ConstructorInfo? ctor = type.GetConstructor([typeof(string), typeof(IFormatProvider)]);
        if (ctor is not null)
        {
            return CreateParser(ctor, hasFormatProvider: true);
        }

        ctor = type.GetConstructor([typeof(string)]);
        if (ctor is not null)
        {
            return CreateParser(ctor);
        }

        return null;
    }

    private static CliValueParser CreateParser(MethodInfo method, bool hasFormatProvider = false)
    {
        return (input, provider) =>
        {
            try
            {
                return method.Invoke(null, hasFormatProvider
                    ? [input, provider]
                    : [input])!;
            }
            catch (TargetInvocationException ex)
            {
                // Unwrap the inner exception to provide more meaningful error messages
                throw ex.InnerException ?? ex;
            }
        };
    }

    private static CliValueParser CreateParser(ConstructorInfo ctor, bool hasFormatProvider = false)
    {
        return (input, provider) =>
        {
            try
            {
                return ctor.Invoke(hasFormatProvider
                    ? [input, provider]
                    : [input])!;
            }
            catch (TargetInvocationException ex)
            {
                // Unwrap the inner exception to provide more meaningful error messages
                throw ex.InnerException ?? ex;
            }
        };
    }
}
