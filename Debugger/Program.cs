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
