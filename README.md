# CommandParser

*A command line parsing and executing framework with a fluent configuration interface*

Want to create a command line application and not have to think too much about
the necessary boilerplate to parse your CLI input? **CommandParser** enables you to
quickly configure and customize your command line interface to your liking, so you can
get to writing application logic as soon as possible.

The framework provides sensible defaults that you may never have to override.

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
