using System;
using JetBrains.Annotations;

namespace Sanctuary;

/// <summary>
/// A component specification. It contains conditions that must be satisfied in order to
/// acquire a component from <see cref="IComponentPool{TComponent,TComponentSpec}"/>. If pool
/// is not able to find/create a component satisfying all conditions, component is not acquired
/// and pool throws and exception.
/// </summary>
[PublicAPI]
public abstract record ComponentSpec<TComponent> : IComponentSpec
{
    /// <summary>
    /// Component that is described by this spec.
    /// </summary>
    public Type ComponentType => typeof(TComponent);
};

internal interface IComponentSpec
{
    Type ComponentType { get; }
};
