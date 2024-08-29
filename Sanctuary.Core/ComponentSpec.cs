using System;

namespace Sanctuary;

/// <summary>
/// A component specification. It contains a list of conditions that must be satisfied in order to
/// acquire a component from <see cref="IComponentPool{TComponent}"/>. Components that don't
/// satisfy all conditions are not eligible to be acquired.
/// </summary>
public record ComponentSpec(Type ComponentType);