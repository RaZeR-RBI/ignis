using System;
using System.Collections;
using System.Collections.Generic;

namespace Ignis
{
    internal interface IComponentCollectionStorage
    {
        bool RemoveComponentFromStorage(long entityId);
        bool StoreComponentForEntity(long entityId);
    }

    public interface IComponentCollection<T> : IEnumerable<T>
        where T : struct
    {
        ref T NextValue();
        bool HasNextValue();
        long GetCurrentEntityId();
        void ResetIterator();
    }
}