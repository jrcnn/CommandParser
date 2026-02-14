using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;

namespace CommandParser.DomainModel;

internal sealed class OptionMetadata : SymbolMetadata
{
    public HashSet<string> Aliases { get; } = new(StringComparer.Ordinal);
    public bool IsRequired { get; set; } = true;
    public bool HasDefaultValue { get; set; } = false;
    public object? DefaultValue { get; set; } = null;

    public OptionMetadata(PropertyInfo targetProperty)
        : base(targetProperty)
    {
        // special case: if the option is a boolean, we want to default it to false instead of null, and treat it as having a default value
        if (targetProperty.PropertyType == typeof(bool))
        {
            HasDefaultValue = true;
            DefaultValue = false;
            return;
        }

        // nullable types have null as their default value
        NullabilityInfoContext context = new();
        NullabilityInfo nullabilityInfo = context.Create(targetProperty);
        if (nullabilityInfo.ReadState == NullabilityState.Nullable)
        {
            HasDefaultValue = true;
        }
    }

    public bool IsReferredToBy(string reference)
        => StringComparer.Ordinal.Equals(Name, reference) || Aliases.Contains(reference);

    protected override string GetDefaultName([DisallowNull] PropertyInfo targetProperty)
    {
        StringBuilder formattedName = new("--");
        string propertyName = targetProperty.Name;

        // propertyName is guaranteed to be non-empty since it's a valid C# identifier, so we can safely access the first character.
        formattedName.Append(char.ToLower(propertyName[0]));
        for (int i = 1; i < propertyName.Length; ++i)
        {
            char currentChar = propertyName[i];
            if (char.IsUpper(currentChar))
            {
                formattedName.Append('-');
            }
            formattedName.Append(char.ToLower(currentChar));
        }

        return formattedName.ToString();
    }
}
