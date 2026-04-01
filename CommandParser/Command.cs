using CommandParser.DomainModel;
using CommandParser.Validation;
using System.Linq.Expressions;
using System.Reflection;

namespace CommandParser;

public abstract class Command(Type modelType, string name, string? description = null)
{
    internal Type ModelType { get; } = modelType;
    internal Func<object, CancellationToken, Task<int>> Callback { get; set; } =
        (_, _) => Task.FromResult(0);
    internal List<Validator> Validators { get; set; } = [];

    internal Dictionary<PropertyInfo, OptionMetadata> options = [];
    internal Dictionary<PropertyInfo, ArgumentMetadata> arguments = [];
    internal HashSet<Command> subcommands = [];

    /// <summary>
    ///     Gets the name of the command, which is used to identify it when parsing input.
    ///     The name is also used in help text to refer to the command.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    ///     Gets the description of the command.
    /// </summary>
    public string Description { get; } = description ?? string.Empty;

    /// <summary>
    ///     Adds the specified <paramref name="subcommand"/> to the collection of subcommands for this command.
    /// </summary>
    /// <param name="subcommand">The subcommand to add to the collection. This parameter cannot be null.</param>
    /// <returns>
    ///     <see langword="true"/> if the subcommand was successfully added; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Subcommand(Command subcommand)
        => subcommands.Add(subcommand);

    public override bool Equals(object? obj)
        => obj is Command other && Name == other.Name;

    public override int GetHashCode()
        => Name.GetHashCode();
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
        Callback =
            (model, _) => Task.FromResult(callback((TModel)model));
    }

    /// <summary>
    ///     Sets the callback function to be invoked when the command is executed.
    /// </summary>
    /// <param name="callback">
    ///     A delegate to execute custom logic based on the parsed model.
    /// </param>
    public void SetCallback(Action<TModel> callback)
    {
        Callback =
            (model, _) =>
            {
                callback((TModel)model);
                return Task.FromResult(0);
            };
    }

    /// <summary>
    ///     Sets the asynchronous callback function to be invoked when the command is executed.
    /// </summary>
    /// <param name="callback">
    ///     A delegate to execute custom asynchronous logic based on the parsed model.
    ///     The delegate returns an integer that can be used as an exit code or status code after command execution.
    /// </param>
    public void SetCallback(Func<TModel, Task<int>> callback)
    {
        Callback =
            async (model, _) => await callback((TModel)model).ConfigureAwait(false);
    }

    /// <summary>
    ///     Sets the asynchronous callback function to be invoked when the command is executed.
    /// </summary>
    /// <param name="callback">
    ///     A delegate to execute custom asynchronous logic based on the parsed model.
    /// </param>
    public void SetCallback(Func<TModel, Task> callback)
    {
        Callback =
            async (model, _) =>
            {
                await callback((TModel)model).ConfigureAwait(false);
                return 0;
            };
    }

    /// <summary>
    ///     Sets the asynchronous callback function to be invoked when the command is executed.
    /// </summary>
    /// <param name="callback">
    ///     A delegate to execute custom asynchronous logic based on the parsed model.
    ///     The delegate returns an integer that can be used as an exit code or status code after command execution.
    /// </param>
    public void SetCallback(Func<TModel, CancellationToken, Task<int>> callback)
    {
        Callback =
            async (model, ct) => await callback((TModel)model, ct).ConfigureAwait(false);
    }

    /// <summary>
    ///     Sets the asynchronous callback function to be invoked when the command is executed.
    /// </summary>
    /// <param name="callback">
    ///     A delegate to execute custom asynchronous logic based on the parsed model.
    /// </param>
    public void SetCallback(Func<TModel, CancellationToken, Task> callback)
    {
        Callback =
            async (model, ct) =>
            {
                await callback((TModel)model, ct).ConfigureAwait(false);
                return 0;
            };
    }

    /// <summary>
    ///     Registers a validator to be invoked before executing the command to validate the parsed model.
    /// </summary>
    /// <remarks>
    ///     This method is for high-level validation of the entire model, and the relationships between its properties.
    ///     It is invoked after all arguments and options have been parsed, validated and bound to the model.
    ///     <para>
    ///         If you wish to validate individual properties of the model, consider using
    ///         <see cref="ArgumentBuilder{TModel, TPop}.WithValidator"/> and <see cref="OptionBuilder{TModel, TProp}.WithValidator"/>
    ///         when configuring the arguments and options for this command. If you do, you can consider all model properties
    ///         to be individually valid when creating the validator you pass to this method.
    ///     </para>
    /// </remarks>
    /// <param name="validator">The delegate with the validation logic.</param>
    public void WithValidator(Validator<TModel> validator)
    {
        Validators.Add(
            (in obj, ctx) => validator((TModel?)obj, ctx));
    }
}
