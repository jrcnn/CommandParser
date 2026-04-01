using CommandParser;
using System.Reflection;
using Test.SymbolBuilderTestData;

namespace Test;

[TestFixture(
    Description = $"Tests for the configuration surface of '{nameof(Command<>)}'",
    TestName = "Command Configuration Tests",
    TestOf = typeof(Command<>))]
public class CommandConfigurationTests
{
    [Test]
    public void TestModelType()
    {
        Command<TestModel> cmd = new("");
        Assert.That(cmd.ModelType, Is.EqualTo(typeof(TestModel)),
            "The command's model type was not set correctly");
    }

    [Test]
    public void TestNameDescription()
    {
        string name = "testcommand";
        string description = "testdescription";
        Command<TestModel> cmd = new(name, description);

        Assert.Multiple(() =>
        {
            Assert.That(cmd.Name, Is.EqualTo(name),
                "The command's name was not correctly set through the constructor");

            Assert.That(cmd.Description, Is.EqualTo(description),
                "The command's description was not correctly set through the constructor");
        });
    }

    [Test]
    public void TestEquality()
    {
        Command<object> cmd1 = new("name1");
        Command<TestModel> cmd2 = new("name1");
        Command<TestModel> cmd3 = new("name2");

        Assert.Multiple(() =>
        {
            Assert.That((Command)cmd1, Is.EqualTo((Command)cmd2),
                "Two command instances with the same name should be equal, even if they operate on different model types");

            Assert.That(cmd1.GetHashCode(), Is.EqualTo(cmd2.GetHashCode()),
                "Two command instances with the same name should produce the same hash code, even if they operate on different model types");

            Assert.That((Command)cmd2, Is.Not.EqualTo((Command)cmd3),
                "Two command instances with different names should not be equal, even if they operate on the same model type");
        });
    }

    [Test]
    public void TestAddSubcommand()
    {
        Command<object> subCmd = new("");
        Command<object> cmd = new("");

        cmd.Subcommand(subCmd);

        Assert.That(cmd.subcommands, Has.Count.EqualTo(1), "The subcommand was not added");
        Assert.That(cmd.subcommands.First(), Is.EqualTo(subCmd), $"The added subcommand is not equal to the original '{nameof(Command)}' instance");

        cmd.Subcommand(subCmd);

        Assert.That(cmd.subcommands, Has.Count.EqualTo(1), "The same subcommand should not be added twice");
    }

    [Test]
    public void TestAddArgument()
    {
        Command<TestModel> cmd = new("");

        cmd.Argument(m => m.StringArgument);

        Assert.That(cmd.arguments, Has.Count.EqualTo(1), "The argument was not added");

        PropertyInfo addedProp = cmd.arguments.First().Key;
        PropertyInfo expectedProp = typeof(TestModel).GetProperty(nameof(TestModel.StringArgument))!;

        Assert.That(addedProp, Is.EqualTo(expectedProp),
            $"The added argument's property '{addedProp.Name}' does not match the expected property '{expectedProp.Name}'");

        Assert.Throws<InvalidOperationException>(() => cmd.Argument(m => m.StringArgument),
            $"Adding the same argument twice should throw an '{nameof(InvalidOperationException)}'");
    }

    [Test]
    public void TestAddOption()
    {
        Command<TestModel> cmd = new("");

        cmd.Option(m => m.BoolOption);
        cmd.Option(m => m.DecimalOption);

        Assert.That(cmd.options, Has.Count.EqualTo(2), "The options were not added");

        PropertyInfo boolProp = typeof(TestModel).GetProperty(nameof(TestModel.BoolOption))!;
        PropertyInfo decimalProp = typeof(TestModel).GetProperty(nameof(TestModel.DecimalOption))!;

        Assert.Multiple(() =>
        {
            Assert.That(cmd.options, Does.ContainKey(boolProp),
                $"Missing option for property '{boolProp.Name}'");

            Assert.That(cmd.options, Does.ContainKey(decimalProp),
                $"Missing option for property '{decimalProp.Name}'");
        });

        Assert.Multiple(() =>
        {
            Assert.Throws<InvalidOperationException>(() => cmd.Option(m => m.BoolOption),
                $"Adding the same option twice should throw an '{nameof(InvalidOperationException)}'");

            Assert.Throws<InvalidOperationException>(() => cmd.Option(m => m.DecimalOption),
                $"Adding the same option twice should throw an '{nameof(InvalidOperationException)}'");
        });
    }

    [Test]
    public void TestAddArgument_OptionAlreadyExists()
    {
        Command<TestModel> cmd = new("");
        cmd.Option(m => m.StringArgument);

        Assert.Throws<InvalidOperationException>(() => cmd.Argument(m => m.StringArgument),
            $"Adding an argument for a property that is already configured as an option should throw an '{nameof(InvalidOperationException)}'");
    }

    [Test]
    public void TestAddOption_ArgumentAlreadyExists()
    {
        Command<TestModel> cmd = new("");
        cmd.Argument(m => m.BoolOption);

        Assert.Throws<InvalidOperationException>(() => cmd.Option(m => m.BoolOption),
            $"Adding an option for a property that is already configured as an argument should throw an '{nameof(InvalidOperationException)}'");
    }
}
