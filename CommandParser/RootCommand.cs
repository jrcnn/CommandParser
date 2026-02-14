using System.Reflection;

namespace CommandParser;

public class RootCommand<TModel>(string? programName = null, string? description = null) :
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
