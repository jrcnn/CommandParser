using CommandParser;
using CommandParser.DomainModel;
using CommandParser.Validation;
using CommandParser.ValueParsing;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Test.SymbolBuilderTestData;

namespace Test;

[TestFixture(
    Description = $"Tests for the configuration surface of {nameof(ArgumentBuilder<,>)}",
    TestName = "Argument Builder Tests",
    TestOf = typeof(ArgumentBuilder<,>))]
public class ArgumentBuilderTests
{
    private static Command<TestModel> helperCommand = null!;

    [SetUp]
    public void Setup()
    {
        helperCommand = new Command<TestModel>("testcommand");
    }

    [Test]
    public void TestCreateArgumentBuilder()
    {
        ArgumentBuilder<TestModel, string> argBuilder = helperCommand.Argument(m => m.StringArgument);
        ArgumentMetadata metadata = argBuilder.Metadata;

        PropertyInfo expectedProperty = typeof(TestModel).GetProperty(nameof(TestModel.StringArgument))!;

        Assert.Multiple(() =>
        {
            Assert.That(metadata.TargetProperty, Is.EqualTo(expectedProperty),
                $"Argument failed to capture the target property '{nameof(TestModel.StringArgument)}'");

            Assert.That(metadata.ValueType, Is.EqualTo(typeof(string)),
                $"Argument failed to capture the type of the target property '{nameof(TestModel.StringArgument)}'");
        });
    }

    [Test]
    public void TestWithName()
    {
        const string expectedName = "Custom Name";
        ArgumentBuilder<TestModel, string> argBuilder = helperCommand.Argument(m => m.StringArgument)
            .WithName(expectedName);

        Assert.That(argBuilder.Metadata.Name, Is.EqualTo(expectedName),
            $"'{nameof(ArgumentBuilder<,>.WithName)}' failed to set the argument name correctly");
    }

    [Test]
    public void TestWithDescription()
    {
        const string expectedDescription = "This is a custom description";
        ArgumentBuilder<TestModel, string> argBuilder = helperCommand.Argument(m => m.StringArgument)
            .WithDescription(expectedDescription);

        Assert.That(argBuilder.Metadata.Description, Is.EqualTo(expectedDescription),
            $"'{nameof(ArgumentBuilder<,>.WithDescription)}' failed to set the argument description correctly");
    }

    [Test]
    [SuppressMessage("Style", "IDE0039:Use local function", Justification = "Delegate type remains explicit for clarity")]
    public void TestWithParser()
    {
        ValueParser<string> customParser = (input, provider) => input.ToUpperInvariant();
        ArgumentBuilder<TestModel, string> argBuilder = helperCommand.Argument(m => m.StringArgument)
            .WithParser(customParser);

        Assert.That(argBuilder.Metadata.Parser, Is.Not.Null,
            $"'{nameof(ArgumentBuilder<,>.WithParser)}' failed to set the parser");

        string input = "test";
        string expectedOutput = "TEST";
        object? actualOutput = argBuilder.Metadata.Parser!(input, null!);

        Assert.That(actualOutput, Is.InstanceOf<string>(),
            $"The parser set by '{nameof(ArgumentBuilder<,>.WithParser)}' did not return the expected type");

        Assert.That(actualOutput, Is.EqualTo(expectedOutput),
            $"The parser set by '{nameof(ArgumentBuilder<,>.WithParser)}' did not produce the expected output");
    }

    [Test]
    [SuppressMessage("Style", "IDE0039:Use local function", Justification = "Delegate type remains explicit for clarity")]
    public void TestWithValidator()
    {
        const string validationErrorMessage = "Value must be 'valid'";
        Validator<string> customValidator = (in value, ctx) =>
        {
            if (value != "valid")
            {
                ctx.AddError(validationErrorMessage);
            }
        };

        ArgumentBuilder<TestModel, string> argBuilder = helperCommand.Argument(m => m.StringArgument)
            .WithValidator(customValidator);

        Assert.That(argBuilder.Metadata.Validators, Has.Count.EqualTo(1),
            $"'{nameof(ArgumentBuilder<,>.WithValidator)}' failed to set the validator");

        ValidationContext validationContext = new(nameof(TestModel.StringArgument));
        argBuilder.Metadata.Validators[0]("invalid", validationContext);

        Assert.That(validationContext.Errors, Has.Count.EqualTo(1),
            $"The validator added by '{nameof(ArgumentBuilder<,>.WithValidator)}' did not produce a validation error");

        Assert.That(validationContext.Errors[0], Is.EqualTo(validationErrorMessage),
            $"The validator added by '{nameof(ArgumentBuilder<,>.WithValidator)}' did not produce the expected validation error message");
    }
}
