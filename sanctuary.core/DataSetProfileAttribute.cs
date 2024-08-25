using System;
using JetBrains.Annotations;

namespace Sanctuary.Core;

[PublicAPI]
[AttributeUsage(AttributeTargets.Method)]
public class DataSetProfileAttribute : Attribute
{
    public string Name { get; }

    public DataSetProfileAttribute(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    public DataSetProfileAttribute()
        : this("DefaultProfile")
    {
    }
}