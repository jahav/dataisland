using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace DataIsland;

/// <summary>
/// A pool that is responsible for providing components of one type for each test. The pool might
/// be static (i.e. only containing specific components) or dynamic (creating components when
/// necessary). The pool is a shared state across all tests and components shouldn't be modified
/// after creation (they might violate <see cref="ComponentSpec{TComponent}"/>).
/// </summary>
/// <typeparam name="TComponent">Type of component being created.</typeparam>
/// <typeparam name="TComponentSpec">Specification with a list of conditions specific for the <typeparamref name="TComponent"/>.</typeparam>
[PublicAPI]
public interface IComponentPool<TComponent, TComponentSpec> : IComponentPool
    where TComponentSpec : ComponentSpec<TComponent>
{
    /// <summary>
    /// Match the requested components with components in the pool and return components that match
    /// requested condition.
    /// </summary>
    /// <param name="requestedComponents">
    /// A list of requested components. The key is a name of component from template and value is
    /// specification of a component.
    /// </param>
    /// <returns>
    /// <para>
    /// A map of resolved components. The keys are component names (same as on the inputs) and
    /// values are component that match the requested <see cref="ComponentSpec{TComponent}"/> from
    /// <paramref name="requestedComponents"/>.
    /// </para>
    /// <para>
    /// All values refer to different components. Even if one component could satisfy two entries,
    /// it can only be used for one and other entry must use a different component.
    /// </para>
    /// <para>
    /// This method must be thread safe.
    /// </para>
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Pool wasn't able to match all <see cref="requestedComponents"/>.
    /// </exception>
    Task<IReadOnlyDictionary<string, TComponent>> AcquireComponentsAsync(IReadOnlyDictionary<string, TComponentSpec> requestedComponents);
}

/// <summary>
/// A pool of one type of components, e.g. <em>SqlServer</em> or <em>Azure Blob Storage</em>.
/// For more info see typed interface <see cref="IComponentPool{TComponent,TComponentSpec}"/>.
/// </summary>
public interface IComponentPool : IAsyncDisposable
{
    /// <summary>
    /// Initializes the pool. Generally called from assembly fixture, before any test are run.
    /// </summary>
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
