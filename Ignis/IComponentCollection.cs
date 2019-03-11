using System;
using System.Collections;
using System.Collections.Generic;

namespace Ignis
{
    internal interface IComponentCollectionInternal<T>
        where T : struct
    {
        bool StoreComponentForEntity(long entityId, T componentValue);
        bool RemoveComponentFromStorage(long entityId);
    }

    public interface IComponentCollection<T> : IEnumerable<T>
        where T : struct
    {
        ref T NextValue();
        bool HasNextValue();
        long GetCurrentEntityId();
    }
}