using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Sanctuary;

[PublicAPI]
public interface IComponentPool<TComponent, TComponentSpec>
    where TComponentSpec : ComponentSpec<TComponent>
{
    Task<IReadOnlyDictionary<string, TComponent>> AcquireComponentsAsync(IReadOnlyDictionary<string, TComponentSpec> requestedComponents);
}