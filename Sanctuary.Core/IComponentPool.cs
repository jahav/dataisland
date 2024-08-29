using System;
using System.Collections.Generic;

namespace Sanctuary;

public interface IComponentPool<TComponent>
{
//    /// <summary>
//    /// 
//    /// </summary>
//    /// <param name="componentName"></param>
//    /// <returns></returns>
//    /// <exception cref="InvalidOperationException">When the pool can't retrieve the component, generally due to exhausted resources.</exception>
//    TComponent GetComponent(string componentName);

    IReadOnlyDictionary<string, TComponent> AcquireComponents(IReadOnlyDictionary<string, ComponentSpec> requestedComponents);
}