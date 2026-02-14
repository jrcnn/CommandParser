using System.Reflection;

namespace CommandParser;

public class RootCommand<TModel>(string? description = null, string? programName = null) :
    Command<TModel>(
        programName
            ?? Assembly.GetEntryAssembly()?.GetName().Name
            ?? string.Empty,
        description)
{
    public int Execute(string[] args)
    {
        throw new NotImplementedException();
    }

    public async Task<int> ExecuteAsync(string[] args)
    {
        throw new NotImplementedException();
    }
}
