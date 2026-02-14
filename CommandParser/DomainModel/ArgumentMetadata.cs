using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace CommandParser.DomainModel;

internal sealed class ArgumentMetadata(PropertyInfo targetProperty) : SymbolMetadata(targetProperty)
{
    protected override string GetDefaultName([DisallowNull] PropertyInfo property)
        => property.Name.ToUpper();
}
