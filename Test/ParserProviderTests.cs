using CommandParser.ValueParsing;
using System.Globalization;
using Test.ParserProviderTestData;

namespace Test;

public class ParserProviderTests
{
    /// <summary>
    ///     REQUIREMENT:
    ///     <see cref="ParserProvider"/> shall return a non-<see langword="null"/> parser for types that have a
    ///     static 'Parse(string, IFormatProvider?)' method, a static 'Parse(string)' method, a constructor
    ///     with parameters 'string' and 'IFormatProvider?', or a constructor with a single 'string' parameter.
    ///     
    ///     If the type does not have any of the above, <see cref="ParserProvider"/> shall return <see langword="null"/>.
    /// </summary>
    [Test]
    public void GetParser_ReturnsParser()
    {
        ValueParser? parseParsable           = ParserProvider.GetParser(typeof(ParseParsable));
        ValueParser? parseParsableStringOnly = ParserProvider.GetParser(typeof(ParseParsableStringOnly));
        ValueParser? ctorParsable            = ParserProvider.GetParser(typeof(ConstructorParsable));
        ValueParser? ctorParsableStringOnly  = ParserProvider.GetParser(typeof(ConstructorParsableStringOnly));
        ValueParser? notParsable             = ParserProvider.GetParser(typeof(NotParsable));

        Assert.Multiple(() =>
        {
            Assert.That(parseParsable, Is.Not.Null,
                $"'{nameof(ParserProvider.GetParser)}' should return a non-null parser for a type with a 'Parse(string, IFormatProvider?)' method");

            Assert.That(parseParsableStringOnly, Is.Not.Null,
                $"'{nameof(ParserProvider.GetParser)}' should return a non-null parser for a type with a 'Parse(string)' method");

            Assert.That(ctorParsable, Is.Not.Null,
                $"'{nameof(ParserProvider.GetParser)}' should return a non-null parser for a type that has a constructor with parameters 'string' and 'IFormatProvider?'");

            Assert.That(ctorParsableStringOnly, Is.Not.Null,
                $"'{nameof(ParserProvider.GetParser)}' should return a non-null parser for a type that has a constructor with a single 'string' parameter");

            Assert.That(notParsable, Is.Null,    
                $"'{nameof(ParserProvider.GetParser)}' should return null for an unparsable type (no Parse method, and no viable constructor)");
        });
    }

    /// <summary>
    ///     REQUIREMENT:
    ///     The parsers returned by <see cref="ParserProvider.GetParser{T}"/> shall correctly
    ///     parse values according to the parsing method or constructor they are based on.
    ///     
    ///     <para>
    ///         The correctness of the returned parser cannot differ from the Parse method or constructor it is based on.
    ///         If the Parse method or constructor correctly parses a value, the parser returned by <see cref="ParserProvider.GetParser{T}"/>
    ///         shall also correctly parse that value. If the Parse method or constructor fails to parse a value, the parser returned by
    ///         <see cref="ParserProvider.GetParser{T}"/> shall also fail to parse that value.
    ///     </para>
    /// </summary>
    [Test]
    public void GetParser_CanParseCorrectValue()
    {
        ValueParser parseParsable           = ParserProvider.GetParser(typeof(ParseParsable))!;
        ValueParser parseParsableStringOnly = ParserProvider.GetParser(typeof(ParseParsableStringOnly))!;
        ValueParser ctorParsable            = ParserProvider.GetParser(typeof(ConstructorParsable))!;
        ValueParser ctorParsableStringOnly  = ParserProvider.GetParser(typeof(ConstructorParsableStringOnly))!;

        ParseParsable? parseParsableResult                          = parseParsable("10",           CultureInfo.InvariantCulture) as ParseParsable;
        ParseParsableStringOnly? parseParsableStringOnlyResult      = parseParsableStringOnly("10", CultureInfo.InvariantCulture) as ParseParsableStringOnly;
        ConstructorParsable? ctorParsableResult                     = ctorParsable("10",            CultureInfo.InvariantCulture) as ConstructorParsable;
        ConstructorParsableStringOnly? ctorParsableStringOnlyResult = ctorParsableStringOnly("10",  CultureInfo.InvariantCulture) as ConstructorParsableStringOnly;

        Assert.Multiple(() =>
        {
            Assert.That(parseParsableResult, Is.Not.Null,
                "The parser based on 'Parse(string, IFormatProvider?)' did not yield the correct result type");

            Assert.That(parseParsableStringOnlyResult, Is.Not.Null,
                "The parser based on 'Parse(string)' did not yield the correct result type");

            Assert.That(ctorParsableResult, Is.Not.Null,
                "The parser based on a constructor with parameters 'string' and 'IFormatProvider?' did not yield the correct result type");

            Assert.That(ctorParsableStringOnlyResult, Is.Not.Null,
                "The parser based on a constructor with a single 'string' parameter did not yield the correct result type");
        });
        
        Assert.Multiple(() =>
        {
            Assert.That(parseParsableResult!.Value, Is.EqualTo(10),
                "The parser based on 'Parse(string, IFormatProvider?)' did not yield the correct value");

            Assert.That(parseParsableStringOnlyResult!.Value, Is.EqualTo(10),
                "The parser based on 'Parse(string)' did not yield the correct value");

            Assert.That(ctorParsableResult!.Value, Is.EqualTo(10),
                "The parser based on a constructor with parameters 'string' and 'IFormatProvider?' did not yield the correct value");

            Assert.That(ctorParsableStringOnlyResult!.Value, Is.EqualTo(10),
                "The parser based on a constructor with a single 'string' parameter did not yield the correct value");
        });
    }

    /// <summary>
    ///     REQUIREMENT:
    ///     The correctness of the returned parser cannot differ from the Parse method or constructor it is based on.
    ///     If the Parse method or constructor correctly parses a value, the parser returned by <see cref="ParserProvider.GetParser{T}"/>
    ///     shall also correctly parse that value. If the Parse method or constructor fails to parse a value, the parser returned by
    ///     <see cref="ParserProvider.GetParser{T}"/> shall also fail to parse that value.
    /// </summary>
    [Test]
    public void GetParser_CannotParseIncorrectValue()
    {
        ValueParser parseParsable           = ParserProvider.GetParser(typeof(ParseParsable))!;
        ValueParser parseParsableStringOnly = ParserProvider.GetParser(typeof(ParseParsableStringOnly))!;
        ValueParser ctorParsable            = ParserProvider.GetParser(typeof(ConstructorParsable))!;
        ValueParser ctorParsableStringOnly  = ParserProvider.GetParser(typeof(ConstructorParsableStringOnly))!;

        Assert.Multiple(() =>
        {
            Assert.That(() => parseParsable("not an int", CultureInfo.InvariantCulture), Throws.InstanceOf<Exception>(),
                "The parser based on 'Parse(string, IFormatProvider?)' should throw an exception for an incorrectly formatted value");

            Assert.That(() => parseParsableStringOnly("not an int", CultureInfo.InvariantCulture), Throws.InstanceOf<Exception>(),
                "The parser based on 'Parse(string)' should throw exception for an incorrectly formatted value");

            Assert.That(() => ctorParsable("not an int", CultureInfo.InvariantCulture), Throws.InstanceOf<Exception>(),
                "The parser based on a constructor with parameters 'string' and 'IFormatProvider?' should throw exception for an incorrectly formatted value");

            Assert.That(() => ctorParsableStringOnly("not an int", CultureInfo.InvariantCulture), Throws.InstanceOf<Exception>(),
                "The parser based on a constructor with a single 'string' parameter should throw exception for an incorrectly formatted value");
        });
    }
}
