# CommandParser

*A command line parsing and executing framework with a fluent configuration interface*

Want to create a command line application and not have to think too much about
the necessary boilerplate to parse your CLI input? **CommandParser** enables you to
quickly configure and customize your command line interface to your liking, so you can
get to writing application logic as soon as possible.

The framework provides sensible defaults that you may never have to override, so the most
common use cases can be handled with minimal effort.

## Example

Let's jump right in and see the framework in action, with a simple example at first.

Let's create an app that adds two numbers!

```csharp
public static int Add(int left, int right) => left + right;
```

This is the logic we want to invoke. Let us see how we can wire the `Add` method to be
invokable in the command line. First, we have to create a model for the root command
(every command requires a model):

```csharp
private class AdditionModel
{
    public int Left { get; set; }
    public int Right { get; set; }
}
```

That's it. Now inside `Main`, we should be able to configure everything we want in just a few lines:

```csharp
public static void Main(string[] args)
{
    RootCommand<AdditionModel> rootCommand = new(
        description: "Adds two numbers and displays the result",
        programName: "add");

    rootCommand.Argument(model => model.Left)
        .WithDescription("The first number");

    rootCommand.Argument(model => model.Right)
        .WithDescription("The second number");
}
```

We've now set up two mandatory arguments for the root command. The next step is to inject
our addition logic into it:

```csharp
rootCommand.SetCallback(model =>
{
    int result = Add(model.Left, model.Right);
    Console.WriteLine($"The result of adding {model.Left} and {model.Right} is: {result}");
});
```

The very last thing we have to do now is to invoke the command:

```csharp
rootCommand.Execute(args);
```

All of this together:

```csharp
using CommandParser;

namespace Debugger;

internal class Program
{
    private class AdditionModel
    {
        public int Left { get; set; }
        public int Right { get; set; }
    }

    public static void Main(string[] args)
    {
        RootCommand<AdditionModel> rootCommand = new(
            description: "Adds two numbers and displays the result",
            programName: "add");

        rootCommand.Argument(model => model.Left)
            .WithDescription("The first number");

        rootCommand.Argument(model => model.Right)
            .WithDescription("The second number");

        rootCommand.SetCallback(model =>
        {
            int result = Add(model.Left, model.Right);
            Console.WriteLine($"The result of adding {model.Left} and {model.Right} is: {result}");
        });

        rootCommand.Execute(args);
    }

    public static int Add(int left, int right) => left + right;
}
```

Our first little app is now complete. Parsing of the CLI pipeline and the values provided to the left and right
arguments will all be handled by the framework.

## Configuration

Let us now go over the configuration capabilities provided by the library. We'll cover commands,
arguments and options in this order.

### Command Configuration

To create a `Command`, simply invoke its constructor. This is pretty self-explanatory, and was already
shown above in the [Example](#example) section. The constructor allows configuration of the two basic
properties of a command symbol: its name and description. The name will be used to invoke it from the CLI,
and the description is shown when the user requests usage information. So far, so good.

When you create a `Command`, you also specify its backing model. The model can be any non-static type you choose,
and its properties will act as hooks that the command's arguments and options will be bound to.

For the purpose of this guide, we'll use the following model for configuration:

```csharp
public class TutorialModel
{
    // Arguments:
    public string Name { get; set; }
    public Job Occupation { get; set; }

    // Options:
    public DateOnly Date { get; set; }
    public bool IsElderly { get; set; }
    public int OwnedPetCount { get; set; }
}
```

Where `Job`:

```csharp
public class Job
{
    public string JobTitle { get; set; }
    public string Company { get; set; }
}
```

The job of our command will be to take a person represented by `TutorialModel` and register them
in some database. The logic for this lives in the following method readily available:

```csharp
public async Task<Result> Register(TutorialModel person, CancellationToken ct) { /* . . . */ }
```

Let's configure the argument properties of our model:

```csharp
cmd.Argument(model => model.Name);
cmd.Argument(model => model.Occupation);
```

And now our options:

```csharp
cmd.Option(model => model.Date);
cmd.Option(model => model.IsElderly);
cmd.Option(model => model.OwnedPetCount);
```

We know that the owner of the database set a restriction that elderly people
cannot own any pets. We can create a model-level validator to enforce this invariant.

Generally, use model validation to impose restrictions to the model *as a whole*, the
relationships between the properties. For robust a robust validation, do not use the
model validator to check individual arguments and options. It's better to attach *symbol-level* validators
to each argument and option to validate their values individually, and let the model validator validate only
the model structure itself.

We'll also do validation in this example accordingly. We can assume that the individual properties are valid,
because symbol validators always run before the model validator.

```csharp
cmd.WithValidator((in model, context) =>
{
    if (model.IsElderly && model.OwnedPetCount > 0)
    {
        context.AddError("Elderly people cannot own any pets!");
    }
});
```

Great, our invariant is now set up in the command's validator. You can create any number of validators with `cmd.WithValidator`,
and all of them will be run. The errors will be accumulated and reported together.

Let's attach our database registry logic to this command, so it's automatically invoked by the framework:

```csharp
cmd.SetCallback(async (model, cancellationToken) => await Register(model, cancellationToken) switch
{
    Result.Success => 0,
    Result.Canceled => 1
    _ => 2
});
```

In this example, our callback also returns an exit code based on the execution result.

And with this, our command is done! However, our arguments and options are not configured
at all yet. `cmd.Option` and `cmd.Argument` actually return builders that can be used to further
configure the symbol. Let's do that.

### Argument configuration

`Command<TModel>.Argument<TProp>(propertySelector)` returns an `ArgumentBuilder<TModel, TProp>` instance that has the following
methods for customization:

- `.WithName(string argName)`: Sets the name for this argument. The default name for an argument is the property name in upper case, so this method doesn't need to be called to give the argument a name, only to override it.
- `.WithDescription(string description)`: Sets the description for this argument. This is for printing usage information, usually by setting `--help` on a command.
- `.WithParser(ValueParser<TProp> parser)`: Sets a closure that will be used to parse the CLI value provided to this argument. For most cases, you do not need to manually provide a parser (see chapter [Value Parsing](#value-parsing)).
- `.WithValidator(Validator<TProp> validator)`: Registers a closure that will be used to validate the parsed value for this argument. Any number of validators can be registered.

With these in mind, let's modify our argument configurations to have everything we need:

```csharp
cmd.Argument(model => model.Name)
    .WithDescription("The person's name")
    .WithValidator((in name, context) =>
    {
        var names = name.Split(' ');
        if (names.Length < 2)
        {
            context.AddError("The first and last names should both be provided");
        }
    });

cmd.Argument(model => model.Occupation)
    .WithDescription("The person's job info (format: <COMPANY_NAME>,<JOB_TITLE>)")
    .WithParser((input, formatProvider) =>
    {
        // just a simple parser that will recognize the above format
        var split = input.Split(',');
        if (split.Length != 2)
        {
            throw new FormatException("The occupation was written in an incorrect format");
        }

        return new Job()
        {
            Company = split[0],
            JobTitle = split[1]
        };
    })
    .WithValidator((in occupation, context) =>
    {
        if (string.IsNullOrWhiteSpace(occupation.JobTitle))
        {
            context.AddError("Job title cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(occupation.Company))
        {
            context.AddError("Company name cannot be empty");
        }
    });
```

### Option configuration

With our arguments fully configured, let's move on to our options.

`Command<TModel>.Option<TProp>(propertySelector)` returns an `OptionBuilder<TModel, TProp>` instance that has the following
methods for customization:


