using CommandParser;

namespace Debugger;

/// <summary>
///     This is an API example for the CLI framework.
/// </summary>
internal class Program
{
    private class BirthdayModel
    {
        public string NameToGreet { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    private class GreetingModel
    {
        public string NameToGreet { get; set; } = string.Empty;
        public string TextForGreeting { get; set; } = string.Empty;
        public bool UseCapitalLetters { get; set; }
    }

    static int Main(string[] args)
    {
        Command<BirthdayModel> birthdayCommand = new(
            "birthday",
            "A command for greeting the user on their birthday");

        birthdayCommand.Argument(model => model.NameToGreet)
            .WithName("NAME")
            .WithDescription("The name to greet");

        birthdayCommand.Argument(model => model.Age)
            .WithName("AGE")
            .WithDescription("The age of the person celebrating their birthday");

        birthdayCommand.SetCallback(model =>
        {
            Console.WriteLine($"Happy {model.Age}th birthday, {model.NameToGreet}!");
        });

        RootCommand<GreetingModel> root = new(
            "A program for greeting the user with a custom text");

        root.Argument(model => model.NameToGreet)
            .WithName("NAME")
            .WithDescription("The name to greet");

        root.Argument(model => model.TextForGreeting)
            .WithName("TEXT")
            .WithDescription("The text to say as the greeting");

        root.Option(model => model.UseCapitalLetters)
            .WithName("--capital")
            .WithAlias("-c")
            .WithDescription("Specifies whether to display the greeting text in capital letters")
            .WithDefaultValue(false);

        root.Subcommand(birthdayCommand);

        root.SetCallback(model =>
        {
            string greeting = $"{model.TextForGreeting}, {model.NameToGreet}!";
            if (model.UseCapitalLetters)
            {
                greeting = greeting.ToUpper();
            }

            Console.WriteLine(greeting);
        });

        return root.Execute(args);
    }
}
