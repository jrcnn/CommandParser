using CommandParser.DomainModel;
using System.Linq.Expressions;
using System.Reflection;

namespace CommandParser;

public abstract class Command(Type modelType, string name, string? description = null)
{
    protected Type ModelType { get; } = modelType;

    private protected Dictionary<PropertyInfo, OptionMetadata> options = [];
    private protected Dictionary<PropertyInfo, ArgumentMetadata> arguments = [];

    protected Func<object, int> callback = _ => 0;

    internal List<Command> subcommands = [];

    public string Name { get; set; } = name;
    public HashSet<string> Aliases { get; } = new(StringComparer.Ordinal);
    public string Description { get; set; } = description ?? string.Empty;
}

/// <summary>
///     Represents a command that can be configured with arguments and options for a specified model type.
/// </summary>
/// <remarks>
///     Each property of the model can have only one argument or one option defined. Attempting to define
///     multiple arguments or options for the same property will result in an exception.
/// </remarks>
/// <typeparam name="TModel">The type of the model that the command operates on.</typeparam>
/// <param name="name">The name of the command, used to identify it.</param>
/// <param name="description">An optional description that provides additional context about the command.</param>
public class Command<TModel>(string name, string? description = null) : Command(typeof(TModel), name, description)
{
    /// <summary>
    ///     Defines an argument for the specified property of the model and returns a builder for configuring the argument.
    /// </summary>
    /// <remarks>
    ///     Use this method to add arguments for model properties. Each property can have only one
    ///     argument or one option defined. Attempting to define multiple arguments or options for the same property will
    ///     result in an exception.
    /// </remarks>
    /// <typeparam name="TProp">
    ///     The type of the property for which the argument is being defined.
    /// </typeparam>
    /// <param name="propertySelector">
    ///     An expression that selects the property of the model to associate with the argument. Cannot be null.
    /// </param>
    /// <returns>
    ///     An <see cref="ArgumentBuilder{TModel, TProp}"/> instance for configuring the argument associated with the specified property.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if an option or argument for the specified property has already been defined.
    /// </exception>
    public ArgumentBuilder<TModel, TProp> Argument<TProp>(Expression<Func<TModel, TProp>> propertySelector)
    {
        ArgumentBuilder<TModel, TProp> builder = new(propertySelector);
        if (options.ContainsKey(builder.Metadata.TargetProperty))
        {
            throw new InvalidOperationException($"An option for property '{builder.Metadata.TargetProperty.Name}' is already defined");
        }

        if (!arguments.TryAdd(builder.Metadata.TargetProperty, builder.Metadata))
        {
            throw new InvalidOperationException($"An argument for property '{builder.Metadata.TargetProperty.Name}' is already defined");
        }

        return builder;
    }

    /// <summary>
    ///     Defines an option for the specified property of the model and returns a builder for configuring the option.
    /// </summary>
    /// <remarks>
    ///     Use this method to add options for model properties. Each property can have only one
    ///     argument or one option defined. Attempting to define multiple arguments or options for the same property will
    ///     result in an exception.
    /// </remarks>
    /// <typeparam name="TProp">
    ///     The type of the property for which the option is being defined.
    /// </typeparam>
    /// <param name="propertySelector">
    ///     An expression that selects the property of the model to associate with the option. Cannot be null.
    /// </param>
    /// <returns>
    ///     An <see cref="OptionBuilder{TModel, TProp}"/> instance for configuring the option associated with the specified property.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if an option or argument for the specified property has already been defined.
    /// </exception>
    public OptionBuilder<TModel, TProp> Option<TProp>(Expression<Func<TModel, TProp>> propertySelector)
    {
        OptionBuilder<TModel, TProp> builder = new(propertySelector);
        if (arguments.ContainsKey(builder.Metadata.TargetProperty))
        {
            throw new InvalidOperationException($"An argument for property '{builder.Metadata.TargetProperty.Name}' is already defined");
        }

        if (!options.TryAdd(builder.Metadata.TargetProperty, builder.Metadata))
        {
            throw new InvalidOperationException($"An option for property '{builder.Metadata.TargetProperty.Name}' is already defined");
        }

        return builder;
    }

    /// <summary>
    ///     Sets the callback function to be invoked when the command is executed.
    /// </summary>
    /// <param name="callback">
    ///     A delegate to execute custom logic based on the parsed model.
    ///     The delegate returns an integer that can be used as an exit code or status code after command execution.
    /// </param>
    public void SetCallback(Func<TModel, int> callback)
    {
        this.callback =
            model => callback((TModel)model);
    }

    /// <summary>
    ///     Sets the callback function to be invoked when the command is executed.
    /// </summary>
    /// <param name="callback">
    ///     A delegate to execute custom logic based on the parsed model.
    /// </param>
    public void SetCallback(Action<TModel> callback)
    {
        this.callback =
            model =>
            {
                callback((TModel)model);
                return 0;
            };
    }
}
