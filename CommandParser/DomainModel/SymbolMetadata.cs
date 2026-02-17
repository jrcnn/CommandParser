using CommandParser.Validation;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace CommandParser.DomainModel;

internal abstract class SymbolMetadata
{
    public string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public Type ValueType { get; }
    public PropertyInfo TargetProperty { get; }
    public List<Validator> Validators { get; } = [];

    internal SymbolMetadata(PropertyInfo targetProperty)
    {
        Name = GetDefaultName(targetProperty);
        ValueType = targetProperty.PropertyType;
        TargetProperty = targetProperty;
    }

    protected abstract string GetDefaultName([DisallowNull] PropertyInfo property);
}
