using System;
using JetBrains.Annotations;

namespace Sanctuary;

/// <summary>
/// A component specification. It contains a list of conditions that must be satisfied in order to
/// acquire a component from <see cref="IComponentPool{TComponent,TComponentSpec}"/>. Components
/// that don't satisfy all conditions are not eligible to be acquired.
/// </summary>
[PublicAPI]
public abstract record ComponentSpec<TComponent> : IComponentSpec
{
    public Type ComponentType => typeof(TComponent);
};

internal interface IComponentSpec
{
    Type ComponentType { get; }
};
