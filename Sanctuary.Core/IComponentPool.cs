using System.Collections.Generic;

namespace Sanctuary;

public interface IComponentPool<TComponent>
{
    IReadOnlyDictionary<string, TComponent> AcquireComponents(IReadOnlyDictionary<string, ComponentSpec> requestedComponents);
}