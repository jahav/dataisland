using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Xunit.v3;

namespace Sanctuary.xUnit.v3;

[PublicAPI]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class DataSetProfileAttribute(string name) : Attribute, ITraitAttribute
{
    internal static string DefaultProfile { get; } = "DefaultProfile";

    public string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));

    public DataSetProfileAttribute()
        : this("DefaultProfile")
    {
    }

    IReadOnlyCollection<KeyValuePair<string, string>> ITraitAttribute.GetTraits()
    {
        return [new KeyValuePair<string, string>("Profile", Name)];
    }
}