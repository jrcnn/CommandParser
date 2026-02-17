using CommandParser.Validation;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace CommandParser.DomainModel;

/// <summary>
///     Provides an interface for configuring an option's attributes, such as name, description,
///     aliases, required status, and default value.
/// </summary>
/// <remarks>
///     By default, an option is marked required and doesn't have a default value.
///     Such an option will cause a parse failure if the user doesn't provide a value for it.
///     You can change this behavior by calling <see cref="IsRequired"/> and/or
///     <see cref="WithDefaultValue"/> on the builder.
///     <para>
///         An option with a nullable <typeparamref name="TProp"/> type is treated as having a default value of <see langword="null"/> and not being required by default.
///     </para>
///     <para>
///         An option with a <see cref="bool"/> type is a special case - it is treated as having a
///         default value of <see langword="false"/> and is not required. Naturally,
///         this can be overriden by the previously stated methods.
///     </para>
/// </remarks>
/// <typeparam name="TModel">The model that the option is associated with.</typeparam>
/// <typeparam name="TProp">
///     The type of the property on <typeparamref name="TModel"/> that this option represents.
/// </typeparam>
public class OptionBuilder<TModel, TProp>
{
    internal OptionMetadata Metadata { get; }

    internal OptionBuilder(Expression<Func<TModel, TProp>> propertySelector)
    {
        Metadata = new(GetProperty(propertySelector));
    }

    /// <summary>
    ///     Sets the <paramref name="name"/> for the option being built.
    /// </summary>
    /// <param name="name">The display name to assign to the option. Cannot be null.</param>
    /// <returns>The current <see cref="OptionBuilder{TModel, TProp}"/> instance for method chaining.</returns>
    public OptionBuilder<TModel, TProp> WithName([DisallowNull] string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be null or whitespace", nameof(name));
        }

        Metadata.Name = name;
        return this;
    }

    /// <summary>
    ///     Adds an alternative name that can be used to refer to the option being built.
    /// </summary>
    /// <param name="alias">The alias to associate with the option. Cannot be null.</param>
    /// <returns>The current <see cref="OptionBuilder{TModel, TProp}"/> instance for method chaining.</returns>
    public OptionBuilder<TModel, TProp> WithAlias([DisallowNull] string alias)
    {
        if (string.IsNullOrWhiteSpace(alias))
        {
            throw new ArgumentException("Alias cannot be null or whitespace", nameof(alias));
        }

        Metadata.Aliases.Add(alias);
        return this;
    }

    /// <summary>
    ///     Sets the <paramref name="description"/> metadata for the option being built.
    /// </summary>
    /// <param name="description">The description text to associate with the option. Cannot be null.</param>
    /// <returns>The current <see cref="OptionBuilder{TModel, TProp}"/> instance for method chaining.</returns>
    public OptionBuilder<TModel, TProp> WithDescription([DisallowNull] string description)
    {
        Metadata.Description = description;
        return this;
    }

    /// <summary>
    ///     Specifies whether the option is required.
    /// </summary>
    /// <param name="isRequired">A value indicating whether the option must be provided. The default is <see langword="true"/>.</param>
    /// <returns>The current <see cref="OptionBuilder{TModel, TProp}"/> instance for method chaining.</returns>
    public OptionBuilder<TModel, TProp> IsRequired(bool isRequired = true)
    {
        Metadata.IsRequired = isRequired;
        return this;
    }

    /// <summary>
    /// Specifies a default value to use for the option if no value is provided by the user.
    /// </summary>
    /// <param name="defaultValue">The value to use as the default for the option when no explicit value is supplied.</param>
    /// <returns>The current <see cref="OptionBuilder{TModel, TProp}"/> instance for method chaining.</returns>
    public OptionBuilder<TModel, TProp> WithDefaultValue(TProp defaultValue)
    {
        Metadata.HasDefaultValue = true;
        Metadata.DefaultValue = defaultValue;
        return this;
    }

    /// <summary>
    ///     Sets the validation logic for the option being built using the provided <paramref name="predicate"/>.
    /// </summary>
    /// <param name="validator">The delegate with the validation logic.</param>
    /// <returns>The current <see cref="ArgumentBuilder{TModel, TProp}"/> instance for method chaining.</returns>
    public OptionBuilder<TModel, TProp> WithValidator(Validator<TProp> validator)
    {
        Metadata.Validators.Add(
            (in obj, ctx) => validator((TProp?)obj, ctx));

        return this;
    }

    private static PropertyInfo GetProperty(Expression<Func<TModel, TProp>> propertySelector)
    {
        if (propertySelector.Body is not MemberExpression memberExpression)
        {
            throw new ArgumentException("The expression must be a member expression", nameof(propertySelector));
        }

        if (memberExpression.Member is not PropertyInfo propertyInfo)
        {
            throw new ArgumentException("The member expression must refer to a property", nameof(propertySelector));
        }

        return propertyInfo;
    }
}
