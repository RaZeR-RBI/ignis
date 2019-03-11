using System;
using System.Collections;
using System.Collections.Generic;

namespace Ignis
{
    internal interface IComponentCollectionStorage
    {
        bool RemoveComponentFromStorage(int entityId);
        bool StoreComponentForEntity(int entityId);
    }

    public interface IComponentCollection<T> : IEnumerable<T>, IEnumerator
        where T : struct
    {
        void UpdateCurrent(T value);
        void Update(int entityId, T value);
        void ForEach(Action<int, T> action);
    }
}