using System;

namespace Sanctuary;

public abstract record ComponentSpec<TComponent>() : ComponentSpec(typeof(TComponent));

/// <summary>
/// A component specification. It contains a list of conditions that must be satisfied in order to
/// acquire a component from <see cref="IComponentPool{TComponent,TComponentSpec}"/>. Components
/// that don't satisfy all conditions are not eligible to be acquired.
/// </summary>
public abstract record ComponentSpec(Type ComponentType);