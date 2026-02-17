using CommandParser.Validation;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace CommandParser.DomainModel;

public class ArgumentBuilder<TModel, TProp>
{
    internal ArgumentMetadata Metadata { get; }

    internal ArgumentBuilder(Expression<Func<TModel, TProp>> propertySelector)
    {
        Metadata = new(GetProperty(propertySelector));
    }

    /// <summary>
    ///     Sets the <paramref name="name"/> for the argument being built.
    /// </summary>
    /// <param name="name">The display name to assign to the argument. Cannot be null.</param>
    /// <returns>The current <see cref="ArgumentBuilder{TModel, TProp}"/> instance for method chaining.</returns>
    public ArgumentBuilder<TModel, TProp> WithName([DisallowNull] string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be null or whitespace", nameof(name));
        }

        Metadata.Name = name;
        return this;
    }

    /// <summary>
    ///     Sets the <paramref name="description"/> metadata for the argument being built.
    /// </summary>
    /// <param name="description">The description text to associate with the argument. Cannot be null.</param>
    /// <returns>The current <see cref="ArgumentBuilder{TModel, TProp}"/> instance for method chaining.</returns>
    public ArgumentBuilder<TModel, TProp> WithDescription([DisallowNull] string description)
    {
        Metadata.Description = description;
        return this;
    }

    /// <summary>
    ///     Sets the validation logic for the argument being built using the provided <paramref name="predicate"/>.
    /// </summary>
    /// <param name="validator">The delegate with the validation logic.</param>
    /// <returns>The current <see cref="ArgumentBuilder{TModel, TProp}"/> instance for method chaining.</returns>
    public ArgumentBuilder<TModel, TProp> WithValidator(Validator<TProp> validator)
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
